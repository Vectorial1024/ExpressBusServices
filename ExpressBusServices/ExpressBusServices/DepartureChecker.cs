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
            TransportLine theLine = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleIterator = theLine.m_vehicles;
            List<VehicleLineProgress> progressList = new List<VehicleLineProgress>();
            // StringBuilder builder = new StringBuilder("Vehicle IDs:\n");
            float current, max;
            while (vehicleIterator != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleIterator].Info;
                info.m_vehicleAI.GetProgressStatus(vehicleIterator, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleIterator], out current, out max);
                // the bool return is simply to indicate whether the bus is stopping at a stop.
                // for us, this is still useful.
                if (max != 0)
                {
                    // a valid bus; invalid bus (eg is despawning) will get max = 0
                    VehicleLineProgress progress = new VehicleLineProgress(vehicleIterator, current / max);
                    progressList.Add(progress);
                    // builder.AppendLine(vehicleIterator.ToString());
                }
                vehicleIterator = instance.m_vehicles.m_buffer[vehicleIterator].m_nextLineVehicle;
            }
            // all vehicles found
            if (progressList.Count == 1)
            {
                // invalid operation; no need to unbunch, just go
                return true;
            }

            // sort the list for in-order progress checking
            progressList.Sort(delegate(VehicleLineProgress left, VehicleLineProgress right)
            {
                // sort by the percentage progress
                return left.percentProgress.CompareTo(right.percentProgress);
            });

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
            return distanceToNext > idealDistance;
        }
    }
}
