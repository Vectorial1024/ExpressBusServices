﻿using System.Collections.Generic;
using UnityEngine;

namespace ExpressBusServices
{
    public class ServiceBalancerUtil
    {
        private static Dictionary<ushort, ushort> redeploymentInstructions = new Dictionary<ushort, ushort>();

        private static Dictionary<ushort, ushort> vehicleCurrentlyAtStop = new Dictionary<ushort, ushort>();

        private static Dictionary<ushort, bool> redeploymentToTerminus= new Dictionary<ushort, bool>();

        private static readonly int STANDARD_BUS_PAX_THRESHOLD = 30;

        private struct TransportLineSegmentAnalysis
        {
            public ushort leadingTerminusStopId;
            public int stopCount;
            public int paxCount;

            public ushort mostWaitingPaxStopId;
            public int mostWaitingPaxCount;

            public bool segmentCanReceiveRedeployment;

            public TransportLineSegmentAnalysis(ushort leadingTerminusStopId, int stopCount, int paxCount, bool segmentCanReceiveRedeployment = false)
            {
                this.leadingTerminusStopId = leadingTerminusStopId;
                this.stopCount = stopCount;
                this.paxCount = paxCount;
                this.mostWaitingPaxStopId = 0;
                this.mostWaitingPaxCount = 0;
                this.segmentCanReceiveRedeployment = segmentCanReceiveRedeployment;
            }

            public void CompareAndUpdateMostWaitingStop(ushort stopID, int waitingPax)
            {
                if (waitingPax > mostWaitingPaxCount)
                {
                    mostWaitingPaxStopId = stopID;
                    mostWaitingPaxCount = waitingPax;
                }
            }
        }

        public static bool FindRedeployToTerminus(ushort vehicleID, ushort transortLineID, ushort currentTerminusStopId, out ushort terminusStopId)
        {
            terminusStopId = 0;
            if (!EBSModConfig.UseServiceSelfBalancing)
            {
                // option not enabled; skip everything!
                MarkIsRedeployingToTerminus(vehicleID, false);
                return false;
            }
            if (!DepartureChecker.StopIsConsideredAsTerminus(currentTerminusStopId, transortLineID))
            {
                // not a terminus; check not allowed!
                MarkIsRedeployingToTerminus(vehicleID, false);
                return false;
            }
            List<TransportLineSegmentAnalysis> analysisList = AnalyzeTransportLinePopularity(transortLineID, currentTerminusStopId);
            if (analysisList.Count < 2)
            {
                // less than 2 segments, this means it is circular, and is not eligible for super-skipping
                MarkIsRedeployingToTerminus(vehicleID, false);
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
            if (OddsPermitRedeployment(selfAvePax, otherAvePaxList, vehicleID))
            {
                // check against the many segments, and determine which one to go to
                float nextInRangeRandNum = UnityEngine.Random.Range(0, summedTotal);
                float currentCapValue = 0;
                ushort loopingTerminusStopId = 0;
                ushort loopingMiddleStopId = 0;
                int loopingMiddleStopPaxCount = 0;
                skipNext = true;
                bool isRedeployingToTerminus = false;
                for (int i = 0; i < acceptedList.Count; i++)
                {
                    TransportLineSegmentAnalysis analysis = acceptedList[i];
                    loopingTerminusStopId = analysis.leadingTerminusStopId;
                    loopingMiddleStopId = analysis.mostWaitingPaxStopId;
                    loopingMiddleStopPaxCount = analysis.mostWaitingPaxCount;
                    currentCapValue += otherAvePaxList[i];
                    if (nextInRangeRandNum < currentCapValue)
                    {
                        // in this current segment
                        break;
                    }
                }
                // pick random 50% chance that it will go to a middle stop with the most passengers
                if (EBSModConfig.ServiceSelfBalancingCanDoMiddleStop && UnityEngine.Random.value < 0.5f && loopingMiddleStopPaxCount > STANDARD_BUS_PAX_THRESHOLD)
                {
                    // must have enough pax waiting
                    // deploy to middle bus stop
                    terminusStopId = loopingMiddleStopId;
                }
                else
                {
                    // deploy to terminus
                    terminusStopId = loopingTerminusStopId;
                    isRedeployingToTerminus = true;
                }
                // Debug.Log("EBS determines that a bus needs to be redeployed: " + currentTerminusStopId + " -> " + terminusStopId);
                MarkIsRedeployingToTerminus(vehicleID, isRedeployingToTerminus);
                return true;
            }
            MarkIsRedeployingToTerminus(vehicleID, false);
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
            TransportLineSegmentAnalysis analysis = new TransportLineSegmentAnalysis(startingTerminusStopId, 1, paxCount[startingTerminusStopId], paxCount[startingTerminusStopId] > STANDARD_BUS_PAX_THRESHOLD);
            analysis.CompareAndUpdateMostWaitingStop(startingTerminusStopId, paxCount[startingTerminusStopId]);
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
                analysis.CompareAndUpdateMostWaitingStop(loopingStopID, paxCount[loopingStopID]);
                analysis.segmentCanReceiveRedeployment |= paxCount[loopingStopID] > STANDARD_BUS_PAX_THRESHOLD;
                // move to next
                loopingStopID = nextStopLink[loopingStopID];
                // Debug.Log("Analyze gouping loop.");
            }

            // return the list
            return analysisList;
        }

        private static bool OddsPermitRedeployment(float selfAvePaxCount, List<float> otherAvePaxCountList, ushort vehicleID)
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
                // Debug.Log("Redeployment true probability (hard) " + 0 + " -> " + 999);
                return true;
            }
            // the odds of moving to any of the candidate segments
            float oddsMove = properOtherValue / properSelfValue;
            // we need to convert a exponential [0, inf) to a logistical [0, 1)
            // we will use a simple exponential fraction function to convert things
            // and the converted value can be directly used for RNG
            float probability = 1 - 1 / (Mathf.Pow(2, oddsMove - 1));
            // Debug.Log("Redeployment probability " + oddsMove + " -> " + probability);
            if (probability < 0)
            {
                return false;
            }
            // finally, do a RNG with such probability * the global config prob value
            // todo read from a config, or not
            float globalBalancerProbability = 0.5f;
            float theProbability = probability * globalBalancerProbability;
            // further reduce probability of repeated redeployment
            if (VehicleIsRedeployingToTerminus(vehicleID))
            {
                theProbability *= 0.5f;
            }
            // Random.value gives a PseudoUniform(0, 1) random value
            float rngPick = UnityEngine.Random.value;
            // Debug.Log("Redeployment true probability " + rngPick + " -> " + theProbability);
            return rngPick <= theProbability;
        }

        public static void MarkRedeployToNewTerminus(ushort vehicleID, ushort targetStopId)
        {
            // mark it here, so that later we can correctly apply this
            redeploymentInstructions[vehicleID] = targetStopId;
        }

        public static bool ReadRedeploymentInstructions(ushort vehicleID, out ushort redeploymentTarget, bool removeEntry = false)
        {
            redeploymentTarget = 0;
            if (!redeploymentInstructions.ContainsKey(vehicleID))
            {
                // no instructions
                return false;
            }
            redeploymentTarget = redeploymentInstructions[vehicleID];
            if (removeEntry)
            {
                redeploymentInstructions.Remove(vehicleID);
            }
            return true;
        }

        public static void MarkVehicleIsAtStopId(ushort vehicleID, ushort stopId)
        {
            vehicleCurrentlyAtStop[vehicleID] = stopId;
        }

        public static void MarkIsRedeployingToTerminus(ushort vehicleID, bool flag)
        {
            redeploymentToTerminus[vehicleID] = flag;
        }

        public static bool VehicleIsRedeployingToTerminus(ushort vehicleID)
        {
            if (!redeploymentToTerminus.ContainsKey(vehicleID))
            {
                return false;
            }
            return redeploymentToTerminus[vehicleID];
        }

        public static bool ReadVehicleCurrentlyAtWhatStop(ushort vehicleID, out ushort stopId)
        {
            stopId = 0;
            if (!vehicleCurrentlyAtStop.ContainsKey(vehicleID))
            {
                return false;
            }
            stopId = vehicleCurrentlyAtStop[vehicleID];
            return true;
        }

        public static bool PopNeedsRedeployToTerminus()
        {
            return false;
        }

        public static void EnsureTableExists()
        {
            // reset the dictionary or whatever data struct we decided to use
            if (redeploymentInstructions == null)
            {
                redeploymentInstructions = new Dictionary<ushort, ushort>();
            }
            if (vehicleCurrentlyAtStop == null)
            {
                vehicleCurrentlyAtStop = new Dictionary<ushort, ushort>();
            }
            if (redeploymentToTerminus == null)
            {
                redeploymentToTerminus = new Dictionary<ushort, bool>();
            }
        }

        public static void ResetRedeploymentRecords()
        {
            // reset the dictionary or whatever data struct we decided to use
            redeploymentInstructions.Clear();
            vehicleCurrentlyAtStop.Clear();
            redeploymentToTerminus.Clear();
        }
    }
}
