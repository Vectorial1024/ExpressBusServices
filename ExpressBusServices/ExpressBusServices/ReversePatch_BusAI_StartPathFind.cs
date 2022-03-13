using HarmonyLib;
using System;
using System.Reflection;

namespace ExpressBusServices
{
    [HarmonyPatch]
    public class ReversePatch_BusAI_StartPathFind
    {

        [HarmonyTargetMethod]
        public static MethodBase TargetRelevantMethod()
        {
            return AccessTools.TypeByName("BusAI")?.GetMethod("StartPathFind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }) ?? null;
        }

        [HarmonyPrepare]
        public static bool DetermineIfShouldPatch()
        {
            // this method should exist
            return true;
        }

        [HarmonyReversePatch]
        public static bool BusAI_StartPathFind(object __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            throw new NotImplementedException("This is a stub that is not available at this moment.");
        }
    }
}
