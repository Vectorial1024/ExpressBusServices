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
        public static void HandleTramAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            VehiclePaxDeltaInfo.Notify_VehicleStartsLoadingPax(vehicleID, ref data);
            // "skip loading" is only for redeployment; trams can never redeploy, "should skip loading" is always false, so no need to call that
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        public static void HandleTramAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop)
        {
            VehiclePaxDeltaInfo.Notify_VehicleFinishedLoadingPax(vehicleID, ref data);
        }
    }
}
