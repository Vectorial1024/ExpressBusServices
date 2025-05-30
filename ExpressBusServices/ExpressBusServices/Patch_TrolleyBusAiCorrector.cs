using ExpressBusServices.Util;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch(nameof(TrolleybusAI.CanLeave), MethodType.Normal)]
    public class Patch_TrolleyBusAiCorrector
    {
        [HarmonyPostfix]
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
        }
    }
}
