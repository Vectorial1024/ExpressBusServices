using ExpressBusServices.PerformanceBoost;
using ExpressBusServices.Redeployment;
using HarmonyLib;
using System.Reflection;
using ExpressBusServices.DataTypes;

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

            VehiclePaxDeltaInfo.EnsureTableExists();
            CitizenRunawayTable.EnsureTableExists();
            BusStopSkippingLookupTable.EnsureTableExists();
            ServiceBalancerUtil.EnsureTableExists();

            TeleportRedeployInstructions.EnsureTableExists();

            CachedVehicleProperties.TouchAndResetCache();
        }

        public static void Deactivate()
        {
            GetHarmonyInstance().UnpatchAll(HarmonyModID);

            VehiclePaxDeltaInfo.WipeTable();
            CitizenRunawayTable.WipeTable();
            BusStopSkippingLookupTable.WipeTable();
            ServiceBalancerUtil.ResetRedeploymentRecords();

            TeleportRedeployInstructions.WipeTable();

            CachedVehicleProperties.TouchAndResetCache();
        }
    }
}
