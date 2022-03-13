using HarmonyLib;
using System;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch("UnloadPassengers", MethodType.Normal)]
    public class ReversePatch_TrolleybusAI_UnloadPassengers
    {
        [HarmonyReversePatch]
        public static bool TrolleybusAI_UnloadPassengers(object __instance, ushort vehicleID, ref Vehicle vehicleData, ushort currentStop, ushort nextStop)
        {
            throw new NotImplementedException("This is a stub that is not available at this moment.");
        }
    }
}
