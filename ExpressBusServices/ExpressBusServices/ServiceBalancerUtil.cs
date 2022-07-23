using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExpressBusServices
{
    public class ServiceBalancerUtil
    {
        private static Dictionary<ushort, ushort> redeploymentInstructions = new Dictionary<ushort, ushort>();

        private struct TransportLineSegmentAnalysis
        {
            public ushort leadingTerminusStopId;
            public int stopCount;
            public int paxCount;

            public bool segmentCanReceiveRedeployment;

            public TransportLineSegmentAnalysis(ushort leadingTerminusStopId, int stopCount, int paxCount, bool segmentCanReceiveRedeployment = false)
            {
                this.leadingTerminusStopId = leadingTerminusStopId;
                this.stopCount = stopCount;
                this.paxCount = paxCount;
                this.segmentCanReceiveRedeployment = segmentCanReceiveRedeployment;
            }
        }

        public static bool FindRedeployToTerminus(ushort transortLineID, ushort currentTerminusStopId, out ushort terminusStopId)
        {
            List<TransportLineSegmentAnalysis> analysisList = AnalyzeTransportLinePopularity(transortLineID, currentTerminusStopId);
            terminusStopId = 0;
            if (analysisList.Count < 2)
            {
                // less than 2 segments, this means it is circular, and is not eligible for super-skipping
                return false;
            }
            // calculate the average number of pax waiting so that can determine the odds
            TransportLineSegmentAnalysis selfAnalysis = analysisList[0];
            float selfAvePax = selfAnalysis.paxCount * 1.0f / selfAnalysis.stopCount;
            List<float> otherAvePaxList = new List<float>();
            List<TransportLineSegmentAnalysis> acceptedList = new List<TransportLineSegmentAnalysis>(); 
            bool skipNext = true;
            float summedTotal = 0;
            foreach (TransportLineSegmentAnalysis analysis in analysisList)
            {
                if (skipNext)
                {
                    // this is just to skip the 1st stop
                    skipNext = false;
                    continue;
                }
                if (!analysis.segmentCanReceiveRedeployment)
                {
                    // non-self segment and cannot receive redeployment
                    // we only want to see segments that can receive redeployment
                    continue;
                }
                float avePax = analysis.paxCount * 1.0f / analysis.stopCount;
                otherAvePaxList.Add(avePax);
                acceptedList.Add(analysis);
                summedTotal += avePax;
            }
            if (OddsPermitRedeployment(selfAvePax, otherAvePaxList))
            {
                // check against the many segments, and determine which one to go to
                float nextInRangeRandNum = UnityEngine.Random.Range(0, summedTotal);
                float currentCapValue = 0;
                ushort loopingTerminusStopId = 0;
                skipNext = true;
                for (int i = 0; i < acceptedList.Count; i++)
                {
                    TransportLineSegmentAnalysis analysis = acceptedList[i];
                    loopingTerminusStopId = analysis.leadingTerminusStopId;
                    currentCapValue += otherAvePaxList[i];
                    if (nextInRangeRandNum < currentCapValue)
                    {
                        // in this current segment
                        break;
                    }
                }
                terminusStopId = loopingTerminusStopId;
                Debug.Log("EBS determines that a bus needs to be redeployed: " + currentTerminusStopId + " -> " + terminusStopId);
                return true;
            }
            return false;
        }

        private static List<TransportLineSegmentAnalysis> AnalyzeTransportLinePopularity(ushort transportLineID, ushort startingTerminusStopId)
        {
            // checks segment terminus -> segment total waiting pax
            // the first item of the list is guaranteed to be the "current segment"
            ushort loopingStopID = startingTerminusStopId;
            Dictionary<ushort, int> paxCount = new Dictionary<ushort, int>();
            Dictionary<ushort, bool> terminusCheck = new Dictionary<ushort, bool>();
            Dictionary<ushort, ushort> nextStopLink = new Dictionary<ushort, ushort>();
            while (true)
            {
                if (paxCount.ContainsKey(loopingStopID))
                {
                    // we looped back to the start
                    break;
                }

                // check waiting passengers
                int residentsWaiting, touristsWaiting;
                TransportLineUtil.CountPassengersWaiting(loopingStopID, out residentsWaiting, out touristsWaiting);
                paxCount.Add(loopingStopID, residentsWaiting + touristsWaiting);

                // check is terminus
                bool isTerminus = DepartureChecker.StopIsConsideredAsTerminus(loopingStopID, transportLineID);
                if (isTerminus)
                {
                    terminusCheck.Add(loopingStopID, isTerminus);
                }

                // next stop
                ushort nextStopId = TransportLine.GetNextStop(loopingStopID);
                nextStopLink.Add(loopingStopID, nextStopId);
                loopingStopID = nextStopId;

                // Debug.Log("Analyze iterating loop.");
            }

            // all information obtained; we are at the first stop of the line
            // create the list
            List<TransportLineSegmentAnalysis> analysisList = new List<TransportLineSegmentAnalysis>();
            TransportLineSegmentAnalysis analysis = new TransportLineSegmentAnalysis(startingTerminusStopId, 1, paxCount[startingTerminusStopId], paxCount[startingTerminusStopId] > 30);
            loopingStopID = nextStopLink[startingTerminusStopId];
            while (true)
            {
                if (terminusCheck.ContainsKey(loopingStopID))
                {
                    analysisList.Add(analysis);
                    analysis = new TransportLineSegmentAnalysis(loopingStopID, 0, 0, false);
                }
                if (loopingStopID == startingTerminusStopId)
                {
                    // we got to the start again
                    break;
                }
                // add info
                analysis.stopCount++;
                analysis.paxCount += paxCount[loopingStopID];
                analysis.segmentCanReceiveRedeployment |= paxCount[loopingStopID] > 30;
                // move to next
                loopingStopID = nextStopLink[loopingStopID];
                // Debug.Log("Analyze gouping loop.");
            }

            // return the list
            return analysisList;
        }

        private static bool OddsPermitRedeployment(float selfAvePaxCount, List<float> otherAvePaxCountList)
        {
            // using the analysis result, performs calculation and determines whether redeployment is allowed
            if (otherAvePaxCountList.Count == 0)
            {
                // no one to transfer to
                return false;
            }
            float properSelfValue = selfAvePaxCount * otherAvePaxCountList.Count;
            float properOtherValue = 0;
            foreach (float otherValue in otherAvePaxCountList)
            {
                properOtherValue += otherValue;
            }
            if (properOtherValue < properSelfValue)
            {
                // generally a better idea to stay at the current segment
                return false;
            }
            if (selfAvePaxCount == 0)
            {
                // to avoid div0 and because of sensibility, we will permit this
                return true;
            }
            // the odds of moving to any of the candidate segments
            float oddsMove = properOtherValue / properSelfValue;
            // we need to convert a exponential [0, inf) to a logistical [0, 1)
            // we will use a simple exponential fraction function to convert things
            // and the converted value can be directly used for RNG
            float probability = 1 - 1 / (Mathf.Pow(2, oddsMove));
            Debug.Log("Redeployment probability " + oddsMove + " -> " + probability);
            if (probability < 0)
            {
                return false;
            }
            // finally, do a RNG with such probability * the global config prob value
            // todo read from a config
            float globalBalancerProbability = 0.5f;
            float theProbability = probability * globalBalancerProbability;
            return UnityEngine.Random.Range(0, 1) <= theProbability;
        }

        public static void MarkRedeployToNewTerminus(VehicleAI aiInstance, ushort vehicleID, ref Vehicle vehicleData, ushort currentStopId, ushort targetStopId)
        {
            // mark it here, so that later we can correctly apply this
            redeploymentInstructions[vehicleID] = targetStopId;
        }

        public static bool PopRedeploymentInstructions(ushort vehicleID, out ushort redeploymentTarget)
        {
            redeploymentTarget = 0;
            if (!redeploymentInstructions.ContainsKey(vehicleID))
            {
                // no instructions
                return false;
            }
            redeploymentTarget = redeploymentInstructions[vehicleID];
            redeploymentInstructions.Remove(vehicleID);
            return true;
        }

        public static bool PopNeedsRedeployToTerminus()
        {
            return false;
        }

        public static void ResetRedeploymentRecords()
        {
            // reset the dictionary or whatever data struct we decided to use
            redeploymentInstructions.Clear();
        }
    }
}
