using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices
{
    public class BusDepotRedeploymentInstructions
    {
        private static Dictionary<ushort, List<ushort>> transportLineDepotInstructions;

        public static void EnsureTableExists()
        {
            if (transportLineDepotInstructions == null)
            {
                transportLineDepotInstructions = new Dictionary<ushort, List<ushort>>();
            }
        }

        public static void WipeTable() => transportLineDepotInstructions?.Clear();

        public static void NotifyTransportLineAddFutureDeployment(ushort transportLineID, ushort targetStopID)
        {
            if (!transportLineDepotInstructions.ContainsKey(transportLineID))
            {
                transportLineDepotInstructions[transportLineID] = new List<ushort>();
            }
            transportLineDepotInstructions[transportLineID].Add(targetStopID);
        }

        public static bool TransportLineReadFutureDeployment(ushort transportLineID, out ushort targetStopID)
        {
            targetStopID = 0;
            if (!transportLineDepotInstructions.ContainsKey(transportLineID))
            {
                return false;
            }
            List<ushort> pendingInstructions = transportLineDepotInstructions[transportLineID];
            if (pendingInstructions.Count == 0)
            {
                return false;
            }
            targetStopID = pendingInstructions.First();
            pendingInstructions.RemoveAt(0);
            if (pendingInstructions.Count == 0)
            {
                transportLineDepotInstructions.Remove(transportLineID);
            }
            // verify that the stop ID is valid; it could be possible that the user removed the bus stop while we are waiting for the future instructions
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLineID];
            // we just need to ensure that the stop ID is in the line
            ushort startingStopID = theLine.GetStop(0);
            ushort loopingStopID = TransportLine.GetNextStop(startingStopID);
            int iterateCount = 0;
            while (loopingStopID != startingStopID)
            {
                if (loopingStopID == targetStopID)
                {
                    return true;
                }
                loopingStopID = TransportLine.GetNextStop(loopingStopID);
                if (++iterateCount >= 32768)
                {
                    // invalid list, yada yada
                    break;
                }
            }
            return false;
        }
    }
}
