using System.Collections.Generic;
using JetBrains.Annotations;

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
        /// <para/>
        /// A new entry will be created if the vehicle was not previously initialized. 
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <returns>The last-known pax-delta info of said vehicle.</returns>
        [NotNull]
        public static VehiclePaxDeltaInfo GetSafely(ushort vehicleID)
        {
            if (!paxDeltaTable.TryGetValue(vehicleID, out VehiclePaxDeltaInfo value))
            {
                value = new VehiclePaxDeltaInfo();
                paxDeltaTable[vehicleID] = value;
            }
            return value;
        }

        /// <summary>
        /// Removes the pax-delta info object from the static table, which frees some memory back to the system.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        public static void Remove(ushort vehicleID) => paxDeltaTable.Remove(vehicleID);

        public static void Notify_VehicleFinishedUnloadingPax(ushort vehicleID, int serviceCounter)
        {
            GetSafely(vehicleID).PaxAlighted = serviceCounter;
        }

        public static void Notify_VehicleStartsLoadingPax(ushort vehicleID, ref Vehicle data)
        {
            GetSafely(vehicleID).PaxBeforeBoarding = data.m_transferSize;
        }

        public static void Notify_VehicleFinishedLoadingPax(ushort vehicleID, ref Vehicle data)
        {
            GetSafely(vehicleID).PaxAfterBoarding = data.m_transferSize;
        }
    }
}
