using System;
using System.Collections.Generic;

namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// A class for remembering per-vehicle passenger delta information, assuming the vehicle is a local public transport vehicle now stopped at some stop.
    /// <para/>
    /// It just turns out, all kinds of public transport vehicles share essentially the same flow. 
    /// </summary>
    public class VehiclePaxDeltaInfo
    {
        public int PaxAlighted { get; set; }

        public int PaxBeforeBoarding { get; set; }

        public int PaxAfterBoarding { get; set; }

        public int PaxActualBoarded => PaxAfterBoarding - PaxBeforeBoarding;

        public bool PaxHasDelta => PaxAlighted > 0 || PaxActualBoarded > 0;

        private static Dictionary<ushort, VehiclePaxDeltaInfo> paxDeltaTable;

        public static void EnsureTableExists()
        {
            if (paxDeltaTable == null)
            {
                paxDeltaTable = new Dictionary<ushort, VehiclePaxDeltaInfo>();
            }
        }

        public static void WipeTable() => paxDeltaTable?.Clear();

        /// <summary>
        /// Resets the pax-delta info related to a vehicle, or touches it if it is not used before, assuming the vehicle is a local supported public transport vehicle.
        /// <para/>
        /// This should be called at the first moment when the vehicle arrives at a stop, and before anything else is done, to ensure correctness.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        public static void TouchAndResetEntry(ushort vehicleID)
        {
            paxDeltaTable[vehicleID] = new VehiclePaxDeltaInfo();
        }

        /// <summary>
        /// Returns the pax-delta info of a vehicle. This does NOT consider the trailers of the vehicle.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <returns>The last-known pax-delta info of said vehicle.</returns>
        /// <exception cref="ArgumentException">Thrown when a pax-delta info was requested without being initialized.</exception>
        public static VehiclePaxDeltaInfo Get(ushort vehicleID)
        {
            if (!paxDeltaTable.TryGetValue(vehicleID, out VehiclePaxDeltaInfo value))
            {
                throw new ArgumentException("PaxDelta info for ${vehicleID} requested without being initialized");
            }
            return value;
        }

        /// <summary>
        /// Removes the pax-delta info object from the static table, which frees some memory back to the system.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        public static void Remove(ushort vehicleID) => paxDeltaTable.Remove(vehicleID);

        public static void Notify_VehicleHasUnloadedPax(ushort vehicleID, ref int serviceCounter)
        {
            Get(vehicleID).PaxAlighted = serviceCounter;
        }

        public static void Notify_VehicleStartsLoadingPax(ushort vehicleID, ref Vehicle data)
        {
            Get(vehicleID).PaxBeforeBoarding = data.m_transferSize;
        }

        public static void Notify_VehicleHasLoadedPax(ushort vehicleID, ref Vehicle data)
        {
            Get(vehicleID).PaxAfterBoarding = data.m_transferSize;
        }
    }
}
