using HarmonyLib;
using System;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("StartPathFind", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(Vehicle) })]
    public class Patch_BusStartPathFind
    {
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
