using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("CanLeave", MethodType.Normal)]
    public class Patch_BusAiCorrector
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ref Vehicle vehicleData)
        {
            /*
             * m_transferSize, in the context of bus routes, represents the number of passengers alighting + boarding the bus at a bus stop.
             * For example, if, at a stop, 1 alighted and 1 boarded, m_transferSize becomes 1 + 1 = 2.
             * This value is recalculated whenver the bus arrives at a bus stop.
             */
            // Skips stops whenever there is no one alighting AND boarding.
            if (vehicleData.m_transferSize == 0)
            {
                __result = true;
            }
        }
    }

    /*
    [HarmonyPatch]
    public class Patch_BusAiCorrector_ForIPT2
    {
        // TODO make a separate mod for IPT2
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("ImprovedPublicTransport2.Detour.Vehicles.BusAIDetour:CanLeave");
        }

        public static bool Prepare()
        {
            return false;
        }
    }
    */
}
