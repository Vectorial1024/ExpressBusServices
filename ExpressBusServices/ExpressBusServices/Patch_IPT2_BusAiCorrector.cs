using ColossalFramework;
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
            return AccessTools.Method("ImprovedPublicTransport2.Detour.Vehicles.BusAIDetour:CanLeave");
        }

        [HarmonyPrepare]
        public static bool DetermineIfShouldPatch()
        {
            return TargetRelevantIPT2Method() != null;
        }

        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            /*
             * Refer to the main class Patch_BusAiCorrector for more details.
             * Changes to this class should be made to the main class first and then mirrored here
             * because the two classes are essentially checking the same thing in two different places.
             */
            TransportManager instance = Singleton<TransportManager>.instance;
            ushort transportLineID = vehicleData.m_transportLine;
            ushort firstStop = instance.m_lines.m_buffer[transportLineID].GetStop(1);
            ushort currentStop = vehicleData.m_targetBuilding;
            if (currentStop != firstStop)
            {
                VehicleBAInfo info = BusPickDropLookupTable.GetInfoForBus(vehicleID);
                if (info != null && info.Alighted + info.Boarded == 0)
                {
                    __result = true;
                }
                else if (vehicleData.m_waitCounter >= 12 && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                {
                    __result = true;
                }
            }
        }
    }
}
