using HarmonyLib;
using UnityEngine;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TransportLine))]
    [HarmonyPatch("AddVehicle", MethodType.Normal)]
    public class Patch_TransportLine_AddVehicle
    {
        [HarmonyPostfix]
        public static void PostFix(ushort vehicleID, ref Vehicle data, bool findTargetStop)
        {
            if (!findTargetStop)
            {
                // we are loading from save; DO NOT modify the vehicle details!
                return;
            }
            ushort transportLineID = data.m_transportLine;
            if (transportLineID == 0)
            {
                return;
            }
            if (BusDepotRedeploymentInstructions.TransportLineReadFutureDeployment(transportLineID, out ushort targetStopID))
            {
                data.m_targetBuilding = targetStopID;
                Debug.Log($"New vehicle of transport line ${transportLineID} now redeploying to stop {targetStopID} as per future instructions.");
            }
        }
    }
}
