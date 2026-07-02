using ExpressBusServices.Util;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices.Patches.Railway
{
    /*
     * note: MetroTrainAI shares code from PassengerTrainAI by partially deriving from it.
     */

    [HarmonyPatch(typeof(PassengerTrainAI))]
    [HarmonyPatch(nameof(PassengerTrainAI.CanLeave), MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_SharedTrainAiCorrector
    {

        /*
         * note:
         * no serious (heavy) railway system in the world has by-request stops, so no "extra skipping" patches for metros
         */

        [HarmonyPostfix]
        [UsedImplicitly]
        public static void ReviewDepartureStatus(PassengerTrainAI __instance, ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            if (__instance is MetroTrainAI)
            {
                VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
            }
        }
    }
}
