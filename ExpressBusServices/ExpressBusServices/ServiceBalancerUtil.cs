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

        private static void AnalyzeTransportLinePopularity()
        {
            // checks segment terminus -> segment total waiting pax
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
