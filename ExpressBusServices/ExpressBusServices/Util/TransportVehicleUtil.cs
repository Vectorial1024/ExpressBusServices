using ColossalFramework;
using ExpressBusServices.DataTypes;
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
            if (data.m_transportLine == 0)
            {
                // no op
                return;
            }
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine];
            theLine.RemoveVehicle(vehicleID, ref data);
            data.Info.m_vehicleAI.SetTransportLine(vehicleID, ref data, 0);
        }

        public static bool VehicleHasProgressPercent(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_transportLine == 0)
            {
                return false;
            }
            List<VehicleLineProgress> progressList = VehicleLineProgress.GetProgressList(data.m_transportLine);
            // find where exists self
            return progressList.Where(item => item.vehicleID == vehicleID).ToList().Count > 0;
        }
    }
}
