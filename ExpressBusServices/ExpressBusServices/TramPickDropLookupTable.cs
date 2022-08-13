using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpressBusServices
{
    public class TramPickDropLookupTable
    {
        private static Dictionary<ushort, VehicleBAInfo> tramPickDropTable;

        public static void EnsureTableExists()
        {
            if (tramPickDropTable == null)
            {
                tramPickDropTable = new Dictionary<ushort, VehicleBAInfo>();
            }
        }

        public static void WipeTable() => tramPickDropTable?.Clear();

        public static VehicleBAInfo GetInfoForTramByLeader(ushort vehicleID, bool ensureExists = false)
        {
            VehicleBAInfo targetInfo;
            if (tramPickDropTable.TryGetValue(vehicleID, out targetInfo))
            {
                return targetInfo;
            }
            else
            {
                if (ensureExists)
                {
                    targetInfo = new VehicleBAInfo();
                    tramPickDropTable[vehicleID] = targetInfo;
                    return targetInfo;
                }
                else
                {
                    return null;
                }
            }
        }

        public static VehicleBAInfo GatherInfoForTramWithLeader(ushort leaderVehicleID)
        {
            // the way the tram works, it reuses some code from the bus ai
            // therefore we need a function to collect the info from all the tram trailers

            // clear our record first
            VehicleBAInfo info = new VehicleBAInfo();

            // and then gather the stuff
            VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
            ushort currentVehicleID = leaderVehicleID;
            int unloadCount = 0;
            int loadCount = 0;
            int iterationCount = 0;
            while (currentVehicleID != 0)
            {
                unloadCount += BusPickDropLookupTable.GetInfoForBus(currentVehicleID).Alighted;
                loadCount += BusPickDropLookupTable.GetInfoForBus(currentVehicleID).ActualBoarded;
                // move to next trailer
                currentVehicleID = vehicleManager.m_vehicles.m_buffer[currentVehicleID].m_trailingVehicle;
                if (++iterationCount > 16384)
                {
                    // invalid list yada yada
                    break;
                }
            }

            // finally saving the stuff
            info.Alighted = unloadCount;
            info.TramActualBoarded = loadCount;

            // 
            return info;
        }

        public static bool RecordForTramExists(ushort vehicleID) => tramPickDropTable?.ContainsKey(vehicleID) ?? false;

        public static void Notify_PassengersAlightedFromTram(ushort vehicleID, int alightedCount)
        {
            // Debug.Log($"Vehicle ID: {vehicleID}, alighted: {alightedCount}");
            GetInfoForTramByLeader(vehicleID, true).Alighted = alightedCount;
        }

        public static void Notify_PassengersAboutToBoardOntoTram(ushort vehicleID, ref Vehicle data)
        {
            GetInfoForTramByLeader(vehicleID, true).PassengersBeforeBoarding = data.m_transferSize;
        }

        public static void Notify_PassengersAlreadyBoardedOntoTram(ushort vehicleID, ref Vehicle data)
        {
            GetInfoForTramByLeader(vehicleID, true).PassengersAfterBoarding = data.m_transferSize;
        }

        public static void Notify_TramTotallyLoadedPassengers(ushort leaderVehicleID, int loadCount)
        {

        }

        public static void DetermineIfTramShouldDepart(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_leadingVehicle != 0)
            {
                // non leader vehicle; IDK!
                return;
            }
            // the logic should be somewhat similar to the bus logic, so you can go there and read the doc
            if (DepartureChecker.NowIsEligibleForInstantDeparture(vehicleID, ref vehicleData))
            {
                // midway tram stop; implement logic here
                if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.NONE)
                {
                    // no action!
                    return;
                }

                if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.TRAM)
                {
                    // brief stop and go
                    // prototype is Hong Kong Tram
                    VehicleBAInfo info = GatherInfoForTramWithLeader(vehicleID);
                    bool noAddDrop = info != null && info.Alighted + info.ActualBoarded == 0;
                    if (noAddDrop)
                    {
                        // no add drop; go now!
                        __result = true;
                        return;
                    }
                    // has add drop
                    if (vehicleData.m_waitCounter >= 12 && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                    {
                        // all boarded; go!
                        __result = true;
                    }
                }
                else if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.LIGHT_RAIL)
                {
                    // full stop but no unbunch go
                    // prototype is Hong Kong LRT
                    if (vehicleData.m_waitCounter >= 12 && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                    {
                        // all boarded; go!
                        __result = true;
                    }
                }
            }
        }
    }
}
