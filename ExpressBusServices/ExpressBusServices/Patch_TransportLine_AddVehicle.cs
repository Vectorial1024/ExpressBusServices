using ExpressBusServices.Redeployment;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TransportLine))]
    [HarmonyPatch(nameof(TransportLine.AddVehicle), MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_TransportLine_AddVehicle
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void CheckRedeploymentInstructions(ushort vehicleID, ref Vehicle data, bool findTargetStop)
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
            if (TeleportRedeployInstructions.TransportLineReadFutureDeployment(transportLineID, out ushort targetStopID))
            {
                data.m_targetBuilding = targetStopID;
                // Debug.Log($"New vehicle of transport line ${transportLineID} now redeploying to stop {targetStopID} as per future instructions.");
            }
        }
    }
}
