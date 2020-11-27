using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TrolleybusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_TrolleyBusLoadsPassengers
    {
        [HarmonyPrefix]
        public static void HandleBusAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAboutToBoardOntoBus(vehicleID, ref data);
        }

        [HarmonyPostfix]
        public static void HandleBusAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAlreadyBoardedOntoBus(vehicleID, ref data);
        }
    }
}
