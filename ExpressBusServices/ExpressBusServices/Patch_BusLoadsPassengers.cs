using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_BusLoadsPassengers
    {
        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersBoardedOntoBus(vehicleID, data.m_transferSize);
        }
    }
}
