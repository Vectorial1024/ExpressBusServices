using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RenderManager;

namespace ExpressBusServices
{
    public class VehicleUtil
    {
        public static int GetMaxCarryingCapacityOfTrain(ushort vehicleID, ref Vehicle data)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            // look self
            int totalCapacity = GetCarryingCapacityOfThisVehicle(vehicleID, ref data);
            ushort iteratorVehicleID = data.m_leadingVehicle;
            // look forward
            int iterateCount = 0;
            while (iteratorVehicleID != 0)
            {
                Vehicle localVehicle = instance.m_vehicles.m_buffer[iteratorVehicleID];
                totalCapacity += GetCarryingCapacityOfThisVehicle(vehicleID, ref localVehicle);
                iteratorVehicleID = localVehicle.m_leadingVehicle;
                iterateCount++;
                if (iterateCount >= instance.m_vehicles.m_size)
                {
                    // invalid
                    break;
                }
            }
            // reset
            iterateCount = 0;
            iteratorVehicleID = data.m_trailingVehicle;
            // look backward
            while (iteratorVehicleID != 0)
            {
                Vehicle localVehicle = instance.m_vehicles.m_buffer[iteratorVehicleID];
                totalCapacity += GetCarryingCapacityOfThisVehicle(vehicleID, ref localVehicle);
                iteratorVehicleID = localVehicle.m_trailingVehicle;
                if (iterateCount >= instance.m_vehicles.m_size)
                {
                    // invalid
                    break;
                }
            }
            // summation complete
            return totalCapacity;
        }

        private static int GetCarryingCapacityOfThisVehicle(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            int count = 0;
            while (citizenUnitIndex != 0)
            {
                count++;
                citizenUnitIndex = instance.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
            }
            return count * 5;
        }
    }
}
