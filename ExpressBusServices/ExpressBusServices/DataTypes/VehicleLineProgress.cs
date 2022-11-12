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
        /// Gets the sorted-by-progress (asc) list of vehicle progress in the line.
        /// Then, you can search within the line to find the vehicle's progress among other vehicles in the same line.
        /// </summary>
        /// <param name="transportLineID"></param>
        /// <returns></returns>
        public static List<VehicleLineProgress> GetProgressList(ushort transportLineID)
        {
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLineID];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleIterator = theLine.m_vehicles;
            List<VehicleLineProgress> progressList = new List<VehicleLineProgress>();
            // StringBuilder builder = new StringBuilder("Vehicle IDs:\n");
            float current, max;
            while (vehicleIterator != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleIterator].Info;
                info.m_vehicleAI.GetProgressStatus(vehicleIterator, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleIterator], out current, out max);
                // the bool return is simply to indicate whether the bus is stopping at a stop.
                // for us, this is still useful.
                if (max != 0)
                {
                    // a valid bus; invalid bus (eg is despawning) will get max = 0
                    VehicleLineProgress progress = new VehicleLineProgress(vehicleIterator, current / max);
                    progressList.Add(progress);
                    // builder.AppendLine(vehicleIterator.ToString());
                }
                vehicleIterator = instance.m_vehicles.m_buffer[vehicleIterator].m_nextLineVehicle;
            }
            // all vehicles found
            // sort the list for in-order progress checking
            progressList.Sort(delegate (VehicleLineProgress left, VehicleLineProgress right)
            {
                // sort by the percentage progress
                return left.percentProgress.CompareTo(right.percentProgress);
            });

            // list is generated
            return progressList;
        }
    }
}
