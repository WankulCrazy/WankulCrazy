using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using WankulCrazyPlugin.patch;
using UnityEngine;
using System;
using WankulCrazyPlugin.importer;

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

        //EnumExtensions.GenerateRemappedEnumValues();

        // 🎯 Patch `(int)myitem.type`
        MethodInfo original_ConvI4 = AccessTools.Method(typeof(EItemType), "ToInt32");
        MethodInfo patch_ConvI4 = AccessTools.Method(typeof(Patch_Enum_Transpiler), "HandleCustomEnumValue");
        if (original_ConvI4 != null)
        {
            harmony.Patch(original_ConvI4, postfix: new HarmonyMethod(patch_ConvI4));
        }

        // 🎯 Patch `==` et `!=`
        MethodInfo original_Comparison = AccessTools.Method(typeof(EItemType), "op_Equality");
        MethodInfo patch_Comparison = AccessTools.Method(typeof(Patch_Enum_Comparison), "HandleEnumComparison");
        if (original_Comparison != null)
        {
            harmony.Patch(original_Comparison, postfix: new HarmonyMethod(patch_Comparison));
        }

        // 🎯 Patch `Enum.GetName()`
        MethodInfo original_GetName = AccessTools.Method(typeof(Enum), "GetName", new Type[] { typeof(Type), typeof(object) });
        MethodInfo patch_GetName = AccessTools.Method(typeof(Patch_Enum_GetName), "Prefix");
        if (original_GetName != null)
        {
            harmony.Patch(original_GetName, prefix: new HarmonyMethod(patch_GetName));
        }

        // 🎯 Patch `Enum.IsDefined()`
        MethodInfo original_IsDefined = AccessTools.Method(typeof(Enum), "IsDefined", new Type[] { typeof(Type), typeof(object) });
        MethodInfo patch_IsDefined = AccessTools.Method(typeof(Patch_Enum_IsDefined), "Prefix");
        if (original_IsDefined != null)
        {
            harmony.Patch(original_IsDefined, prefix: new HarmonyMethod(patch_IsDefined));
        }

        // 🎯 Patch `Enum.Parse()`
        MethodInfo original_Parse = AccessTools.Method(typeof(Enum), "Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) });
        MethodInfo patch_Parse = AccessTools.Method(typeof(Patch_Enum_Parse), "Prefix");
        if (original_Parse != null)
        {
            harmony.Patch(original_Parse, prefix: new HarmonyMethod(patch_Parse));
        }

        MethodInfo original_OnLevelFinishedLoading = AccessTools.Method(typeof(CGameManager), "OnLevelFinishedLoading");
        MethodInfo patch_OnLevelFinishedLoading = AccessTools.Method(typeof(GameStarting), "OnLevelFinishedLoading");
        harmony.Patch(original_OnLevelFinishedLoading, postfix: new HarmonyMethod(patch_OnLevelFinishedLoading));

        MethodInfo original_OnDayStarted = AccessTools.Method(typeof(PriceChangeManager), "OnDayStarted");
        MethodInfo patch_OnDayStarted = AccessTools.Method(typeof(CardPrice), "OnDayStarted");
        harmony.Patch(original_OnDayStarted, postfix: new HarmonyMethod(patch_OnDayStarted));

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

        MethodInfo original_CardOpeningSequenceUpdate = AccessTools.Method(typeof(CardOpeningSequence), "Update");
        MethodInfo patch_CardOpeningSequenceUpdate = AccessTools.Method(typeof(CardOpening), "Update");
        harmony.Patch(original_CardOpeningSequenceUpdate, prefix: new HarmonyMethod(patch_CardOpeningSequenceUpdate));

        MethodInfo original_Update = AccessTools.Method(typeof(CardOpeningSequence), "Update");
        MethodInfo patch_Update = AccessTools.Method(typeof(CardOpening), "UpdatePreFix");
        harmony.Patch(original_Update, prefix: new HarmonyMethod(patch_Update));

        MethodInfo original_binderSetCard = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "UpdateBinderAllCardUI");
        MethodInfo patch_binderSetCard = AccessTools.Method(typeof(SortUI), "UpdateBinderAllCardUI");
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
        MethodInfo patch_InitCardPhone = AccessTools.Method(typeof(CheckPriceUI), "CheckPricePanelInitCard");
        harmony.Patch(original_InitCardPhone, prefix: new HarmonyMethod(patch_InitCardPhone));

        MethodInfo original_EvaluateItemPanelUI = AccessTools.Method(typeof(CheckPriceScreen), "EvaluateCardPanelUI");
        MethodInfo patch_EvaluateItemPanelUI = AccessTools.Method(typeof(CheckPriceUI), "EvaluateCardPanelUI");
        harmony.Patch(original_EvaluateItemPanelUI, prefix: new HarmonyMethod(patch_EvaluateItemPanelUI));

        MethodInfo original_OnPressOpenCardPriceGraph = AccessTools.Method(typeof(CheckPriceScreen), "OnPressOpenCardPriceGraph");
        MethodInfo patch_OnPressOpenCardPriceGraph = AccessTools.Method(typeof(CheckPriceUI), "OnPressOpenCardPriceGraph");
        harmony.Patch(original_OnPressOpenCardPriceGraph, prefix: new HarmonyMethod(patch_OnPressOpenCardPriceGraph));

        MethodInfo original_ShowCardPriceChart = AccessTools.Method(typeof(ItemPriceGraphScreen), "ShowCardPriceChart");
        MethodInfo patch_ShowCardPriceChart = AccessTools.Method(typeof(CheckPriceUI), "ShowCardPriceChart");
        harmony.Patch(original_ShowCardPriceChart, prefix: new HarmonyMethod(patch_ShowCardPriceChart));

        MethodInfo original_AddCard = AccessTools.Method(typeof(CPlayerData), "AddCard");
        MethodInfo patch_AddCard = AccessTools.Method(typeof(Inventory), "AddCard");
        harmony.Patch(original_AddCard, postfix: new HarmonyMethod(patch_AddCard));

        MethodInfo original_RemoveCard = AccessTools.Method(typeof(CPlayerData), "ReduceCard");
        MethodInfo patch_RemoveCard = AccessTools.Method(typeof(Inventory), "RemoveCard");
        harmony.Patch(original_RemoveCard, postfix: new HarmonyMethod(patch_RemoveCard));

        MethodInfo original_GetIcon = AccessTools.Method(typeof(MonsterData), "GetIcon");
        MethodInfo patch_GetIcon = AccessTools.Method(typeof(ReplacingCards), "GetIcon");
        harmony.Patch(original_GetIcon, prefix: new HarmonyMethod(patch_GetIcon));

        MethodInfo original_OpenSortAlbumScreen = AccessTools.Method(typeof(CollectionBinderUI), "OpenSortAlbumScreen");
        MethodInfo patch_OpenSortAlbumScreenPrefix = AccessTools.Method(typeof(SortUI), "OpenSortAlbumScreenPrefix");
        MethodInfo patch_OpenSortAlbumScreen = AccessTools.Method(typeof(SortUI), "OpenSortAlbumScreen");
        harmony.Patch(original_OpenSortAlbumScreen, postfix: new HarmonyMethod(patch_OpenSortAlbumScreen), prefix: new HarmonyMethod(patch_OpenSortAlbumScreenPrefix));

        MethodInfo original_UpdateBinder = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "Update");
        MethodInfo patch_UpdateBinder = AccessTools.Method(typeof(SortUI), "Update");
        harmony.Patch(original_UpdateBinder, prefix: new HarmonyMethod(patch_UpdateBinder));

        MethodInfo original_OnSortingMethodUpdated = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "OnSortingMethodUpdated");
        MethodInfo patch_OnSortingMethodUpdated = AccessTools.Method(typeof(SortUI), "OnSortingMethodUpdated");
        harmony.Patch(original_OnSortingMethodUpdated, prefix: new HarmonyMethod(patch_OnSortingMethodUpdated));

        MethodInfo original_ExpansionOpenScreen = AccessTools.Method(typeof(CardExpansionSelectScreen), "OpenScreen");
        MethodInfo patch_ExpansionOpenScreen = AccessTools.Method(typeof(ExpansionScreen), "OpenExpansionScreen");
        harmony.Patch(original_ExpansionOpenScreen, postfix: new HarmonyMethod(patch_ExpansionOpenScreen));

        MethodInfo original_ExpansionPressButton = AccessTools.Method(typeof(CardExpansionSelectScreen), "OnPressButton");
        MethodInfo patch_ExpansionPressButton = AccessTools.Method(typeof(ExpansionScreen), "OnExpansionPressButton");
        harmony.Patch(original_ExpansionPressButton, prefix: new HarmonyMethod(patch_ExpansionPressButton));

        MethodInfo original_CardExpansionUpdated = AccessTools.Method(typeof(WorkbenchUIScreen), "OnCardExpansionUpdated");
        MethodInfo patch_CardExpansionUpdated = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnCardExpansionUpdated");
        harmony.Patch(original_CardExpansionUpdated, postfix: new HarmonyMethod(patch_CardExpansionUpdated));

        MethodInfo original_RarityOpenScreen = AccessTools.Method(typeof(CardRaritySelectScreen), "OpenScreen");
        MethodInfo patch_RarityOpenScreen = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OpenRarityScreen");
        harmony.Patch(original_RarityOpenScreen, postfix: new HarmonyMethod(patch_RarityOpenScreen));

        MethodInfo original_RarityPressButton = AccessTools.Method(typeof(CardRaritySelectScreen), "OnPressButton");
        MethodInfo patch_RarityPressButton = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnRarityPressButton");
        harmony.Patch(original_RarityPressButton, prefix: new HarmonyMethod(patch_RarityPressButton));

        MethodInfo original_CardRarityUpdated = AccessTools.Method(typeof(WorkbenchUIScreen), "OnRarityLimitUpdated");
        MethodInfo patch_CardRarityUpdated = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnRarityLimitUpdated");
        harmony.Patch(original_CardRarityUpdated, postfix: new HarmonyMethod(patch_CardRarityUpdated));

        MethodInfo original_WorkbenchOpenScreen = AccessTools.Method(typeof(WorkbenchUIScreen), "OpenScreen");
        MethodInfo patch_WorkbenchOpenScreen = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OpenWorkBenchScreen");
        harmony.Patch(original_WorkbenchOpenScreen, postfix: new HarmonyMethod(patch_WorkbenchOpenScreen));

        MethodInfo original_RunBundleCardBulkFunction = AccessTools.Method(typeof(WorkbenchUIScreen), "RunBundleCardBulkFunction");
        MethodInfo patch_RunBundleCardBulkFunction = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "RunBundleCardBulkFunction");
        harmony.Patch(original_RunBundleCardBulkFunction, prefix: new HarmonyMethod(patch_RunBundleCardBulkFunction));

        MethodInfo original_SetSingleCard = AccessTools.Method(typeof(BinderPageGrp), "SetSingleCard");
        MethodInfo patch_SetSingleCard = AccessTools.Method(typeof(ReplacingCards), "SetSingleCard");
        harmony.Patch(original_SetSingleCard, prefix: new HarmonyMethod(patch_SetSingleCard));

        MethodInfo original_EvaluateOpenCardPack = AccessTools.Method(typeof(InteractionPlayerController), "EvaluateOpenCardPack");
        MethodInfo patch_EvaluateOpenCardPackPrefix = AccessTools.Method(typeof(CardOpening), "EvaluateOpenCardPackPreFix");
        MethodInfo patch_EvaluateOpenCardPackPostfix = AccessTools.Method(typeof(CardOpening), "EvaluateOpenCardPackPostFix");
        harmony.Patch(original_EvaluateOpenCardPack, prefix: new HarmonyMethod(patch_EvaluateOpenCardPackPrefix), postfix: new HarmonyMethod(patch_EvaluateOpenCardPackPostfix));

        MethodInfo original_CollectionBinderFlipAnimCtrlOnRightMouseButtonUp = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "OnRightMouseButtonUp");
        MethodInfo patch_CollectionBinderFlipAnimCtrlOnRightMouseButtonUp = AccessTools.Method(typeof(ReplacingCards), "CollectionBinderFlipAnimCtrlOnRightMouseButtonUp");
        harmony.Patch(original_CollectionBinderFlipAnimCtrlOnRightMouseButtonUp, prefix: new HarmonyMethod(patch_CollectionBinderFlipAnimCtrlOnRightMouseButtonUp));

        MethodInfo original_OnPayingDone = AccessTools.Method(typeof(Customer), "OnPayingDone");
        MethodInfo patch_OnPayingDone = AccessTools.Method(typeof(CardPrice), "OnPayingDone");
        harmony.Patch(original_OnPayingDone, prefix: new HarmonyMethod(patch_OnPayingDone));

        MethodInfo original_ItemTypeToCollectionPackType = AccessTools.Method(typeof(InventoryBase), "ItemTypeToCollectionPackType");
        MethodInfo patch_ItemTypeToCollectionPackType = AccessTools.Method(typeof(CustomItemsImporter), "ItemTypeToCollectionPackType");
        harmony.Patch(original_ItemTypeToCollectionPackType, prefix: new HarmonyMethod(patch_ItemTypeToCollectionPackType));

        MethodInfo original_GetCardExpansionType = AccessTools.Method(typeof(InventoryBase), "GetCardExpansionType");
        MethodInfo patch_GetCardExpansionType = AccessTools.Method(typeof(CustomItemsImporter), "GetCardExpansionType");
        harmony.Patch(original_GetCardExpansionType, prefix: new HarmonyMethod(patch_GetCardExpansionType));

        MethodInfo original_CardBoxToCardPack = AccessTools.Method(typeof(InteractionPlayerController), "CardBoxToCardPack");
        MethodInfo patch_CardBoxToCardPack = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "CardBoxToCardPack");
        harmony.Patch(original_CardBoxToCardPack, prefix: new HarmonyMethod(patch_CardBoxToCardPack));

        //MethodInfo original_EvaluateTakeItemFromShelf = AccessTools.Method(typeof(InteractionPlayerController), "EvaluateTakeItemFromShelf");
        //MethodInfo patch_EvaluateTakeItemFromShelf = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "EvaluateTakeItemFromShelfTranspiler");
        //harmony.Patch(original_EvaluateTakeItemFromShelf, transpiler: new HarmonyMethod(patch_EvaluateTakeItemFromShelf));

        //MethodInfo original_HasEnoughSlotToHoldCard = AccessTools.Method(typeof(InteractionPlayerController), "HasEnoughSlotToHoldCard");
        //MethodInfo patch_HasEnoughSlotToHoldCard = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "HasEnoughSlotToHoldCardTranspiler");
        //harmony.Patch(original_HasEnoughSlotToHoldCard, transpiler: new HarmonyMethod(patch_HasEnoughSlotToHoldCard));

        MethodInfo original_Awake = AccessTools.Method(typeof(InteractionPlayerController), "Awake");
        MethodInfo patch_AwakePostfix = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "AwakePostfix");
        harmony.Patch(original_Awake, postfix: new HarmonyMethod(patch_AwakePostfix));

        MethodInfo original_EvaluateOpenCardPackV2 = AccessTools.Method(typeof(InteractionPlayerController), "EvaluateOpenCardPack");
        MethodInfo patch_EvaluateOpenCardPack = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "EvaluateOpenCardPack");
        harmony.Patch(original_EvaluateOpenCardPackV2, prefix: new HarmonyMethod(patch_EvaluateOpenCardPack));

        //MethodInfo original_AddHoldCard = AccessTools.Method(typeof(InteractionPlayerController), "AddHoldCard");
        //MethodInfo patch_AddHoldCard = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "AddHoldCard");
        //harmony.Patch(original_AddHoldCard, postfix: new HarmonyMethod(patch_AddHoldCard));

        MethodInfo original_RemoveToolTip = AccessTools.Method(typeof(InteractionPlayerController), "RemoveToolTip");
        MethodInfo patch_RemoveToolTip = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "RemoveToolTip");
        harmony.Patch(original_RemoveToolTip, postfix: new HarmonyMethod(patch_RemoveToolTip));

        MethodInfo original_CanOpenPack = AccessTools.Method(typeof(InteractionPlayerController), "CanOpenPack");
        MethodInfo patch_CanOpenPack = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "CanOpenPack");
        harmony.Patch(original_CanOpenPack, prefix: new HarmonyMethod(patch_CanOpenPack));

        MethodInfo original_CanOpenCardBox = AccessTools.Method(typeof(InteractionPlayerController), "CanOpenCardBox");
        MethodInfo patch_CanOpenCardBox = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "CanOpenCardBox");
        harmony.Patch(original_CanOpenCardBox, prefix: new HarmonyMethod(patch_CanOpenCardBox));

        MethodInfo original_DelayLerpSpawnedCardPackToHand = AccessTools.Method(typeof(InteractionPlayerController), "DelayLerpSpawnedCardPackToHand");
        MethodInfo patch_DelayLerpSpawnedCardPackToHandPostfix = AccessTools.Method(typeof(WankulCrazyPlugin.patch.InteractionPlayerControllerPatch), "DelayLerpSpawnedCardPackToHandPostfix");
        harmony.Patch(original_DelayLerpSpawnedCardPackToHand, postfix: new HarmonyMethod(patch_DelayLerpSpawnedCardPackToHandPostfix));

        MethodInfo original_SetMesh = AccessTools.Method(typeof(Item), "SetMesh");
        MethodInfo patch_SetMeshPatch = AccessTools.Method(typeof(WankulCrazyPlugin.importer.PatchTexturesImporter), "ItemPostfix");
        harmony.Patch(original_SetMesh, postfix: new HarmonyMethod(patch_SetMeshPatch));

        MethodInfo original_WindowsPoster = AccessTools.Method(typeof(UnlockRoomManager), "Init");
        MethodInfo patch_WindowsPoster = AccessTools.Method(typeof(WindowsPosters), "Init");
        harmony.Patch(original_WindowsPoster, postfix: new HarmonyMethod(patch_WindowsPoster));

        MethodInfo original_SetCustomer = AccessTools.Method(typeof(CustomerTradeCardScreen), "SetCustomer");
        MethodInfo patch_SetCustomer = AccessTools.Method(typeof(CustomerTradeCardScreenPatch), "SetCustomer");
        harmony.Patch(original_SetCustomer, prefix: new HarmonyMethod(patch_SetCustomer));

        MethodInfo original_OnCardScanned = AccessTools.Method(typeof(UI_CashCounterScreen), "OnCardScanned");
        MethodInfo patch_OnCardScanned = AccessTools.Method(typeof(UI_CashCounterScreenPatch), "OnCardScanned");
        harmony.Patch(original_OnCardScanned, prefix: new HarmonyMethod(patch_OnCardScanned));

        MethodInfo original_GetCardAmount = AccessTools.Method(typeof(CPlayerData), "GetCardAmount");
        MethodInfo patch_GetCardAmount = AccessTools.Method(typeof(CPlayerDataPatch), "GetCardAmount");
        harmony.Patch(original_GetCardAmount, prefix: new HarmonyMethod(patch_GetCardAmount));
    }

    public static string GetPluginPath()
    {
        return Path.Combine(Application.dataPath, "../BepInEx/plugins", PluginInfo.PLUGIN_NAME);
    }

    public static object GetPProperty(object __instance, string fieldName) {
        Type type = __instance.GetType();
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo field = type.GetField(fieldName, flags);
        if (field == null)
        {
            Plugin.Logger.LogError($"Field {fieldName} not found");
            return null;
        }
        object value = field.GetValue(__instance);
        return value;
    }

    public static object SetPProperty(object __instance, string fieldName, object value)
    {
        Type type = __instance.GetType();
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo field = type.GetField(fieldName, flags);
        if (field == null)
        {
            Plugin.Logger.LogError($"Field {fieldName} not found");
            return value;
        }
        field.SetValue(__instance, value);
        return value;
    }

    public static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }

    public static Transform FindChildByPath(Transform parent, string path)
    {
        string[] segments = path.Split('/');
        Transform current = parent;

        foreach (string segment in segments)
        {
            current = current.Find(segment);
            if (current == null)
            {
                return null;
            }
        }

        return current;
    }

    public static Transform GetByPathIn(string source, string path)
    {
        return FindChildByPath(GameObject.Find(source).transform, path);
    }
}
