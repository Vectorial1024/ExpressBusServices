using HarmonyLib;
using System;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch("StartPathFind", MethodType.Normal)]
    public class ReversePatch_TrolleybusAI_StartPathFind
    {
        [HarmonyReversePatch]
        public static bool TrolleybusAI_StartPathFind(object __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            throw new NotImplementedException("This is a stub that is not available at this moment.");
        }
    }
}
