using ExpressBusServices.Util;
using HarmonyLib;
using UnityEngine;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TramAI))]
    [HarmonyPatch(nameof(TramAI.CanLeave), MethodType.Normal)]
    public class Patch_TramAiCorrector
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
            // TramPickDropLookupTable.DetermineIfTramShouldDepart(ref __result, vehicleID, ref vehicleData);
        }
    }
}
