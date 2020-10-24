using ColossalFramework;
using HarmonyLib;

namespace ExpressBusServices
{
    [HarmonyPatch(typeof(BusAI))]
    [HarmonyPatch("CanLeave", MethodType.Normal)]
    public class Patch_BusAiCorrector
    {
        [HarmonyPostfix]
        public static void PostFix(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            /*
             * After reconsidering how everything works, I've decided to make use of 
             * one large static dictionary to store info of alight+board for each bus UID.
             * 
             * Previously it was found that simply reading the vehicle info is not enough to
             * determine alight+board count because alighting uses another variable.
             * Eventually I made use of two Harmony patches to read the alight+board info
             * and write it to the static dictionary.
             * 
             * The logic is now sth like this:
             * 
             * When bus arrives at stop:
             * Write down alighting+boarding count
             * Determine if the bus can leave now given the alighting, boarding, and n-th stop info
             * 
             * It is also determined that storing stop UID is not necessary because
             * each bus can only stop at one stop at any given point in time.
             * Stopping at a new stop will simply invalidate the previous stop info.
             * 
             * This entire system has a nice property that no save-file access is needed.
             */
            TransportManager instance = Singleton<TransportManager>.instance;
            ushort transportLineID = vehicleData.m_transportLine;
            ushort firstStop = instance.m_lines.m_buffer[transportLineID].GetStop(1);
            ushort currentStop = vehicleData.m_targetBuilding;
            if (currentStop != firstStop)
            {
                // midway bus stop; implement logic here
                VehicleBAInfo info = BusPickDropLookupTable.GetInfoForBus(vehicleID);
                if (info != null && info.Alighted + info.Boarded == 0)
                {
                    // this bus did not get any alight or board
                    // then depart now
                    __result = true;
                }
                else if (vehicleData.m_waitCounter >= 12 && ReversePatch_VehicleAI_CanLeave.BaseVehicleAI_CanLeave(null, vehicleID, ref vehicleData))
                {
                    // this bus has alight+board but has waited enough to qualify departure AND all citizens are boarded
                    // then depart now
                    // (the "all citizens boarded" part is more obvious when e.g. Realistic Walking Speed mod is used)
                    __result = true;
                }
            }
        }
    }
}
