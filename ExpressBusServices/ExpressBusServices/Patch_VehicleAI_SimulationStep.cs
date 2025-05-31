using HarmonyLib;
using System;
using System.Reflection;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [UsedImplicitly]
    public class Patch_VehicleAI_SimulationStep
    {
        /*
         * Special thanks to klyte45 from TLM for letting me use this logic.
         */

        [HarmonyTargetMethod]
        [UsedImplicitly]
        public static MethodBase TargetRelevantMethod()
        {
            return AccessTools.Method(typeof(VehicleAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(int) });
        }

        [HarmonyPrepare]
        [UsedImplicitly]
        public static bool DetermineIfShouldPatch()
        {
            // this method should exist
            // however, dont do it if TLM is detected
            return !ModDetector.TransportLinesManagerIsLoaded();
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        public static void PreSimulationStep(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine != 0 && vehicleData.m_path == 0 && (vehicleData.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
                vehicleData.Info.m_vehicleAI.SetTransportLine(vehicleID, ref vehicleData, 0);
            }
        }
    }
}
