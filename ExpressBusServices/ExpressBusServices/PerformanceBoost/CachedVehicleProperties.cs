using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices.PerformanceBoost
{
    public class CachedVehicleProperties
    {
        // cached stuff of a vehicle that does not survive a save-load.
        public int TrainCapacity { get; internal set; }

        private static Dictionary<ushort, CachedVehicleProperties> cachedProps = new Dictionary<ushort, CachedVehicleProperties>();

        public static void TouchAndResetCache()
        {
            if (cachedProps == null)
            {
                cachedProps = new Dictionary<ushort, CachedVehicleProperties>();
            }
            cachedProps.Clear();
        }

        public static CachedVehicleProperties GetFromCache(ushort vehicleId)
        {
            // does not nsure exists
            return cachedProps[vehicleId] ?? null;
        }

        public static void SetToCache(ushort vehicleId, CachedVehicleProperties cachedData)
        {
            cachedProps[vehicleId] = cachedData;
        }

        public static void UnsetCache(ushort vehicleId)
        {
            cachedProps.Remove(vehicleId);
        }
    }
}
