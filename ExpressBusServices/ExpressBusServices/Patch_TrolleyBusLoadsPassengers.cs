using ExpressBusServices.DataTypes;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_TrolleyBusLoadsPassengers
    {
        [HarmonyPrefix]
        public static bool HandleBusAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAboutToBoardOntoBus(vehicleID, ref data);
            VehiclePaxDeltaInfo.Notify_VehicleStartsLoadingPax(vehicleID, ref data);
            if (BusStopSkippingLookupTable.BusShouldSkipPassengerLoading(vehicleID))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void HandleBusAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAlreadyBoardedOntoBus(vehicleID, ref data);
            VehiclePaxDeltaInfo.Notify_VehicleFinishedLoadingPax(vehicleID, ref data);
        }
    }
}
