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
                    VehicleBAInfo info = GetInfoForTramByLeader(vehicleID);
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
