using ExpressBusServices.Redeployment;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TransportManager))]
    [HarmonyPatch(nameof(TransportManager.ReleaseLine), MethodType.Normal)]
    [UsedImplicitly]
    public class Patch_TransportManager_ReleaseLine
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void CleanUpTransportLine(ushort lineID)
        {
            TeleportRedeployInstructions.NotifyTransportLineDeleted(lineID);
        }
    }
}
