using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpressBusServices
{
    public class BusPickDropLookupTable
    {
        private static Dictionary<ushort, VehicleBAInfo> busPickDropTable;

        public static void EnsureTableExists()
        {
            if (busPickDropTable == null)
            {
                busPickDropTable = new Dictionary<ushort, VehicleBAInfo>();
            }
        }

        public static void WipeTable() => busPickDropTable?.Clear();

        public static VehicleBAInfo GetInfoForBus(ushort vehicleID, bool ensureExists = false)
        {
            VehicleBAInfo targetInfo;
            if (busPickDropTable.TryGetValue(vehicleID, out targetInfo))
            {
                return targetInfo;
            }
            else
            {
                if (ensureExists)
                {
                    targetInfo = new VehicleBAInfo();
                    busPickDropTable[vehicleID] = targetInfo;
                    return targetInfo;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool RecordForBusExists(ushort vehicleID) => busPickDropTable?.ContainsKey(vehicleID) ?? false;

        public static void Notify_PassengersAlightedFromBus(ushort vehicleID, int alightedCount)
        {
            // Debug.Log($"Vehicle ID: {vehicleID}, alighted: {alightedCount}");
            GetInfoForBus(vehicleID, true).Alighted = alightedCount;
        }

        [Obsolete("Use Notify_PassengersAlightedFromBus(2) instead.")]
        public static void Notify_PassengersAlightedFromBus(ushort vehicleID, ushort transferSize, int serviceCounter)
        {
            // NOTE: serviceCounter is the real "alighted count"; the transferSize is simply hinting how many remains on the vehicle, and is not useful.
            // Debug.Log($"Vehicle ID: {vehicleID}, alighted: {serviceCounter}");
            GetInfoForBus(vehicleID, true).Alighted = serviceCounter;
        }

        [Obsolete("Use Notify_PassengersAboutToBoardOntoBus and Notify_PassengersAlreadyBoardedOntoBus instead.")]
        public static void Notify_PassengersBoardedOntoBus(ushort vehicleID, ushort transferSize)
        {
            // Debug.Log($"Vehicle ID: {vehicleID}, boarded: {transferSize}");
            GetInfoForBus(vehicleID, true).Boarded = transferSize;
        }

        public static void Notify_PassengersAboutToBoardOntoBus(ushort vehicleID, ref Vehicle data)
        {
            GetInfoForBus(vehicleID, true).PassengersBeforeBoarding = data.m_transferSize;
        }

        public static void Notify_PassengersAlreadyBoardedOntoBus(ushort vehicleID, ref Vehicle data)
        {
            GetInfoForBus(vehicleID, true).PassengersAfterBoarding = data.m_transferSize;
        }

        public static void DetermineIfBusShouldDepart(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            /*
             * After reconsidering how everything works, I've decided to make use of 
             * one large static dictionary to store info of alight+board for each bus UID.
             * 
             * Previously it was found that simply reading the vehicle info is not enough to
             * determine alight+board count because alighting uses another variable.
             * Eventually I made use of two Harmony patches to read the alight+board info
             * and write it to the static dictionary.
             * 
             * The logic is now sth like this:
             * 
             * When bus arrives at stop:
             * Write down alighting+boarding count
             * Determine if the bus can leave now given the alighting, boarding, and n-th stop info
             * 
             * It is also determined that storing stop UID is not necessary because
             * each bus can only stop at one stop at any given point in time.
             * Stopping at a new stop will simply invalidate the previous stop info.
             * 
             * This entire system has a nice property that no save-file access is needed.
             */
            bool vehicleIsMinibus = vehicleData.m_transferSize <= 20;
            if (DepartureChecker.NowIsEligibleForInstantDeparture(vehicleID, ref vehicleData))
            {
                // midway bus stop; implement logic here
                // note: we disallow intercity buses from entering this segmentof code
                VehicleBAInfo info = GetInfoForBus(vehicleID);
                int waitTime = 12;
                int addDropDiff = info != null ? info.Alighted + info.ActualBoarded : 0;
                if (EBSModConfig.CanUseMinibusMode && vehicleIsMinibus && addDropDiff > 0 && addDropDiff <= 5)
                {
                    waitTime = 4;
                }
                if (info != null && addDropDiff == 0)
                {
                    // this bus did not get any alight or board
                    // then depart now
                    __result = true;
                }
                else if (vehicleData.m_waitCounter >= waitTime && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                {
                    // this bus has alight+board but has waited enough to qualify departure AND all citizens are boarded
                    // then depart now
                    // (the "all citizens boarded" part is more obvious when e.g. Realistic Walking Speed mod is used)
                    __result = true;
                }
                // apply the "runaway citizen check" here; it seems previously we have applied the fix at a wrong place.
            }
            // we are now at the point where we want to check whether to fix "runaway citizen".
            if (__result == false && vehicleData.m_waitCounter > 12)
            {
                // we should be able to depart... why can't we?
                if (UnbunchingReckeckIsActuallyCanLeave(vehicleID, ref vehicleData))
                {
                    // huh. we should actually be able to depart. the unbunching gave the green light.
                    __result = true;
                }
                if (!ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                {
                    // actually, someone still has not yet boarded, so do not depart yet.
                    __result = false;
                }
                // at midway bus stop
                // We should correct those bugged CIMs.
                // again, what we do here is simply "request recheck", but not to decide anything.
                // let the game decide things
                if (vehicleData.m_waitCounter % 4 == 0)
                {
                    // Don't check too frequently to reduce CPU stress.
                    CitizenRunawayTable.FixInvalidPublicTransitPassengers(vehicleID, ref vehicleData);
                }
            }
            // remove the redeployment instructions to avoid contaminating with arriving at other stops
            if (__result == true)
            {
                // can depart, remove redeployment instructions
                ServiceBalancerUtil.ReadRedeploymentInstructions(vehicleID, out _, removeEntry: true);
            }
        }

        public static bool UnbunchingReckeckIsActuallyCanLeave(ushort vehicleID, ref Vehicle vehicleData)
        {
            // mainly for IPT2; thre is probably some side effect that is caused by how the IPT2 plugin is influencing the work of IPT2 itself
            // this aims to remedy that.
            return Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].CanLeaveStop(vehicleData.m_targetBuilding, vehicleData.m_waitCounter >> 4);
        }
    }
}
