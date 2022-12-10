using ExpressBusServices.Redeployment;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(TransportManager))]
    [HarmonyPatch("ReleaseLine", MethodType.Normal)]
    public class Patch_TransportManager_ReleaseLine
    {
        [HarmonyPostfix]
        public static void PostFix(ushort lineID)
        {
            TeleportRedeployInstructions.NotifyTransportLineDeleted(lineID);
        }
    }
}
