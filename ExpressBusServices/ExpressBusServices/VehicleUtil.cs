using ColossalFramework;
using ExpressBusServices.PerformanceBoost;

namespace ExpressBusServices
{
    public static class VehicleUtil
    {
        public static int GetMaxCarryingCapacityOfTrain(ushort vehicleID, ref Vehicle data)
        {
            // try to get from cache
            CachedVehicleProperties props = CachedVehicleProperties.GetFromCache(vehicleID);
            if (props != null)
            {
                if (props.TrainCapacity.HasValue)
                {
                    return props.TrainCapacity.Value;
                }
            }

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
            // also save to the cache
            if (props == null)
            {
                props = new CachedVehicleProperties
                {
                    TrainCapacity = totalCapacity
                };
            } else
            {
                props.TrainCapacity = totalCapacity;
            }
            CachedVehicleProperties.SetToCache(vehicleID, props);
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

        public static bool IsEveryoneAboardTheTrain(ushort vehicleID, ref Vehicle data)
        {
            // VehicleAI has CanLeave: true when all pax are inside the vehicle.
            // we just call this for all vehicles of the train
            // assume first vehicle ID

            if (!ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref data))
            {
                return false;
            }

            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort currentVehicleID = data.m_trailingVehicle;
            while (currentVehicleID != 0)
            {
                ref Vehicle currentData = ref instance.m_vehicles.m_buffer[currentVehicleID];
                if (!ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, currentVehicleID, ref currentData))
                {
                    return false;
                }
                // check next
                currentVehicleID = currentData.m_trailingVehicle;
            }

            // all vehicles in train has pax
            return true;
        }
    }
}
