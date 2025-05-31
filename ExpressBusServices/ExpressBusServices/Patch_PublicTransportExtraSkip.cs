using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    // set priority such that IPT2 can execute first; essentially this should execute last
    [HarmonyPatch]
    [HarmonyPriority(Priority.LowerThanNormal)]
    [UsedImplicitly]
    public class Patch_PublicTransportExtraSkip
    {
        /*
         * Special thanks to klyte45 from TLM for letting me use this logic.
         */

        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetRelevantMethod()
        {
            return AccessTools.Method(typeof(VehicleAI), "ArrivingToDestination");
        }

        [HarmonyPrepare]
        [UsedImplicitly]
        public static bool DetermineIfShouldPatch()
        {
            // dont do it if TLM is detected
            // however, it seems that we can only detect whether the files of TLM exists;
            // we cant actually detect whether they are enabled, so this is modified to be "always true".
            // FileLog.Log($"TLM exists? {ModDetector.TransportLinesManagerIsLoaded()}");
            // return !ModDetector.TransportLinesManagerIsLoaded();
            return true;
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        public static bool ExtraSkippingLogic(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            // FileLog.Log($"Current Express Mode: {EBSModConfig.CurrentExpressBusMode}; Do Extra Skip? ${(int)EBSModConfig.CurrentExpressBusMode < (int)EBSModConfig.ExpressMode.AGGRESSIVE}");
            if (!(__instance is BusAI || __instance is TrolleybusAI))
            {
                // not bus or trolleybus; no
                return true;
            }
            if ((int)EBSModConfig.CurrentExpressBusMode < (int)EBSModConfig.ExpressMode.AGGRESSIVE)
            {
                // settings not enabled; no
                return true;
            }

            ushort currentStop = vehicleData.m_targetBuilding;
            if (currentStop == 0 || vehicleData.m_transportLine == 0)
            {
                // this can happen when e.g. the depot is forced to deactivate and the vehicles are therefore forced to return to base
                // in this case, don't do it
                return true;
            }
            if (!DepartureChecker.CanSkipNextStop(vehicleID, ref vehicleData))
            {
                // is arriving at terminus; dont do this!
                return true;
            }
            TransportLineUtil.CountPassengersWaiting(currentStop, out int residents, out int tourists);
            var unloadPredict = TransportLineUtil.GetQuantityPassengerUnloadOnNextStop(vehicleID, ref vehicleData, out bool full, out bool empty);

            if (unloadPredict > 0 || (!full && (residents + tourists) > 0))
            {
                // have someone dropping off OR have boardable passengers
                // dont do it
                return true;
            }

            // okay can do
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            vehicleData.m_targetBuilding = nextStop;
            BusStopSkippingLookupTable.Notify_BusShouldSkipLoading(vehicleID);
            var pathfindParams = new object[] { vehicleID, vehicleData };
            var unloadParams = new object[] { vehicleID, vehicleData, currentStop, nextStop };
            if (__instance is BusAI busAi)
            {
                if (!(bool) AccessTools.Method(typeof(BusAI), "StartPathFind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }).Invoke(busAi, pathfindParams))
                {
                    // something bad happened; cancel
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }

                vehicleData = (Vehicle)pathfindParams[1];
                // I think this is to let it iterate their stuff
                AccessTools.Method(typeof(BusAI), "UnloadPassengers").Invoke(busAi, unloadParams);
                AccessTools.Method(typeof(BusAI), "LoadPassengers").Invoke(busAi, unloadParams);
            }
            else if (__instance is TrolleybusAI trolleyAi)
            {
                if (!(bool) AccessTools.Method(typeof(TrolleybusAI), "StartPathfind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }).Invoke(trolleyAi, pathfindParams))
                {
                    // something bad happened; cancel
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }

                vehicleData = (Vehicle)pathfindParams[1];
                // I think this is to let it iterate their stuff
                AccessTools.Method(typeof(TrolleybusAI), "UnloadPassengers").Invoke(trolleyAi, unloadParams);
                AccessTools.Method(typeof(TrolleybusAI), "LoadPassengers").Invoke(trolleyAi, unloadParams);
            }
            else
            {
                // we should have already filtered this...?
                return true;
            }

            // get next path
            if (vehicleData.m_path == 0 && (vehicleData.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
                vehicleData.Info.m_vehicleAI.SetTransportLine(vehicleID, ref vehicleData, 0);
            }
            return false;
        }
    }
}
