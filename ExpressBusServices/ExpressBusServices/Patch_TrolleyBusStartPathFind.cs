using HarmonyLib;
using System;
using System.Reflection;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [UsedImplicitly]
    public class Patch_TrolleyBusStartPathFind
    {
        [UsedImplicitly]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("TrolleybusAI:StartPathFind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() });
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        public static void AdjustPathfindTargetForRedeployment(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (ServiceBalancerUtil.ReadRedeploymentInstructions(vehicleID, out ushort redeploymentTarget))
            {
                vehicleData.m_targetBuilding = redeploymentTarget;
            }
        }
    }
}
