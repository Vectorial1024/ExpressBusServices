using ExpressBusServices.DataTypes;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices.Patches.Tram
{
    [HarmonyPatch(typeof(TramAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_TramLoadsPassengers
    {
        /*
         * important note:
         * to simplify the code for trams and unify the logic, now all tram cars individually register their passenger status
         * decision to instant-depart is delegated to the first car of the tram, which then scans its trailers to collect whole-tram stats.
         */

        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [UsedImplicitly]
        public static bool HandleTramAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
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
        public static void HandleTramAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop)
        {
            BusStopSkippingLookupTable.ForgetBus(vehicleID);
            VehiclePaxDeltaInfo.Notify_VehicleFinishedLoadingPax(vehicleID, ref data);
        }
    }
}
