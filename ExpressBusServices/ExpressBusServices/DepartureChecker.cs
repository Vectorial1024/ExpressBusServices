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
            ushort transportLineID = vehicleData.m_transportLine;
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);

            return !StopIsConsideredAsTerminus(currentStop, transportLineID);
        }

        // if true, then this mod will instruct buses to skip the stop
        // this looks very similar to the above, except that the numbers are adjusted for correctness.
        public static bool CanSkipNextStop(ushort vehicleID, ref Vehicle vehicleData)
        {
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
            return firstStopID != 0 && stopID != 0 && stopID == firstStopID;
        }
    }
}
