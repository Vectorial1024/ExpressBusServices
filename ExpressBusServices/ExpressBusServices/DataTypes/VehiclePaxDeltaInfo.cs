using System.Collections.Generic;
using ColossalFramework;
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

        /// <summary>
        /// The actual delta-pax of this vehicle. This is used by minibus-mode.
        /// </summary>
        public int PaxDeltaCount => PaxAlighted + PaxActualBoarded;
        
        public bool HasPaxDelta => PaxAlighted > 0 || PaxActualBoarded > 0;

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

        /// <summary>
        /// Returns whether the vehicle set of the given vehicle (aka the "train" that contains the vehicle) has any passenger delta.
        /// This counts the whole set in addition to the vehicle itself; trailers and leaders are also considered.
        /// <para/>
        /// This method may be safely called by any valid vehicle of the vehicle set, and will return identical results. 
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <param name="data">The data reference of the vehicle in question, to be used for iteration.</param>
        /// <returns>If true, then the vehicle set has a pax-delta among its constituent vehicles.</returns>
        public static bool VehicleSetHasPaxDelta(ushort vehicleID, ref Vehicle data)
        {
            // optimization: short circuit if this one has pax delta
            // single-vehicle buses and busy trams will benefit from this optimization
            if (GetSafely(vehicleID).HasPaxDelta)
            {
                return true;
            }

            // this vehicle does NOT have delta, but other vehicles in the set may have
            // standard procedure
            // note: we assume "valid lists" so we will not check for iteration sizes as seen in vanilla code.
            VehicleManager managerInstance = Singleton<VehicleManager>.instance;

            // first, iterate the "pointer" to the front.
            ref Vehicle currentData = ref data;
            ushort currentID = currentData.m_leadingVehicle;
            while (currentID != 0)
            {
                currentData = managerInstance.m_vehicles.m_buffer[currentID];
                currentID = currentData.m_leadingVehicle;
            }

            // we are at the front
            // next, iterate till the end
            while (true)
            {
                if (GetSafely(currentID).HasPaxDelta)
                {
                    return true;
                }
                currentID = currentData.m_trailingVehicle;
                if (currentID == 0)
                {
                    break;
                }
                currentData = managerInstance.m_vehicles.m_buffer[currentID];
            }
            // reached end without pax delta
            return false;
        }
    }
}
