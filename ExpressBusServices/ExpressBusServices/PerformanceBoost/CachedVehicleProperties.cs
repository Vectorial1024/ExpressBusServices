using ColossalFramework;
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

        private uint ExpirySimTick { get; set; }

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
            // does not ensure exists
            if (!cachedProps.ContainsKey(vehicleId))
            {
                return null;
            }
            CachedVehicleProperties props = cachedProps[vehicleId];
            // is it expired?
            if (props.ExpirySimTick > Singleton<SimulationManager>.instance.m_currentTickIndex)
            {
                // expired; forget it
                UnsetCache(vehicleId);
                return null;
            }
            // not expired yet
            return props;
        }

        public static void SetToCache(ushort vehicleId, CachedVehicleProperties cachedData)
        {
            // how long?
            // I found some data suggesting 60 ticks = 1 second by definition
            // so 300 -> 5 seconds, sounds reasonable; this will be enough to cover a bit of the unbunching
            cachedData.ExpirySimTick = Singleton<SimulationManager>.instance.m_currentTickIndex + 300;
            cachedProps[vehicleId] = cachedData;
        }

        public static void UnsetCache(ushort vehicleId)
        {
            cachedProps.Remove(vehicleId);
        }
    }
}
