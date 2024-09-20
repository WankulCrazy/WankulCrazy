using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Rendering;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.patch;
using WankulCrazyPlugin.utils;

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
        MethodInfo patch_setcardui = AccessTools.Method(typeof(ReplacingCards), "SetCardUI");
        harmony.Patch(original_setcardui, postfix: new HarmonyMethod(patch_setcardui));

        MethodInfo original_closeup = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "EnterViewUpCloseState");
        MethodInfo patch_closeup_prefix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePrefix");
        MethodInfo patch_closeup_postfix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePostfix");
        harmony.Patch(original_closeup, prefix: new HarmonyMethod(patch_closeup_prefix), postfix: new HarmonyMethod(patch_closeup_postfix));

        MethodInfo original_CardOpening = AccessTools.Method(typeof(CardOpeningSequence), "GetPackContent");
        MethodInfo patch_CardOpening = AccessTools.Method(typeof(WankulInventory), "CardOpening");
        harmony.Patch(original_CardOpening, postfix: new HarmonyMethod(patch_CardOpening));

        MethodInfo original_save= AccessTools.Method(typeof(CSaveLoad), "Save");
        MethodInfo patch_save = AccessTools.Method(typeof(SavesManager), "SaveCardsAssociations");
        harmony.Patch(original_save, postfix: new HarmonyMethod(patch_save));

        MethodInfo original_load = AccessTools.Method(typeof(CSaveLoad), "Save");
        MethodInfo patch_load = AccessTools.Method(typeof(SavesManager), "LoadCardsAssociations");
        harmony.Patch(original_load, postfix: new HarmonyMethod(patch_load));
    }

}
