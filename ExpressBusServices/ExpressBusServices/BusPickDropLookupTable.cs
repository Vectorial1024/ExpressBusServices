using System.Collections.Generic;

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

        public static void Notify_PassengersAlightedFromBus(ushort vehicleID, ushort transferSize, int serviceCounter)
        {
            int actualAlighted = transferSize + serviceCounter;
            GetInfoForBus(vehicleID, true).Alighted = actualAlighted;
        }

        public static void Notify_PassengersBoardedOntoBus(ushort vehicleID, ushort transferSize)
        {
            GetInfoForBus(vehicleID, true).Boarded = transferSize;
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
            if (DepartureChecker.NowIsEligibleForInstantDeparture(vehicleID, ref vehicleData))
            {
                // midway bus stop; implement logic here
                VehicleBAInfo info = GetInfoForBus(vehicleID);
                if (info != null && info.Alighted + info.Boarded == 0)
                {
                    // this bus did not get any alight or board
                    // then depart now
                    __result = true;
                }
                else if (vehicleData.m_waitCounter >= 12 && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                {
                    // this bus has alight+board but has waited enough to qualify departure AND all citizens are boarded
                    // then depart now
                    // (the "all citizens boarded" part is more obvious when e.g. Realistic Walking Speed mod is used)
                    __result = true;
                }
            }
            if (__result == false && vehicleData.m_waitCounter > 12 && !ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
            {
                // I can't depart because some CIMs have reserved us but run away.
                // We should correct those bugged CIMs.
                if (vehicleData.m_waitCounter % 4 == 0)
                {
                    // Don't check too frequently to reduce CPU stress.
                    CitizenRunawayTable.FixInvalidPublicTransitPassengers(vehicleID, ref vehicleData);
                }
            }
        }
    }
}
