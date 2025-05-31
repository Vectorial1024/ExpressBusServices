using System.Collections.Generic;

namespace ExpressBusServices
{
    // this is used so that eg IPT2 can update their UI screens correctly
    public static class BusStopSkippingLookupTable
    {
        // what vehicle ID will skip the stop
        private static HashSet<ushort> busSkipStopTable;

        public static void EnsureTableExists()
        {
            if (busSkipStopTable == null)
            {
                busSkipStopTable = new HashSet<ushort>();
            }
        }

        public static void WipeTable() => busSkipStopTable?.Clear();

        public static bool BusShouldSkipPassengerLoading(ushort vehicleID)
        {
            return busSkipStopTable.Contains(vehicleID);
        }

        public static void Notify_BusShouldSkipLoading(ushort vehicleID)
        {
            busSkipStopTable.Add(vehicleID);
        }

        public static void ForgetBus(ushort vehicleID)
        {
            busSkipStopTable.Remove(vehicleID);
        }
    }
}
