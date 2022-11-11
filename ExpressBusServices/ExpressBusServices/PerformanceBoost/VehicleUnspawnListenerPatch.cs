using HarmonyLib;

namespace ExpressBusServices.PerformanceBoost
{
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("Unspawn", MethodType.Normal)]
    public class VehicleUnspawnListenerPatch
    {
        [HarmonyPostfix]
        public static void OnUnspawn(ushort vehicleID)
        {
            CachedVehicleProperties.UnsetCache(vehicleID);
        }
    }
}
