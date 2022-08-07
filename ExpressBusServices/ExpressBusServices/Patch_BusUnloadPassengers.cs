using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("TransportArriveAtTarget", MethodType.Normal)]
    public class Patch_BusUnloadPassengers
    {
        [HarmonyPrefix]
        public static void CheckRedeployment(ushort vehicleID, ref Vehicle data, ref bool forceUnload)
        {
            ushort redeploymentTarget;
            ushort transportLineId = data.m_transportLine;
            // not yet target the next stop
            ushort currentTerminusStopId = data.m_targetBuilding;
            ServiceBalancerUtil.MarkVehicleIsAtStopId(vehicleID, currentTerminusStopId);
            if (ServiceBalancerUtil.FindRedeployToTerminus(vehicleID, transportLineId, currentTerminusStopId, out redeploymentTarget))
            {
                // set destination to somewhere else
                // it is basically a "super skip"
                ServiceBalancerUtil.MarkRedeployToNewTerminus(vehicleID, redeploymentTarget);
                // data.m_targetBuilding = redeploymentTarget;
                BusStopSkippingLookupTable.Notify_BusShouldSkipLoading(vehicleID);
                // force everyone to get dropped off for redeployment; they aren't supposed to travel around the bus line through termini anyways
                forceUnload = true;
            }
        }

        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data, ref int serviceCounter)
        {
            BusPickDropLookupTable.Notify_PassengersAlightedFromBus(vehicleID, serviceCounter);
        }
    }
}
