using ExpressBusServices.DataTypes;
using ExpressBusServices.Util;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch(nameof(BusAI.CanLeave), MethodType.Normal)]
    public class Patch_BusAiCorrector
    {
        [HarmonyPostfix]
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleDepartureUtil.ReviewDepartureStatus(ref __result, vehicleID, ref vehicleData);
            // BusPickDropLookupTable.DetermineIfBusShouldDepart(ref __result, vehicleID, ref vehicleData);
        }
    }
}
