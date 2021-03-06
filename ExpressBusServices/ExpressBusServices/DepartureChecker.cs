﻿using System;
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
            ushort firstStop = instance.m_lines.m_buffer[transportLineID].GetStop(0);
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);

            // note:
            // if I somehow cannot determine where I am at or where the first stop is at, always unbunch.
            // this may be related to some random errors that I get about "vehicles not leaving stop",
            // where perhaps some vehicle states messed up.
            return currentStop == 0 || firstStop == 0 || currentStop != firstStop;
        }
    }
}
