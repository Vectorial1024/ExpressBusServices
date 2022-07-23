using HarmonyLib;
using System;
using System.Reflection;

namespace ExpressBusServices
{
    [HarmonyPatch]
    public class Patch_BusStartPathFind
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("BusAI:StartPathFind", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() });
        }

        [HarmonyPrefix]
        public static void AdjustPathfindTargetForRedeployment(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (ServiceBalancerUtil.PopRedeploymentInstructions(vehicleID, out ushort redeploymentTarget))
            {
                vehicleData.m_targetBuilding = redeploymentTarget;
            }
        }
    }
}
