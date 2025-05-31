using ColossalFramework;
using System;
using UnityEngine;

namespace ExpressBusServices
{
    /*
     * Special thanks to klyte45 from TLM for letting me use this logic.
     */
    public static class TransportLineUtil
    {
        public static void CountPassengersWaiting(ushort currentStop, out int residents, out int tourists)
        {
            int residentsIn = 0;
            int touristsIn = 0;
            var cm = CitizenManager.instance;
            DoWithEachPassengerWaiting(currentStop, (citizen) =>
            {
                if ((cm.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
                {
                    touristsIn++;
                }
                else
                {
                    residentsIn++;
                }
            });

            residents = residentsIn;
            tourists = touristsIn;
        }

        public static void DoWithEachPassengerWaiting(ushort currentStop, Action<ushort> actionToDo)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[nextStop].m_position;
            nm.m_nodes.m_buffer[currentStop].m_maxWaitTime = 0;
            int minX = Mathf.Max((int)((position.x - 72) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int)((position.z - 72) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int)((position.x + 72) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int)((position.z + 72) / 8f + 1080f), 2159);
            int zIterator = minZ;
            while (zIterator <= maxZ)
            {
                int xIterator = minX;
                while (xIterator <= maxX)
                {
                    ushort citizenIterator = cm.m_citizenGrid[(zIterator * 2160) + xIterator];
                    int loopCounter = 0;
                    while (citizenIterator != 0)
                    {
                        ushort nextGridInstance = cm.m_instances.m_buffer[citizenIterator].m_nextGridInstance;
                        if ((cm.m_instances.m_buffer[citizenIterator].m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None)
                        {
                            Vector3 a = cm.m_instances.m_buffer[citizenIterator].m_targetPos;
                            float distance = Vector3.SqrMagnitude(a - position);
                            if (distance < 8196f)
                            {
                                CitizenInfo info = cm.m_instances.m_buffer[citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[citizenIterator], position, position2))
                                {
                                    actionToDo(citizenIterator);
                                }
                            }
                        }
                        citizenIterator = nextGridInstance;
                        if (++loopCounter > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                    xIterator++;
                }
                zIterator++;
            }
        }

        public static int GetQuantityPassengerUnloadOnNextStop(ushort vehicleId, ref Vehicle data, out bool full, out bool empty)
        {
            var firstVehicle = data.GetFirstVehicle(vehicleId);
            if (firstVehicle != vehicleId)
            {
                return GetQuantityPassengerUnloadOnNextStop(firstVehicle, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicle], out full, out empty);
            }
            if (data.m_transportLine == 0)
            {
                full = false;
                empty = false;
                return -1;
            }
            var stopNodeId = data.m_targetBuilding;
            if (stopNodeId == 0)
            {
                full = false;
                empty = false;
                return 0;
            }
            NetManager nmInstance = NetManager.instance;
            Vector3 stopPos = nmInstance.m_nodes.m_buffer[stopNodeId].m_position;
            ushort nextStop = TransportLine.GetNextStop(stopNodeId);
            bool forceUnload = nextStop == 0;

            int serviceCounter = 0;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint numCitizenUnits = instance.m_units.m_size;
            uint num2 = data.m_citizenUnits;
            int num3 = 0;
            while (num2 != 0U)
            {
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num2)].GetCitizen(i);
                    if (citizen != 0U)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            if (!DryRun_TransportArriveAtTarget(ref instance.m_instances.m_buffer[instance2], stopPos, forceUnload))
                            {
                                serviceCounter++;
                            }
                        }
                    }
                }
                num2 = instance.m_units.m_buffer[(int)((UIntPtr)num2)].m_nextUnit;
                if (++num3 > numCitizenUnits)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            data.Info.m_vehicleAI.GetBufferStatus(vehicleId, ref data, out _, out int passengers, out int capacity);
            full = capacity - passengers <= 0;
            empty = passengers == 0;
            return passengers - serviceCounter;
        }

        // anything below this line, I am not exactly sure what they mean, but they "just work"

        private static bool DryRun_TransportArriveAtTarget(ref CitizenInstance citizenData, Vector3 stopPos, bool forceUnload)
        {
            PathManager instance = Singleton<PathManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            if ((citizenData.m_flags & CitizenInstance.Flags.OnTour) == CitizenInstance.Flags.OnTour)
            {
                if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.TargetIsNode)
                {
                    ushort targetBuilding = citizenData.m_targetBuilding;
                    if (targetBuilding != 0 && Vector3.SqrMagnitude(instance2.m_nodes.m_buffer[targetBuilding].m_position - stopPos) < 4f)
                    {
                        return false;
                    }
                }
                return true;
            }
            var pathPosIdx = citizenData.m_pathPositionIndex;
            var targetPath = citizenData.m_path;

            IncrementPath(instance, ref pathPosIdx, ref targetPath);
            if (targetPath != 0U)
            {
                if (instance.m_pathUnits.m_buffer[(int)((UIntPtr)targetPath)].GetPosition(pathPosIdx >> 1, out PathUnit.Position pathPos2))
                {
                    uint laneID2 = PathManager.GetLaneID(pathPos2);

                    var pathPositionTarget2 = instance2.m_lanes.m_buffer[(int)((UIntPtr)laneID2)].CalculatePosition(1 - (pathPos2.m_offset / 255f));
                    var distNext2 = Vector3.SqrMagnitude(pathPositionTarget2 - stopPos);
                    //if (TransportLinesManagerMod.DebugMode)
                    //{
                      //  Vector3 pathPositionTarget = instance2.m_lanes.m_buffer[(int)((UIntPtr)laneID2)].CalculatePosition(pathPos2.m_offset / 255f);
                        //float distNext = Vector3.SqrMagnitude(pathPositionTarget - stopPos);
                        //LogUtils.DoLog($"pathOffset = {pathPos2.m_offset} ({pathPos2.m_offset / 255f}), lane = {pathPos2.m_lane} ({laneID2}), segment = {pathPos2.m_segment}, distNext = {distNext}, distNext2 = {distNext2}");
                    //}

                    if (distNext2 < 4f)
                    {
                        if (!forceUnload)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void IncrementPath(PathManager instance, ref byte pathPosIdx, ref uint targetPath)
        {
            if (targetPath != 0U)
            {
                pathPosIdx += 2;
                if (pathPosIdx >> 1 >= instance.m_pathUnits.m_buffer[targetPath].m_positionCount)
                {
                    targetPath = instance.m_pathUnits.m_buffer[targetPath].m_nextPathUnit;
                    pathPosIdx = 0;
                }
            }
        }
    }
}
