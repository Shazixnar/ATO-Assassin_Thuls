using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Obeliskial_Essentials;

namespace AssassinThuls
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInDependency("com.stiffmeds.obeliskialcontent")]
    [BepInProcess("AcrossTheObelisk.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal const int ModDate = 20251225;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");
            // register with Obeliskial Essentials
            Essentials.RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "Shazixnar",
                _description: "Redone Thuls' traits and element cards.",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://across-the-obelisk.thunderstore.io/package/Shazixnar/Assassin_Thuls/",
                _contentFolder: "Assassin Thuls",
                _type: new string[5] { "content", "hero", "trait", "card", "perk" }
            );
            // apply patches
            harmony.PatchAll();
        }
    }
}
