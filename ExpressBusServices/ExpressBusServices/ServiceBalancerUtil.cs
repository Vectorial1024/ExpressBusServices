using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices
{
    public class ServiceBalancerUtil
    {
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

        public static bool FindRedeployToTerminus(out ushort terminusStopId)
        {
            AnalyzeTransportLinePopularity();
            terminusStopId = 0;
            if (OddsPermitRedeployment())
            {
                MarkRedeployToNewTerminus();
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
                    analysis = new TransportLineSegmentAnalysis(startingTerminusStopId, 0, 0, false);
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
                loopingStopID = nextStopLink[startingTerminusStopId];
            }

            // return the list
            return analysisList;
        }

        private static bool OddsPermitRedeployment()
        {
            // using the analysis result, performs calculation and determines whether redeployment is allowed
            /*
             * var segmentCount;
             * var currentSegmentWaitingPax;
             */
            return false;
        }

        private static void MarkRedeployToNewTerminus()
        {
            //
        }

        public static bool PopNeedsRedeployToTerminus()
        {
            return false;
        }

        public static void ResetRedeploymentRecords()
        {
            // reset the dictionary or whatever data struct we decided to use
        }
    }
}
