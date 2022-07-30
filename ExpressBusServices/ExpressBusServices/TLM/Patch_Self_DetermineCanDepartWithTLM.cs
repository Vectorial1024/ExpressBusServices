using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExpressBusServices.TLM
{
    [HarmonyPatch(typeof(DepartureChecker))]
    [HarmonyPatch("StopIsConsideredAsTerminus", MethodType.Normal)]
    public class Patch_Self_DetermineCanDepartWithTLM
    {
        private static Type Type_TLM_TLMStopDataContainer = null;

        static Patch_Self_DetermineCanDepartWithTLM()
        {
            try
            {
                if (ModDetector.TransportLinesManagerIsLoaded())
                {
                    Type_TLM_TLMStopDataContainer = AccessTools.TypeByName("Klyte.TransportLinesManager.Extensions.TLMStopDataContainer");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Express Bus Services: reflection error. " + ex.ToString());
            }
        }

        [HarmonyPrepare]
        public static bool CheckIfShouldPatch()
        {
            // dont do this patch if Pawnmorpher is detected; there are race conditions
            // if Pawnmorpher is loaded then we use PostFix_Pawnmorpher_HealthUtil instead.
            return ModDetector.TransportLinesManagerIsLoaded();
        }

        // post fix the "is this a terminus stop" to cater for TLM terminus cases
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort stopID)
        {
            // could this be a TLM bus terminus?
            // note that the first stop in line is not a "terminus by computation"; instead, it is a "terminus by definition"
            // and so if we use this "terminus by computation" method, we will not read first stops correctly.
            // we will also need to preserve the value from our base mod

            // code: __result |= TLMStopDataContainer.Instance.SafeGet(stopID).IsTerminal;

            if (Type_TLM_TLMStopDataContainer == null)
            {
                return;
            }

            // we will let the error flow out then
            // var methodGetInstance = Type_TLM_TLMStopDataContainer.GetProperty("Instance").GetGetMethod();


            /*
            __result |= TLMStopDataContainer.Instance.SafeGet(stopID).IsTerminal;
            if (false)
            {

            }
            */
        }
    }
}
