using HarmonyLib;
using System.Reflection;

namespace ExpressBusServices
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.High)]
    public class Patch_IPT2_TrolleyBusAiCorrector
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetRelevantIPT2Method()
        {
            return AccessTools.Method("ImprovedPublicTransport2.Detour.Vehicles.TrolleybusAIDetour:CanLeave");
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
