using HarmonyLib;
using UnityEngine;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TramAI))]
    [HarmonyPatch("CanLeave", MethodType.Normal)]
    public class Patch_TramAiCorrector
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            TramPickDropLookupTable.DetermineIfTramShouldDepart(ref __result, vehicleID, ref vehicleData);
        }
    }
}
