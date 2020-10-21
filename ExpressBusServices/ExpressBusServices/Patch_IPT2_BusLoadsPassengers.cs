using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.High)]
    public class Patch_IPT2_BusLoadsPassengers
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

        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID)
        {
            // thanks to IPT, I have to do things in a very roundabout way.
            Vehicle data = VehicleManager.instance.m_vehicles.m_buffer[vehicleID];
            BusPickDropLookupTable.Notify_PassengersBoardedOntoBus(vehicleID, data.m_transferSize);
        }
    }
}
