using ExpressBusServices.Util;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch(nameof(BusAI.CanLeave), MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_BusAiCorrector
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
        }
    }
}
