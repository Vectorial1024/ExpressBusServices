using ExpressBusServices.DataTypes;

namespace ExpressBusServices.Util
{
    /// <summary>
    /// The util class for handling customized vehicle departure logic
    /// </summary>
    public static class VehicleDepartureUtil
    {
        /// <summary>
        /// The centralized method for reviewing whether the given local public transport vehicle should change its departure status.
        /// This mod may hold or release vehicles according to a list of criteria, essentially overriding vanilla logic.
        /// </summary>
        /// <param name="__result">The result of this operation; if true, signals the game engine to depart the vehicle at the next sim-step.</param>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <param name="vehicleData">The data of the vehicle in question.</param>
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            /*
             * this is a tried-and-tested procedure to review departure status:
             * - when each vehicle arrives at a stop, write down the pax-gained and pax-lost stats (this event is fired for each vehicle in the vehicle set)
             * - with this, we can determine whether a pax-delta has occurred
             * - there is a set of logic to act when pax-delta occurs/does not occur
             * - decision to depart is delegated to the first vehicle of the set
             *
             * since everyone is looking at the first vehicle of the vehicle set for the actual decision,
             * the chances of "inconsistent departure state" (esp. in e.g. trams with multiple vehicles together) is eliminated
             *
             * since each vehicle can only stop at a single stop at any given time, we do not need to remember the stop IDs
             * this has the nice property that no save-game editing is needed!
             * but this also means we need to be careful when dealing with the "we just loaded the savegame" edge case
             */
            if (vehicleData.m_leadingVehicle != 0)
            {
                // delegate decision to the first vehicle of the set
                TransportVehicleUtil.FindFirstVehicleOfVehicleSet(vehicleID, ref vehicleData, out ushort firstVehicleID, out Vehicle firstVehicleData);
                ReviewDepartureStatus(ref __result, firstVehicleID, ref firstVehicleData);
                return;
            }

            if (!VehiclePaxDeltaInfo.Has(vehicleID))
            {
                // we don't have data for this; abort!
                return;
            }
            // wip
        }
        // wip
    }
}
