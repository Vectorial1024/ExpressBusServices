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

            RubberbandingCommand unbunchingIntention = RubberbandingCommand.Default;
            if (!VehiclePaxDeltaInfo.Has(vehicleID) || DepartureChecker.NowHasPotentialToSkipUnbunching(vehicleID, ref vehicleData))
            {
                // now is not at terminus, which has potential to skip unbunching
                // or, now is game fresh load, so no data, and we default to "no unbunching"
                int waitTime = 12;
                bool vehicleIsMinibus = vehicleData.m_leadingVehicle == 0 && vehicleData.m_trailingVehicle == 0 && VehicleUtil.GetMaxCarryingCapacityOfTrain(vehicleID, ref vehicleData) <= 20;
                if (EBSModConfig.CanUseMinibusMode && vehicleIsMinibus)
                {
                    // minibus mode: allow faster departure if pax delta is small
                    var paxDeltaInfo = VehiclePaxDeltaInfo.GetSafely(vehicleID);
                    if (paxDeltaInfo.PaxDeltaCount <= 5)
                    {
                        waitTime = 4;
                    }
                }

                // class-specific checking
                if (DepartureChecker.VehicleIsTram(vehicleData))
                {
                    // special handling for trams
                    if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.TRAM)
                    {
                        // brief stop and go
                        // prototype is Hong Kong Tram
                        if (VehiclePaxDeltaInfo.VehicleSetHasPaxDelta(vehicleID, ref vehicleData))
                        {
                            // has pax delta; wait fully
                            if (vehicleData.m_waitCounter >= waitTime)
                            {
                                unbunchingIntention = RubberbandingCommand.Go;
                            }
                        }
                        else
                        {
                            // no pax delta; go now!
                            unbunchingIntention = RubberbandingCommand.Go;
                        }
                    }
                    else if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.LIGHT_RAIL)
                    {
                        // full stop but no unbunch go
                        // prototype is Hong Kong LRT
                        if (vehicleData.m_waitCounter >= waitTime)
                        {
                            // whatever happens, they need to wait for the timer to finish
                            unbunchingIntention = RubberbandingCommand.Go;
                        }
                    }
                }
                else if (!VehiclePaxDeltaInfo.VehicleSetHasPaxDelta(vehicleID, ref vehicleData) || vehicleData.m_waitCounter >= waitTime)
                {
                    // no pax delta; can skip unbunching
                    // OR, we have waited enough
                    unbunchingIntention = RubberbandingCommand.Go;
                }
                // note: we no longer directly manipulate the departure flag here.
            }
            else
            {
                // now is at terminus, usually need to unbunch
                unbunchingIntention = DepartureChecker.GetRubberbandingUnbunchingForVehicle(vehicleID, ref vehicleData);
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

        /// <summary>
        /// Try to troubleshoot why departure is not allowed. This may change departure flag to true.
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="vehicleID"></param>
        /// <param name="vehicleData"></param>
        private static void TryTroubleshootWhyCannotDepart(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            if (__result)
            {
                // already planning to depart; no action needed
                return;
            }

            // why cannot go?
            if (vehicleData.m_waitCounter <= 12)
            {
                // wait counter <= 12 is normal
                return;
            }

            // we must respect pax that hasn't boarded yet
            if (!ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
            {
                // some cims are still boarding the vehicles; don't do it!
                return;
                /*
                 * note: the "Cim Runaway Problem" was originally discovered here,
                 * but eventually, Public Transport Unstucker (another mod) was made to more effectively deal with this problem.
                 * for general ease of management, users of this mod should also use Public Transport Unstucker.
                 */
            }

            // all pax aboard; check the advanced unbunching instructions
            if (DepartureChecker.RecheckUnbunchingCanLeave(vehicleID, ref vehicleData))
            {
                // rubberbanding says we gotta go now (spacing is good enough OR too many vehicles piled up, etc.)
                __result = true;
                return;
                // todo this feels like duplicated logic: rubberbanding seems to have 2 methods that essentially are opposites of each other
            }

            // then, idk.
        }
        // wip
    }
}
