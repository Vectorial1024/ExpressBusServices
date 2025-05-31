using ExpressBusServices.DataTypes;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_BusLoadsPassengers
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [UsedImplicitly]
        public static bool HandleBusAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            VehiclePaxDeltaInfo.Notify_VehicleStartsLoadingPax(vehicleID, ref data);
            if (BusStopSkippingLookupTable.BusShouldSkipPassengerLoading(vehicleID))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        public static void HandleBusAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusStopSkippingLookupTable.ForgetBus(vehicleID);
            VehiclePaxDeltaInfo.Notify_VehicleFinishedLoadingPax(vehicleID, ref data);
        }
    }
}
