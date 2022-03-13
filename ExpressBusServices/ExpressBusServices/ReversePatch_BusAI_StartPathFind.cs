using HarmonyLib;
using System;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("StartPathFind", MethodType.Normal)]
    public class ReversePatch_BusAI_StartPathFind
    {
        [HarmonyReversePatch]
        public static bool BusAI_StartPathFind(object __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            throw new NotImplementedException("This is a stub that is not available at this moment.");
        }
    }
}
