using ColossalFramework;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TramAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_TramLoadsPassengers
    {
        // we will have to utilize the bus lookup table... it is just very strange

        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        public static void HandleTramAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_leadingVehicle != 0)
            {
                return;
            }

            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            ushort currentVehicleID = vehicleID;
            int iterationCount = 0;
            while (currentVehicleID != 0)
            {
                Vehicle trailerData = vehicleManager.m_vehicles.m_buffer[currentVehicleID];
                BusPickDropLookupTable.Notify_PassengersAboutToBoardOntoBus(currentVehicleID, ref trailerData);
                // move to next trailer
                currentVehicleID = vehicleManager.m_vehicles.m_buffer[currentVehicleID].m_trailingVehicle;
                if (++iterationCount > 16384)
                {
                    // invalid list yada yada
                    break;
                }
            }
        }

        [HarmonyPostfix]
        public static void HandleTramAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop)
        {
            if (data.m_leadingVehicle != 0)
            {
                return;
            }

            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            ushort currentVehicleID = vehicleID;
            int iterationCount = 0;
            while (currentVehicleID != 0)
            {
                Vehicle trailerData = vehicleManager.m_vehicles.m_buffer[currentVehicleID];
                BusPickDropLookupTable.Notify_PassengersAlreadyBoardedOntoBus(currentVehicleID, ref trailerData);
                // move to next trailer
                currentVehicleID = vehicleManager.m_vehicles.m_buffer[currentVehicleID].m_trailingVehicle;
                if (++iterationCount > 16384)
                {
                    // invalid list yada yada
                    break;
                }
            }
        }
    }
}
