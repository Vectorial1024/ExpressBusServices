using ColossalFramework;
using ExpressBusServices.DataTypes;
using ExpressBusServices.Redeployment;
using ExpressBusServices.Util;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch(nameof(BusAI.TransportArriveAtTarget), MethodType.Normal)]
    public class Patch_BusUnloadPassengers
    {
        /*
         * important note:
         * for some reason, this is called from BusAI, and also TramAI.
         */

        [HarmonyPrefix]
        public static void CheckRedeployment(ushort vehicleID, ref Vehicle data, ref bool forceUnload)
        {
            // reset the relevant pax-delta info first
            VehiclePaxDeltaInfo.TouchAndResetEntry(vehicleID);

            if (DepartureChecker.VehicleIsTram(data))
            {
                // trams are not eligible for redeployment.
                return;
            }
            if (DepartureChecker.VehicleIsNotBus(data))
            {
                // huh???
                return;
            }
            if (DepartureChecker.BusIsIntercityBus(data))
            {
                // no
                return;
            }
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
                // what if it is too far away? we will need to handle the stuff.
                if (TeleportRedeployInstructions.ShouldUseTeleportationRedeployment(vehicleID, redeploymentTarget))
                {
                    TeleportRedeployInstructions.NotifyTransportLineAddFutureDeployment(data.m_transportLine, redeploymentTarget);
                    TransportVehicleUtil.TellVehicleToReturnToBase(vehicleID, ref data);
                    // Debug.Log($"Vehicle {vehicleID} is redeploying via teleportation because the target {redeploymentTarget} is too far away.");
                }
            }
            if (!TransportVehicleUtil.VehicleHasProgressPercent(vehicleID, ref data))
            {
                // something wrong happened; all should have progress!
                // send them back to depot
                BusStopSkippingLookupTable.Notify_BusShouldSkipLoading(vehicleID);
                forceUnload = true;
                TransportVehicleUtil.TellVehicleToReturnToBase(vehicleID, ref data);
            }
        }

        [HarmonyPostfix]
        public static void HandleBusArrivedAtTarget(ushort vehicleID, ref Vehicle data, ref int serviceCounter)
        {
            VehiclePaxDeltaInfo.Notify_VehicleFinishedUnloadingPax(vehicleID, serviceCounter);
        }
    }
}
