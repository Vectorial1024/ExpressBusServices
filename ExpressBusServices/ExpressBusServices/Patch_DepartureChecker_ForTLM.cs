using HarmonyLib;
using Klyte.TransportLinesManager.Extensions;
using UnityEngine;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(DepartureChecker))]
    [HarmonyPatch("NowIsEligibleForInstantDeparture", MethodType.Normal)]
    public class Patch_DepartureChecker_ForTLM
    {
        // post fix the "can we instant-depart here" to cater for TLM terminus cases
        // this is supposed to go into a separate mod, but for simplicity/convenience, for practicality, for experimentation, and for Workshop politics,
        // we will refrain from making a new mod for TLM compatibility.
        // instead of a full mod, we will include its only patch file here
        // we will use "raw" reflection to see whether we can detect TLM
        // and then read from TLM's terminus config to determine if we should unbunch.
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            // could this be a TLM bus terminus?
            ushort currentStop = TransportLine.GetPrevStop(vehicleData.m_targetBuilding);
            if (TLMStopDataContainer.Instance.SafeGet(currentStop).IsTerminal)
            {
                // it is a terminal; we cannot allow unbunching here.
                Debug.Log("Stop with UID " + currentStop + " is a terminus.");
                __result = false;
            }
        }
    }
}
