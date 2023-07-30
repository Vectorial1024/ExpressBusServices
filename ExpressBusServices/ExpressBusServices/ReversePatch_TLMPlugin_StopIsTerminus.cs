using System;
using System.Reflection;
using HarmonyLib;

namespace ExpressBusServices
{
	[HarmonyPatch]
	public class ReversePatch_TLMPlugin_StopIsTerminus
	{
		internal static bool PatchIsSuccessful_HasTLM { get; set; }

		[HarmonyPrepare]
		public static bool ShouldPatch()
		{
			return TargetMethod() != null;
		}

		[HarmonyTargetMethod]
		public static MethodBase TargetMethod()
        {
			MethodInfo targetMethod = AccessTools.Method("ExpressBusServices_TLM.DepartureChecker_TLM:TLM_StopIsTerminus");
			PatchIsSuccessful_HasTLM = targetMethod != null;
			return targetMethod;
		}

		[HarmonyReversePatch]
		public static bool StopIsConsideredTerminus(ushort stopID)
		{
			throw new NotImplementedException("This is a stub that is not available at this moment.");
		}
	}
}
