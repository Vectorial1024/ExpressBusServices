using ColossalFramework;
using ExpressBusServices.DataTypes;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TramAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_TramLoadsPassengers
    {
        /*
         * important note:
         * to simplify the code for trams and unify the logic, now all tram cars individually register their passenger status
         * decision to instant-depart is delegated to the first car of the tram, which then scans its trailers to collect whole-tram stats. 
         */

        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        public static void HandleTramAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            VehiclePaxDeltaInfo.Notify_VehicleStartsLoadingPax(vehicleID, ref data);
            if (data.m_leadingVehicle != 0)
            {
                return;
            }
            TramPickDropLookupTable.ForgetTram(vehicleID);

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
            VehiclePaxDeltaInfo.Notify_VehicleFinishedLoadingPax(vehicleID, ref data);
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
