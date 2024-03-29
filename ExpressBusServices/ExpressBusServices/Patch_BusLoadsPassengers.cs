﻿using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("LoadPassengers", MethodType.Normal)]
    public class Patch_BusLoadsPassengers
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        public static bool HandleBusAboutToLoadPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusPickDropLookupTable.Notify_PassengersAboutToBoardOntoBus(vehicleID, ref data);
            if (BusStopSkippingLookupTable.BusShouldSkipPassengerLoading(vehicleID))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void HandleBusAlreadyLoadedPassengers(ushort vehicleID, ref Vehicle data)
        {
            BusStopSkippingLookupTable.ForgetBus(vehicleID);
            BusPickDropLookupTable.Notify_PassengersAlreadyBoardedOntoBus(vehicleID, ref data);
        }
    }
}
