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
    }
}
