using System;
using System.Collections.Generic;
using ColossalFramework;
using ExpressBusServices.DataTypes;
using UnityEngine;

namespace ExpressBusServices
{
    public class DepartureChecker
    {
        public static readonly float UnbunchingProximityPercentDist = 0.02f;

        // if true, then this mod will intervene in handling instant departures etc.
        public static bool NowHasPotentialToSkipUnbunching(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (BusIsIntercityBus(vehicleData))
            {
                // always no for intercity buses
                return false;
            }

            // todo check some global flag to instant depart for select buses
            if (ServiceBalancerUtil.ReadRedeploymentInstructions(vehicleID, out _))
            {
                // we have redeployment instructions, let's do it
                return true;
            }

            ushort transportLineID = vehicleData.m_transportLine;
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);

            return !StopIsConsideredAsTerminus(currentStop, transportLineID);
        }

        // if true, then this mod will instruct buses to skip the stop
        // this looks very similar to the above, except that the numbers are adjusted for correctness.
        public static bool CanSkipNextStop(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (BusIsIntercityBus(vehicleData))
            {
                // always no for intercity buses
                return false;
            }

            ushort transportLineID = vehicleData.m_transportLine;
            ushort approachingStop = vehicleData.m_targetBuilding;

            return !StopIsConsideredAsTerminus(approachingStop, transportLineID);
        }

        public static bool StopIsConsideredAsTerminus(ushort stopID, ushort transportLineID)
        {
            // IPT2 will override this, to make the things more streamlined
            // return true if this should be considered a terminus
            // buses will unbunch when at terminus

            TransportManager transportManager = Singleton<TransportManager>.instance;
            ushort firstStopID = transportManager.m_lines.m_buffer[transportLineID].GetLastStop();

            // architecture reversal
            // we will actively read TLM info
            bool isTerminus = firstStopID != 0 && stopID != 0 && stopID == firstStopID;
            if (ReversePatch_TLMPlugin_StopIsTerminus.PatchIsSuccessful_HasTLM)
            {
                isTerminus |= ReversePatch_TLMPlugin_StopIsTerminus.StopIsConsideredTerminus(stopID);
            }
            return isTerminus;
        }

        public static bool BusIsIntercityBus(Vehicle vehicleData)
        {
            ItemClass itemClass = vehicleData.Info.m_class;
            return TransportStationAI.IsIntercity(itemClass);
        }

        public static bool VehicleIsTram(Vehicle vehicleData)
        {
            ItemClass itemClass = vehicleData.Info.m_class;
            return itemClass.m_service == ItemClass.Service.PublicTransport && itemClass.m_subService == ItemClass.SubService.PublicTransportTram;
        }

        public static bool VehicleIsNotBus(Vehicle vehicleData)
        {
            ItemClass itemClass = vehicleData.Info.m_class;
            return itemClass.m_service != ItemClass.Service.PublicTransport || itemClass.m_subService != ItemClass.SubService.PublicTransportBus;
        }

        public static bool VehicleIsNotTrolleyBus(Vehicle vehicleData)
        {
            ItemClass itemClass = vehicleData.Info.m_class;
            return itemClass.m_service != ItemClass.Service.PublicTransport || itemClass.m_subService != ItemClass.SubService.PublicTransportTrolleybus;
        }

        /// <summary>
        /// Determines the midway instant-depart command to be given to the vehicle.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <param name="vehicleData">The data of the vehicle in question.</param>
        /// <returns>The midway instant-depart command for the vehicle.</returns>
        public static RubberbandingCommand GetInstantDepartIntentionForVehicle(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (!VehiclePaxDeltaInfo.Has(vehicleID))
            {
                // no data; most likely fresh-load from savegame
                // just hold for 12 ticks as usual.
                return vehicleData.m_waitCounter >= 12 ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
            }

            // have data; check for pax delta
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
            if (VehicleIsTram(vehicleData))
            {
                // special handling for trams
                if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.TRAM)
                {
                    // brief stop and go
                    // prototype is Hong Kong Tram
                    if (!VehiclePaxDeltaInfo.VehicleSetHasPaxDelta(vehicleID, ref vehicleData))
                    {
                        // no pax delta; go now!
                        return RubberbandingCommand.Go;
                    }
                    // hax pax delta; wait fully
                    return vehicleData.m_waitCounter >= waitTime ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
                }
                if (EBSModConfig.CurrentExpressTramMode == EBSModConfig.ExpressTramMode.LIGHT_RAIL)
                {
                    // full stop but no unbunch go
                    // prototype is Hong Kong LRT
                    // whatever happens, they need to wait for the timer to finish
                    return vehicleData.m_waitCounter >= waitTime ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
                }
                // tram mode not enabled
                return RubberbandingCommand.Default;
            }

            // this should be buses/trolleybuses
            if (!VehiclePaxDeltaInfo.VehicleSetHasPaxDelta(vehicleID, ref vehicleData))
            {
                // no pax delta; can skip unbunching
                return RubberbandingCommand.Go;
            }
            // has pax delta; wait fully!
            return vehicleData.m_waitCounter >= waitTime ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
        }

        /// <summary>
        /// Determines the rubberbanding unbunching command to be given to the vehicle.
        /// </summary>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <param name="vehicleData">The data of the vehicle in question.</param>
        /// <returns>The rubberbanding unbunching command for the vehicle.</returns>
        public static RubberbandingCommand GetRubberbandingIntentionForVehicle(ushort vehicleID, ref Vehicle vehicleData)
        {
            int targetWaitCounter = 12;
            if (vehicleData.m_waitCounter >= 250)
            {
                // technical limit: we must let them go, otherwise they flip over and appear as if they have just arrived
                // this might explain why sometimes IPT2 vehicles are apparently "stuck" when unbunching.
                return RubberbandingCommand.Go;
            }
            ushort vehicleTransportLine = vehicleData.m_transportLine;
            if (vehicleTransportLine == 0)
            {
                // not part of any user-defined public transport line; no comment from us!
                return RubberbandingCommand.Default;
            }

            // get the instant analysis object
            TransportLineVehicleProgress lineProgress = VehicleLineProgress.GetTransportLineVehicleProgress(vehicleTransportLine);
            if (lineProgress.VehiclesCount < 2)
            {
                // too few vehicles; no need to unbunch!
                return vehicleData.m_waitCounter >= targetWaitCounter ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
            }

            // find our progress in the list
            VehicleLineProgress? selfProgress = lineProgress.GetProgressOf(vehicleID);
            if (!selfProgress.HasValue)
            {
                // ???
                return RubberbandingCommand.Default;
            }

            /*
             * note: for best vanilla correctness, there is actually this arcane "CanLeaveStop" method that checks m_averageInterval and m_trafficLightState0
             * one of its OR conditions checks the condition (interval - state + 2) / 4 <= 0
             * this interval fluctuates, but intuitively, more vehicles in the line results in lower average interval, and it also depends on line length
             * this traffic light state is more arcane; it seems this is 0 for no-light junctions, 2 for red lights, and 8 for green lights
             * it is known TMPE possibly assigns new values to this traffic light state (saw a value of 15 before)
             *
             * anyway, iirc this arcane condition should somehow sync with the current traffic light state (prefer to go at green lights)
             * but this condition most of the time will not trigger, because of the following:
             * it will be true only when interval is smaller than some variable (probably <= 15),
             * but then this interval has to be really really small, and it's quite difficult to push this interval value below 20,
             * so for the most part, it won't trigger and can be safely ignored.
             */

            // ---
            // begin checking!

            if (VehicleHasEnoughUnbunchingSpacing(selfProgress.Value, lineProgress, out float currentSpacing, out float idealSpacing))
            {
                // enough spacing already; go and catch up!
                return vehicleData.m_waitCounter >= targetWaitCounter ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
            }

            /*
             * not enough spacing; possible causes:
             * - natural; no jams, but front vehicle just hasn't got there yet
             * - traffic jam; front vehicle blocked by jam and hasn't got there
             * - redeployment; redeploying to the immediate next stop of current stop will show as "not enough spacing"
             *
             * determine which waiting style to use by checking overcrowdedness.
             * with this, rubberbanding is achieved.
             */

            int bunchedVehiclesCount = CountSelfAndBehindBunchingVehicles(selfProgress.Value, lineProgress);
            if (bunchedVehiclesCount > 3)
            {
                // overcrowding; use fast waiting time
                // for each extra vehicle after the 3rd one, decrease uniformly such that the minimum amount of time waiting is at 16 (1.33x boarding time) with 11 vehicles waiting
                targetWaitCounter = Mathf.Max(64 - (bunchedVehiclesCount - 3) * 6, 16);
            }
            else
            {
                // not overcrowded; use spacing waiting time
                // we apply a curve to guard the wait counter.
                /*
                 * note:
                 * this curve (a multiplier) is designed with the following in mind:
                 * - it becomes 1 when spacing progress reaches 1
                 * - it increases somewhat inversely when spacing progress is less than 1
                 *
                 * we set the increment rate to 2 to hopefully make it less likely to be exceeded by the increasing waiting time itself
                 */
                float standardWaitCounter = 64;
                float curveProgressSpacing = currentSpacing / idealSpacing;
                float curveIncrementRate = 2f;
                float curveFactor = curveIncrementRate * 2;
                targetWaitCounter = (int) ((curveFactor / curveProgressSpacing - (curveFactor - 1)) * standardWaitCounter);
            }

            // decide
            return vehicleData.m_waitCounter >= targetWaitCounter ? RubberbandingCommand.Go : RubberbandingCommand.Hold;
        }

        private static int CountSelfAndBehindBunchingVehicles(VehicleLineProgress vehicleProgress, TransportLineVehicleProgress lineProgress)
        {
            // includes self
            // counts how many vehicles are currently bunched at a stop
            ushort selfVehicleID = vehicleProgress.VehicleID;
            int vehicleCount = 1;
            float detectionSpacing = UnbunchingProximityPercentDist;
            float selfPercentProgress = vehicleProgress.PercentProgress;
            VehicleLineProgress? backVehicleProgress = lineProgress.GetProgressOfBackOf(vehicleProgress.VehicleID);
            while (backVehicleProgress.HasValue && backVehicleProgress.Value.VehicleID != selfVehicleID)
            {
                // check spacing
                VehicleLineProgress innerBackVehicleProgress = backVehicleProgress.Value;
                float backSpacing = selfPercentProgress - innerBackVehicleProgress.PercentProgress;
                if (backSpacing < 0)
                {
                    // wrap around
                    backSpacing += 1;
                }
                if (backSpacing > detectionSpacing)
                {
                    // too far away; stop!
                    break;
                }
                vehicleCount++;
                backVehicleProgress = lineProgress.GetProgressOfBackOf(innerBackVehicleProgress.VehicleID);
            }
            return vehicleCount;
        }

        /// <summary>
        /// Given a vehicle V, returns whether V has unbunched enough that we consider V to have enough spacing with the vehicle in front of V.
        /// </summary>
        /// <param name="vehicleProgress">The vehicle progress of the vehicle V.</param>
        /// <param name="lineProgress"></param>
        /// <param name="currentSpacing">The current unbunching spacing (same unit as vehicle progress) as calculated by this method.</param>
        /// <param name="idealSpacing">The idea unbunching spacing (same unit as vehicle progress) as recommended by this method.</param>
        /// <returns></returns>
        private static bool VehicleHasEnoughUnbunchingSpacing(
            VehicleLineProgress vehicleProgress,
            TransportLineVehicleProgress lineProgress,
            out float currentSpacing,
            out float idealSpacing)
        {
            // determine ideal spacing first
            // this can potentially be exposed as a config for unbunch strength
            float unbunchingBuffer = 0.2f;
            idealSpacing = (1 + unbunchingBuffer) / lineProgress.VehiclesCount;

            // then, check current spacing
            VehicleLineProgress? frontVehicleProgress = lineProgress.GetProgressOfFrontOf(vehicleProgress.VehicleID);
            if (!frontVehicleProgress.HasValue)
            {
                // ???
                currentSpacing = 0;
                return true;
            }

            float progressSpacing = frontVehicleProgress.Value.PercentProgress - vehicleProgress.PercentProgress;
            if (progressSpacing < 0)
            {
                // wrap around
                progressSpacing += 1;
            }
            currentSpacing = progressSpacing;
            return progressSpacing > idealSpacing;
        }
    }
}
