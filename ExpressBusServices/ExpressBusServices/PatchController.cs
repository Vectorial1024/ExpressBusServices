using HarmonyLib;
using System.Reflection;

namespace ExpressBusServices
{
    internal class PatchController
    {
        public static string HarmonyModID
        {
            get
            {
                return "com.vectorial1024.cities.ebs";
            }
        }

        /*
         * The "singleton" design is pretty straight-forward.
         */

        private static Harmony harmony;

        public static Harmony GetHarmonyInstance()
        {
            if (harmony == null)
            {
                harmony = new Harmony(HarmonyModID);
            }

            return harmony;
        }

        public static void Activate()
        {
            GetHarmonyInstance().PatchAll(Assembly.GetExecutingAssembly());
            BusPickDropLookupTable.EnsureTableExists();
            CitizenRunawayTable.EnsureTableExists();
            BusStopSkippingLookupTable.EnsureTableExists();
            ServiceBalancerUtil.EnsureTableExists();
        }

        public static void Deactivate()
        {
            GetHarmonyInstance().UnpatchAll(HarmonyModID);
            BusPickDropLookupTable.WipeTable();
            CitizenRunawayTable.WipeTable();
            BusStopSkippingLookupTable.WipeTable();
            ServiceBalancerUtil.ResetRedeploymentRecords();
        }
    }
}
