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
            TransportManager instance = Singleton<TransportManager>.instance;
            ushort transportLineID = vehicleData.m_transportLine;
            // the semantics! they must be clear! dont try to cheat with "first is index=1 and current=next" again! it hurts devops!
            // paradoxically, the game stores the first stop as "the last stop", drawing reference how the "first stop" is selected again when the line is completed
            ushort firstStop = instance.m_lines.m_buffer[transportLineID].GetLastStop();
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);

            // note:
            // if I somehow cannot determine where I am at or where the first stop is at, always unbunch.
            // this may be related to some random errors that I get about "vehicles not leaving stop",
            // where perhaps some vehicle states messed up.
            return currentStop == 0 || firstStop == 0 || !StopIsConsideredAsTerminus(currentStop, transportLineID);
        }

        // if true, then this mod will instruct buses to skip the stop
        // this looks very similar to the above, except that the numbers are adjusted for correctness.
        public static bool CanSkipNextStop(ushort vehicleID, ref Vehicle vehicleData)
        {
            TransportManager transportManager = Singleton<TransportManager>.instance;
            ushort transportLineID = vehicleData.m_transportLine;
            // the semantics! they must be clear! dont try to cheat with "first is index=1 and current=next" again! it hurts devops!
            // paradoxically, the game stores the first stop as "the last stop", drawing reference how the "first stop" is selected again when the line is completed
            ushort firstStop = transportManager.m_lines.m_buffer[transportLineID].GetLastStop();
            ushort approachingStop = vehicleData.m_targetBuilding;

            // note:
            // if I somehow cannot determine where I am at or where the first stop is at, always unbunch.
            // this may be related to some random errors that I get about "vehicles not leaving stop",
            // where perhaps some vehicle states messed up.
            return approachingStop == 0 || firstStop == 0 || !StopIsConsideredAsTerminus(approachingStop, transportLineID);
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
            return stopID != firstStopID;
        }
    }
}
