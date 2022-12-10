using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices.Util
{
    public class TransportVehicleUtil
    {
        public static void TellVehicleToReturnToBase(ushort vehicleID, ref Vehicle data)
        {
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine];
            theLine.RemoveVehicle(vehicleID, ref data);
            data.Info.m_vehicleAI.SetTransportLine(vehicleID, ref data, 0);
        }
    }
}
