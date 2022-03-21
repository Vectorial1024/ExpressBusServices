using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
using System.Linq;

namespace ExpressBusServices
{
    internal class ModDetector
    {
        public static readonly ulong MODID_KLYTE45_TRANSPORT_LINES_MANAGER = 1312767991;

        public static bool TransportLinesManagerIsLoaded()
        {
            // detect official release first, and then detect local/unofficial builds
            // this is assuming that the local/unofficial builds have a similar structure to the known builds.
            if (VerifyModEnabled(MODID_KLYTE45_TRANSPORT_LINES_MANAGER))
            {
                return true;
            }
            return AccessTools.TypeByName("Klyte.TransportLinesManager.TLMController")?.GetMethod("AutoColor") != null;
        }

        private static bool VerifyModEnabled(ulong modId)
        {
            PluginManager.PluginInfo pluginInfo = Singleton<PluginManager>.instance.GetPluginsInfo().FirstOrDefault((PluginManager.PluginInfo pi) => pi.publishedFileID.AsUInt64 == modId);
            return pluginInfo != null && pluginInfo.isEnabled && pluginInfo.overrideState == PluginManager.OverrideState.Enabled;
        }
    }
}
