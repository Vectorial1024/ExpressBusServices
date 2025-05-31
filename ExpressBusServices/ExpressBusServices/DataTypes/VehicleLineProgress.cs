using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices.DataTypes
{
    public struct VehicleLineProgress
    {
        public ushort vehicleID;
        public float percentProgress;

        public VehicleLineProgress(ushort vehicleID, float progress)
        {
            this.vehicleID = vehicleID;
            this.percentProgress = progress;
        }

        /// <summary>
        /// Returns the TransportLineProgress of the given transport line for vehicle progress analysis.
        /// </summary>
        /// <param name="transportLineID"></param>
        /// <returns></returns>
        public static TransportLineVehicleProgress GetTransportLineVehicleProgress(ushort transportLineID)
        {
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLineID];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            // assume valid list, so no "exceeded size", etc.
            // we will iterate the list until it reads 0, which indicates "no more vehicles in the line"
            ushort iteratingVehicleID = theLine.m_vehicles;
            List<VehicleLineProgress> progressList = new List<VehicleLineProgress>();
            // StringBuilder builder = new StringBuilder("Vehicle IDs:\n");
            while (iteratingVehicleID != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[iteratingVehicleID].Info;
                info.m_vehicleAI.GetProgressStatus(iteratingVehicleID, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[iteratingVehicleID], out float current, out float max);
                // the bool return is simply to indicate whether the bus is stopping at a stop.
                // (true indicates "is moving", so false indicates "is at stop")
                // not useful right now, but it will be useful later
                if (max != 0)
                {
                    // a valid bus; invalid bus (eg is despawning) will get max = 0
                    VehicleLineProgress progress = new VehicleLineProgress(iteratingVehicleID, current / max);
                    progressList.Add(progress);
                    // builder.AppendLine(vehicleIterator.ToString());
                }
                iteratingVehicleID = instance.m_vehicles.m_buffer[iteratingVehicleID].m_nextLineVehicle;
            }
            // all vehicles found
            // give to dedicated object
            return new TransportLineVehicleProgress(progressList);
        }
    }
}
