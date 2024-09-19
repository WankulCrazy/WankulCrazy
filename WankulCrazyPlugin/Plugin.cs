using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Rendering;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.patch;

namespace WankulCrazyPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        MethodInfo original_start = AccessTools.Method(typeof(TitleScreen), "Start");
        MethodInfo patch_start = AccessTools.Method(typeof(GameStarting), "Start");
        harmony.Patch(original_start, new HarmonyMethod(patch_start));

        MethodInfo original_setcardui = AccessTools.Method(typeof(CardUI), "SetCardUI");
        MethodInfo patch_setcardui = AccessTools.Method(typeof(ReplacingAllCards), "SetCardUI_patch");
        harmony.Patch(original_setcardui, postfix: new HarmonyMethod(patch_setcardui));

        MethodInfo original_CardOpening = AccessTools.Method(typeof(CardOpeningSequence), "GetPackContent");
        MethodInfo patch_CardOpening = AccessTools.Method(typeof(WankulInventory), "CardOpening");
        harmony.Patch(original_CardOpening, postfix: new HarmonyMethod(patch_CardOpening));
    }

}
