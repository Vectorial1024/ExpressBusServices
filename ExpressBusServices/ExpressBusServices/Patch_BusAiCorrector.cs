﻿using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("CanLeave", MethodType.Normal)]
    public class Patch_BusAiCorrector
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            BusPickDropLookupTable.DetermineIfBusShouldDepart(ref __result, vehicleID, ref vehicleData);
        }
    }
}
