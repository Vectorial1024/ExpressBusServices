using System;
using HarmonyLib;

namespace ExpressBusServices
{
	[HarmonyPatch(typeof(VehicleAI))]
	[HarmonyPatch(nameof(VehicleAI.CanLeave), MethodType.Normal)]
	public class ReversePatch_VehicleAI_CanLeave
    {
		[HarmonyReversePatch]
		public static bool BaseVehicleAI_CanLeave(object __instance, ushort vehicleID, ref Vehicle vehicleData)
		{
			throw new NotImplementedException("This is a stub that is not available at this moment.");
		}
	}
}
