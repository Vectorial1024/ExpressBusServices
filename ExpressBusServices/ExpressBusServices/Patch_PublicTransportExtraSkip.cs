using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices
{
    [HarmonyPatch]
    public class Patch_PublicTransportExtraSkip
    {
        /*
         * Special thanks to klyte45 from TLM for letting me use this logic.
         */

        [HarmonyTargetMethod]
        public static MethodBase TargetRelevantMethod()
        {
            return AccessTools.Method(typeof(VehicleAI), "ArrivingToDestination");
        }

        [HarmonyPrepare]
        public static bool DetermineIfShouldPatch()
        {
            // dont do it if TLM is detected
            return !ModDetector.TransportLinesManagerIsLoaded();
        }

        [HarmonyPrefix]
        public static bool PreFix(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            if (!(__instance is BusAI || __instance is TrolleybusAI))
            {
                // not bus or trolleybus; no
                return true;
            }
            if (false)
            {
                // read the config: dont do it
                return true;
            }

            ushort currentStop = vehicleData.m_targetBuilding;
            ref TransportLine line = ref TransportManager.instance.m_lines.m_buffer[vehicleData.m_transportLine];
            if (!DepartureChecker.NowIsEligibleForInstantDeparture(vehicleID, ref vehicleData))
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
            var pathfindParams = new object[] { vehicleID, vehicleData };
            if (__instance is BusAI busAi)
            {
                if (!ReversePatch_BusAI_StartPathFind.BusAI_StartPathFind(__instance, vehicleID, ref vehicleData))
                {
                    // something bad happened; cancel
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }

                vehicleData = (Vehicle)pathfindParams[1];
                // I think this is to let it iterate their stuff
                ReversePatch_BusAI_UnloadPassengers.BusAI_UnloadPassengers(busAi, vehicleID, ref vehicleData, currentStop, nextStop);
            } 
            else if (__instance is TrolleybusAI trolleyAi)
            {
                if (!ReversePatch_TrolleybusAI_StartPathFind.TrolleybusAI_StartPathFind(__instance, vehicleID, ref vehicleData))
                {
                    // something bad happened; cancel
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }

                vehicleData = (Vehicle)pathfindParams[1];
                // I think this is to let it iterate their stuff
                ReversePatch_TrolleybusAI_UnloadPassengers.TrolleybusAI_UnloadPassengers(trolleyAi, vehicleID, ref vehicleData, currentStop, nextStop);
            }

            return false;
        }
    }
}
