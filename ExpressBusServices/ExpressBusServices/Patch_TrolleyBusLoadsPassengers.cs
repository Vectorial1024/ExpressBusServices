using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_TrolleyBusLoadsPassengers
    {
        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersBoardedOntoBus(vehicleID, data.m_transferSize);
        }
    }
}
