using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;

namespace ExpressBusServices
{
    public class DepartureChecker
    {
        // if true, then this mod will intervene in handling instant departures etc.
        public static bool NowIsEligibleForInstantDeparture(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (BusIsIntercityBus(vehicleData))
            {
                // always no for intercity buses
                return false;
            }

            // todo check some global flag to instant depart for select buses
            if (ServiceBalancerUtil.ReadRedeploymentInstructions(vehicleID, out _))
            {
                // we have redeployment instructions, let's do it
                return true;
            }

            ushort transportLineID = vehicleData.m_transportLine;
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);

            return !StopIsConsideredAsTerminus(currentStop, transportLineID);
        }

        // if true, then this mod will instruct buses to skip the stop
        // this looks very similar to the above, except that the numbers are adjusted for correctness.
        public static bool CanSkipNextStop(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (BusIsIntercityBus(vehicleData))
            {
                // always no for intercity buses
                return false;
            }

            ushort transportLineID = vehicleData.m_transportLine;
            ushort approachingStop = vehicleData.m_targetBuilding;

            return !StopIsConsideredAsTerminus(approachingStop, transportLineID);
        }

        [Obsolete("Pleasse refactor to use StopIsConsideredTerminus instead.")]
        public static bool StopIsTerminus(ushort stop)
        {
            // none are terminus; however this will be overridden by the TLM extension mod
            return false;
        }

        public static bool StopIsConsideredAsTerminus(ushort stopID, ushort transportLineID)
        {
            // both IPT2 and TLM will override this, to make the things more streamlined
            // return true if this should be considered a terminus
            // buses will unbunch when at terminus

            TransportManager transportManager = Singleton<TransportManager>.instance;
            ushort firstStopID = transportManager.m_lines.m_buffer[transportLineID].GetLastStop();

            // architecture reversal
            bool isTerminus = firstStopID != 0 && stopID != 0 && stopID == firstStopID;
            if (ReversePatch_TLMPlugin_StopIsTerminus.PatchIsSuccessful_HasTLM)
            {
                isTerminus |= ReversePatch_TLMPlugin_StopIsTerminus.StopIsConsideredTerminus(stopID);
            }
            return isTerminus;
        }

        public static bool BusIsIntercityBus(Vehicle vehicleData)
        {
            ItemClass itemClass = vehicleData.Info.m_class;
            return TransportStationAI.IsIntercity(itemClass);
        }

        public static bool RecheckUnbunchingCanLeave(ushort vehicleID, ref Vehicle vehicleData)
        {
            // mainly for IPT2; there is probably some side effect that is caused by how the IPT2 plugin is influencing the work of IPT2 itself
            // this aims to remedy that.
            // this is also a place to further extend the unbunching checking, so that we can implement the so-called rapid deployment feature.
            bool canLeave = Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].CanLeaveStop(vehicleData.m_targetBuilding, vehicleData.m_waitCounter >> 4);
            // todo recalculate with a different waiting time when the budget is not at 100%.
            return canLeave;
        }
    }
}
