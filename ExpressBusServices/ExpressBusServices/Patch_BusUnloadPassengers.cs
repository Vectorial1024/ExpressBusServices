using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("TransportArriveAtTarget", MethodType.Normal)]
    public class Patch_BusUnloadPassengers
    {
        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data, ref int serviceCounter)
        {
            BusPickDropLookupTable.Notify_PassengersAlightedFromBus(vehicleID, serviceCounter);
        }
    }
}
