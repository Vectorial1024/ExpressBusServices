using HarmonyLib;
using System;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("UnloadPassengers", MethodType.Normal)]
    public class ReversePatch_BusAI_UnloadPassengers
    {
        [HarmonyReversePatch]
        public static void BusAI_UnloadPassengers(object __instance, ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop)
        {
            throw new NotImplementedException("This is a stub that is not available at this moment.");
        }
    }
}
