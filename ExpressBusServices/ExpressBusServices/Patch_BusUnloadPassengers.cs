using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("TransportArriveAtTarget", MethodType.Normal)]
    public class Patch_BusUnloadPassengers
    {
        [HarmonyPrefix]
        public static void CheckRedeployment(ushort vehicleID, ref Vehicle data)
        {
            ushort redeploymentTarget;
            ushort transportLineId = data.m_transportLine;
            // not yet target the next stop
            ushort currentTerminusStopId = data.m_targetBuilding;
            if (ServiceBalancerUtil.FindRedeployToTerminus(transportLineId, currentTerminusStopId, out redeploymentTarget))
            {
                // set destination to somewhere else
                // it is basically a "super skip"
                ServiceBalancerUtil.MarkRedeployToNewTerminus(null, vehicleID, ref data, data.m_targetBuilding, redeploymentTarget);
                // data.m_targetBuilding = redeploymentTarget;
                BusStopSkippingLookupTable.Notify_BusShouldSkipLoading(vehicleID);
            }
        }

        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data, ref int serviceCounter)
        {
            BusPickDropLookupTable.Notify_PassengersAlightedFromBus(vehicleID, serviceCounter);
        }
    }
}
