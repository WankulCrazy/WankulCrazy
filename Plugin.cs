﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
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
        MethodInfo patch_setcarduiprefix = AccessTools.Method(typeof(ReplacingCards), "SetCardUIPrefix");
        MethodInfo patch_setcarduipostfix = AccessTools.Method(typeof(ReplacingCards), "SetCardUIPostFix");
        harmony.Patch(original_setcardui, prefix: new HarmonyMethod(patch_setcarduiprefix), postfix: new HarmonyMethod(patch_setcarduipostfix));

        MethodInfo original_closeup = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "EnterViewUpCloseState");
        MethodInfo patch_closeup_prefix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePrefix");
        MethodInfo patch_closeup_postfix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePostfix");
        harmony.Patch(original_closeup, prefix: new HarmonyMethod(patch_closeup_prefix), postfix: new HarmonyMethod(patch_closeup_postfix));

        MethodInfo original_CardOpening = AccessTools.Method(typeof(CardOpeningSequence), "GetPackContent");
        MethodInfo patch_CardOpening = AccessTools.Method(typeof(CardOpening), "OpenBooster");
        harmony.Patch(original_CardOpening, postfix: new HarmonyMethod(patch_CardOpening));

        MethodInfo original_binderSetCard = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "UpdateBinderAllCardUI");
        MethodInfo patch_binderSetCard = AccessTools.Method(typeof(ReplacingCards), "UpdateBinderAllCardUI");
        harmony.Patch(original_binderSetCard, postfix: new HarmonyMethod(patch_binderSetCard));

        MethodInfo original_save = AccessTools.Method(typeof(CSaveLoad), "Save");
        MethodInfo patch_save = AccessTools.Method(typeof(Saves), "Save");
        harmony.Patch(original_save, postfix: new HarmonyMethod(patch_save));

        MethodInfo original_load = AccessTools.Method(typeof(CSaveLoad), "Load");
        MethodInfo patch_load = AccessTools.Method(typeof(Saves), "Load");
        harmony.Patch(original_load, postfix: new HarmonyMethod(patch_load));

        // Récupère la méthode originale à patcher en spécifiant les paramètres (ici sans paramètres)
        MethodInfo originalMethod1 = AccessTools.Method(typeof(CPlayerData), "GetCardMarketPrice", new[] { typeof(CardData) });
        MethodInfo patchMethod1 = AccessTools.Method(typeof(CardPrice), nameof(CardPrice.Postfix_GetCardMarketPrice_CardData));
        harmony.Patch(originalMethod1, postfix: new HarmonyMethod(patchMethod1));

        MethodInfo originalMethod2 = AccessTools.Method(typeof(CPlayerData), "GetCardMarketPrice", new[] { typeof(int), typeof(ECardExpansionType), typeof(bool) });
        MethodInfo patchMethod2 = AccessTools.Method(typeof(CardPrice), nameof(CardPrice.Postfix_GetCardMarketPrice_ThreeParams));
        harmony.Patch(originalMethod2, postfix: new HarmonyMethod(patchMethod2));

        MethodInfo original_InitCardPhone = AccessTools.Method(typeof(CheckPricePanelUI), "InitCard");
        MethodInfo patch_InitCardPhone = AccessTools.Method(typeof(ReplacingCards), "InitCard");
        harmony.Patch(original_InitCardPhone, postfix: new HarmonyMethod(patch_InitCardPhone));
    }

}