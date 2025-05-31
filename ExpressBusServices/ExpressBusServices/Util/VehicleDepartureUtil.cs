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
        /// <para/>
        /// DO NOT call this for irrelevant/unsupported vehicles!
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

            RubberbandingCommand unbunchingIntention = RubberbandingCommand.Default;
            // note: no-data cases will need to be handled at respective methods
            // otherwise, will cause vehicles to e.g. ignore unbunching
            if (DepartureChecker.NowHasPotentialToSkipUnbunching(vehicleID, ref vehicleData))
            {
                // now is not at terminus, which has potential to skip unbunching
                // note: we no longer directly manipulate the departure flag here.
                unbunchingIntention = DepartureChecker.GetInstantDepartIntentionForVehicle(vehicleID, ref vehicleData);
            }
            else
            {
                // now is at terminus, usually need to unbunch
                unbunchingIntention = DepartureChecker.GetRubberbandingIntentionForVehicle(vehicleID, ref vehicleData);
            }

            // update the flag according to our intention
            if (unbunchingIntention == RubberbandingCommand.Hold)
            {
                // don't go yet!
                __result = false;
            }
            else if (unbunchingIntention == RubberbandingCommand.Go)
            {
                // we intend to go; has everyone boarded yet?
                if (VehicleUtil.IsEveryoneAboardTheTrain(vehicleID, ref vehicleData))
                {
                    // yes indeed.
                    __result = true;
                }
            }

            // status is finalized
            if (__result)
            {
                // allowed to depart

                // remove the redeployment instructions to avoid contaminating with arriving at other stops
                ServiceBalancerUtil.ReadRedeploymentInstructions(vehicleID, out _, removeEntry: true);
            }
        }
    }
}
