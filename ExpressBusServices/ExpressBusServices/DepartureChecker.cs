using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using Epic.OnlineServices.Presence;
using ExpressBusServices.DataTypes;
using UnityEngine;
using static RenderManager;

namespace ExpressBusServices
{
    public class DepartureChecker
    {
        // if true, then this mod will intervene in handling instant departures etc.
        public static bool NowIsEligibleForInstantDeparture(ushort vehicleID, ref Vehicle vehicleData)
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

        [Obsolete("Pleasse refactor to use StopIsConsideredTerminus instead.")]
        public static bool StopIsTerminus(ushort stop)
        {
            // none are terminus; however this will be overridden by the TLM extension mod
            return false;
        }

        public static bool StopIsConsideredAsTerminus(ushort stopID, ushort transportLineID)
        {
            // both IPT2 and TLM will override this, to make the things more streamlined
            // return true if this should be considered a terminus
            // buses will unbunch when at terminus

            TransportManager transportManager = Singleton<TransportManager>.instance;
            ushort firstStopID = transportManager.m_lines.m_buffer[transportLineID].GetLastStop();

            // architecture reversal
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

        public static bool RecheckUnbunchingCanLeave(ushort vehicleID, ref Vehicle vehicleData)
        {
            // mainly for IPT2; there is probably some side effect that is caused by how the IPT2 plugin is influencing the work of IPT2 itself
            // this aims to remedy that.
            // this is also a place to further extend the unbunching checking, so that we can implement the so-called rapid deployment feature.
            bool canLeave = Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].CanLeaveStop(vehicleData.m_targetBuilding, vehicleData.m_waitCounter >> 4);
            // todo recalculate with a different waiting time when the budget is not at 100%.
            if (VehicleLineProgressNeedToCatchUp(vehicleID, ref vehicleData))
            {
                return true;
            }
            return canLeave;
        }

        public static bool RecheckUnbunchingShouldStay(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_waitCounter >= 250)
            {
                // technical limit: we must leave them go, otherwise they flip over and appear as if they have just arrived
                return false;
            }
            return VehicleLineProgressNeedToChill(vehicleID, ref vehicleData);
        }

        private static bool VehicleLineProgressNeedToCatchUp(ushort vehicleID, ref Vehicle vehicleData)
        {
            ushort transportLine = vehicleData.m_transportLine;
            if (transportLine == 0)
            {
                return false;
            }

            /*
             * Objectievs:
             * 1. Locate the vehicle in the line
             * 2. Locate the previous vehicle in the line (thus calculating the progress)
             * 3. Count number of vehicles in the line
             */
            List<VehicleLineProgress> progressList = VehicleLineProgress.GetProgressList(transportLine);
            if (progressList.Count < 2)
            {
                // invalid operation: too few buses, no need to unbunch, just catch up is ok
                return true;
            }

            // print the list for reference
            /*
            StringBuilder builder2 = new StringBuilder("Progresses:\n");
            foreach (VehicleLineProgress prog in progressList)
            {
                builder2.AppendLine(prog.percentProgress.ToString());
            }
            Debug.Log(builder2.ToString());
            */

            // find this vehicle and its "previous" vehicle
            int indexOfThis = 0;
            int indexOfNext = 0;
            for (int i = 0; i < progressList.Count; i++)
            {
                if (progressList[i].vehicleID == vehicleID)
                {
                    indexOfThis = i;
                    indexOfNext = i + 1;
                    break;
                }
            }
            // must exists
            if (indexOfNext >= progressList.Count)
            {
                // wrap around
                indexOfNext = 0;
            }

            // calculate expected distance
            // this can potentially be exposed as a config for unbunch strength
            float unbunchingBuffer = 0.2f;
            float idealDistance = (1 + unbunchingBuffer) / progressList.Count;

            // check expected distance
            float distanceToNext = progressList[indexOfNext].percentProgress - progressList[indexOfThis].percentProgress;
            if (distanceToNext < 0)
            {
                // wrap around
                distanceToNext += 1;
            }

            // decide: is the distance too large?
            /*
            String message = "Distance compare " + distanceToNext + " " + idealDistance;
            if (distanceToNext > idealDistance)
            {
                message += " GO";
            }
            Debug.Log(message);
            */
            if (distanceToNext > idealDistance)
            {
                return true;
            }

            // second layer:
            // the shouldChill has released control when number of waiting vehicles exceed 3
            // when there are more than enough vehicles waiting at the stop, reduce the waiting time and check again.
            if (progressList.Count < 3)
            {
                return false;
            }
            float selfPercentProgress = progressList[indexOfThis].percentProgress;
            float percentageDistance = 0.1f;
            int waitingVehicles = 0;
            int loopingIndex = indexOfThis;
            int iterationCount = 0;
            while (true)
            {
                if (loopingIndex == 0)
                {
                    loopingIndex = progressList.Count;
                }
                loopingIndex--;
                VehicleLineProgress currentProgress = progressList[loopingIndex];
                if (currentProgress.vehicleID == vehicleID)
                {
                    // we somehow reached ourselves!
                    break;
                }
                float percentProgress = progressList[loopingIndex].percentProgress;
                float currentPercentDistance = selfPercentProgress - percentProgress;
                if (currentPercentDistance < 0)
                {
                    currentPercentDistance += 1;
                }
                if (currentPercentDistance >= percentageDistance)
                {
                    // out of range
                    break;
                }
                if (++iterationCount >= 16384)
                {
                    // huh? bad list?
                    break;
                }
                waitingVehicles++;
            }
            // for each extra vehicle after the 3rd one, decrease uniformly such that the minimum amount of time waiting is at 24 (2x boarding time)
            int fasterWaitingTime = Mathf.Max(64 - (Mathf.Max(waitingVehicles - 3, 0)) * 5, 24);
            return vehicleData.m_waitCounter > fasterWaitingTime;
        }

        private static bool VehicleLineProgressNeedToChill(ushort vehicleID, ref Vehicle vehicleData)
        {
            ushort transportLine = vehicleData.m_transportLine;
            if (transportLine == 0)
            {
                return false;
            }

            /*
             * Objectievs:
             * 1. Locate the vehicle in the line
             * 2. Locate the previous vehicle in the line (thus calculating the progress)
             * 3. Count number of vehicles in the line
             */
            List<VehicleLineProgress> progressList = VehicleLineProgress.GetProgressList(transportLine);
            if (progressList.Count < 2)
            {
                // invalid operation: too few buses, no need to chill, just catch up is ok
                return false;
            }

            // print the list for reference
            /*
            StringBuilder builder2 = new StringBuilder("Progresses:\n");
            foreach (VehicleLineProgress prog in progressList)
            {
                builder2.AppendLine(prog.percentProgress.ToString());
            }
            Debug.Log(builder2.ToString());
            */

            // find this vehicle and its "previous" vehicle
            int indexOfThis = 0;
            int indexOfNext = 0;
            for (int i = 0; i < progressList.Count; i++)
            {
                if (progressList[i].vehicleID == vehicleID)
                {
                    indexOfThis = i;
                    indexOfNext = i + 1;
                    break;
                }
            }
            // must exists
            if (indexOfNext >= progressList.Count)
            {
                // wrap around
                indexOfNext = 0;
            }

            // check if it is overcrowded: if overcrowded, then use the vanilla method of unbunching
            // we check those within 2 progress percent, whether there are more than 3 buses waiting (including self)
            if (progressList.Count > 3)
            {
                float overcrowdProgressDistance = 0.02f;
                int indexOfQueueing = indexOfThis;
                int crowdedCount = 0;
                VehicleLineProgress selfProgress = progressList[indexOfThis];
                while (true)
                {
                    VehicleLineProgress queueing = progressList[indexOfQueueing];
                    float progressDistance = selfProgress.percentProgress - queueing.percentProgress;
                    if (progressDistance < 0)
                    {
                        progressDistance += 1;
                    }
                    if (progressDistance > overcrowdProgressDistance)
                    {
                        // went out of range, and there aren't enough buses
                        break;
                    }
                    crowdedCount++;
                    if (crowdedCount > 3)
                    {
                        // too many buses near here; don't wait!
                        return false;
                    }
                    // check next
                    if (indexOfQueueing == 0)
                    {
                        indexOfQueueing = progressList.Count;
                    }
                    indexOfQueueing--;
                }
            }

            // calculate expected distance
            // this can potentially be exposed as a config for unbunch strength
            /*
             * note: because we are dealing with minimum unbunching distance, we CANNOT set any hard limits
             * the reason is obvious: what if there is a traffic jam? if we set a hard minimum limit, then we are simply back to vanilla unbunching
             * and that is not ok
             * instead, make use of vanilla's idea to use waiting time, and extend upon it:
             * if the buses are too close, let the latter bus wait longer, but not wait infinitely.
             */
            float unbunchingThreshold = 0.2f;
            // the way the distance is calculated is similar to the above "catchup" function
            float checkingDistance = (1 - unbunchingThreshold) / progressList.Count;

            // check expected distance
            float distanceToNext = progressList[indexOfNext].percentProgress - progressList[indexOfThis].percentProgress;
            if (distanceToNext < 0)
            {
                // wrap around
                distanceToNext += 1;
            }

            if (distanceToNext > checkingDistance)
            {
                // has met the threshold. the game can decide whether can unbunch (based on waiting time)
                return false;
            }

            // distance is below threshold
            // we apply a hyperbolic curve to guard the waiting time.
            float standardWaitingTime = 64;
            float curveProgressPercent = distanceToNext / checkingDistance;
            float curveIncrementRate = 2f;
            float curveFactor = curveIncrementRate * 2;
            float designatedWaitingTime = (curveFactor / curveProgressPercent - (curveFactor - 1)) * standardWaitingTime;

            // decide: is the waiting time too small?
            String message = "Waiting time compare " + vehicleData.m_waitCounter + " " + designatedWaitingTime;
            if (vehicleData.m_waitCounter < designatedWaitingTime)
            {
                message += " CHILL";
            }
            // Debug.Log(message);

            return vehicleData.m_waitCounter < designatedWaitingTime;
        }
    }
}
