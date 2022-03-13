using HarmonyLib;

namespace ExpressBusServices
{
    internal class ModDetector
    {
        public static bool TransportLinesManagerIsLoaded()
        {
            return AccessTools.TypeByName("Klyte.TransportLinesManager.TLMController")?.GetMethod("AutoColor") != null;
        }
    }
}
