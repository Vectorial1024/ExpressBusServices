using HarmonyLib;
using System.Reflection;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.High)]
    public class Patch_IPT2_BusAiCorrector
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetRelevantIPT2Method()
        {
            return AccessTools.TypeByName("ImprovedPublicTransport2.Detour.Vehicles.BusAIDetour")?.GetMethod("CanLeave") ?? null;
        }

        [HarmonyPrepare]
        public static bool DetermineIfShouldPatch()
        {
            return TargetRelevantIPT2Method() != null;
        }

        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            BusPickDropLookupTable.DetermineIfBusShouldDepart(ref __result, vehicleID, ref vehicleData);
        }
    }
}
