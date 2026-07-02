using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices.Patches.Bus
{
    [HarmonyPatch]
    [UsedImplicitly]
    public class Patch_BusStartPathFind
    {
        [UsedImplicitly]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("BusAI:StartPathFind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() });
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
