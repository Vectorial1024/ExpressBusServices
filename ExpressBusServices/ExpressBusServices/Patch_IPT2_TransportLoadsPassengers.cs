using HarmonyLib;
using System.Reflection;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.High)]
    public class Patch_IPT2_TransportLoadsPassengers
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetRelevantIPT2Method()
        {
            return AccessTools.Method("ImprovedPublicTransport2.HarmonyPatches.LoadPassengersPatch:LoadPassengersPre");
        }

        [HarmonyPrepare]
        public static bool DetermineIfShouldPatch()
        {
            return TargetRelevantIPT2Method() != null;
        }

        // thanks to IPT, I have to do things in a very roundabout way.
        // but, in another perspective, this covers all the transport types that IPT can cover
        // will still need to handle vehicles with trailers, but bruh, it works for now.
        [HarmonyPrefix]
        public static void HandleTransportAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAboutToBoardOntoBus(vehicleID, ref data);
        }

        [HarmonyPostfix]
        public static void HandleTransportAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAlreadyBoardedOntoBus(vehicleID, ref data);
        }
    }
}
