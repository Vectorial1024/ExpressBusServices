using ExpressBusServices.Util;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices.Patches.Metro
{
    [HarmonyPatch(typeof(MetroTrainAI))]
    [HarmonyPatch(nameof(MetroTrainAI.CanLeave), MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_MetroTrainAiCorrector
    {

        /*
         * note:
         * no serious (heavy) railway system in the world has by-request stops, so no "extra skipping" patches for metros
         */

        [HarmonyPostfix]
        [UsedImplicitly]
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
        }
    }
}
