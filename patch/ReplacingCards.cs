using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;
using Logger = HarmonyLib.Tools.Logger;
using System.Threading;
using WankulCrazyPlugin.cards;
using Newtonsoft.Json;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using WankulCrazyPlugin.inventory;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.patch;
public class ReplacingCards
{
    static void SetCardUIPrefix(CardData cardData)
    {
        WankulCardsData cardsData = WankulCardsData.Instance;

        // fixing broken saves
        if (cardData == null)
        {
            Plugin.Logger.LogWarning("gameCardData is null");

            cardData = cardsData.GetUnassciatedCardData();
            WankulCardData debugwankulCardData = WankulCardsData.GetAJETER();

            string key = $"{cardData.monsterType}_{cardData.borderType}_{cardData.expansionType}";
            Plugin.Logger.LogWarning($"gameCardData is null, adding to inventory {debugwankulCardData.Title} {debugwankulCardData.Index}, {key}");

            cardsData.SetFromMonster(cardData, debugwankulCardData);
            CPlayerData.AddCard(cardData, 1);
        }

        CardData gameCardData = cardData;

        WankulCardData wankulCardData = cardsData.GetFromMonster(gameCardData, true);
        if (wankulCardData == null)
        {
            return;
        }

        gameCardData.isFoil = false;
        gameCardData.isChampionCard = false;

        if (wankulCardData is EffigyCardData)
        {
            EffigyCardData effigyCard = (EffigyCardData)wankulCardData;

            if (effigyCard.Rarity >= Rarity.UR1)
            {
                gameCardData.isFoil = true;
            }

        }
    }

    static void SetCardUIPostFix(CardData cardData, CardUI __instance)
    {
        if (cardData == null)
        {
            Plugin.Logger.LogError("gameCardData is null");
            return;
        }
        CardData gameCardData = cardData;
        WankulCardsData cardsData = WankulCardsData.Instance;

        WankulCardData wankulCardData = cardsData.GetFromMonster(gameCardData, true);
        if (wankulCardData == null)
        {
            string key = $"{gameCardData.monsterType}_{gameCardData.borderType}_{gameCardData.expansionType}";
            Plugin.Logger.LogError($"wankulCardData is null from {key}, using AJETER");
            wankulCardData = WankulCardsData.GetAJETER();
        }

        if (__instance.m_NormalGrp != null)
        {
            __instance.m_NormalGrp.SetActive(true);
        }

        gameCardData.isFoil = false;
        gameCardData.isChampionCard = false;

        if (wankulCardData is EffigyCardData)
        {
            EffigyCardData effigyCard = (EffigyCardData)wankulCardData;

            if (effigyCard.Rarity >= Rarity.UR1)
            {
                gameCardData.isFoil = true;
            }

        }

        try
        {
            __instance.m_CardBGImage.sprite = wankulCardData.Sprite;
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError("Error setting m_CardBGImage sprite: " + e);
            Plugin.Logger.LogInfo("Setting m_CardBorderImage instead");
            if (__instance.m_CardBorderImage != null)
            {
                __instance.m_CardBorderImage.sprite = wankulCardData.Sprite;
            }
            else
            {
                Plugin.Logger.LogError("m_CardBorderImage is null");
            }
        }

        __instance.m_FullArtGrp?.gameObject.SetActive(false);
        __instance.m_GhostCard?.gameObject.SetActive(false);
        __instance.m_MonsterImage?.gameObject.SetActive(false);
        __instance.m_MonsterMaskImage?.gameObject.SetActive(false);
        __instance.m_RarityImage?.gameObject.SetActive(false);
        __instance.m_AncientArtifactImage?.gameObject.SetActive(false);
        __instance.m_NumberText?.gameObject.SetActive(false);
        __instance.m_MonsterNameText?.gameObject.SetActive(false);
        __instance.m_RarityText?.gameObject.SetActive(false);
        __instance.m_Stat1Text?.gameObject.SetActive(false);
        __instance.m_Stat2Text?.gameObject.SetActive(false);
        __instance.m_Stat3Text?.gameObject.SetActive(false);
        __instance.m_Stat4Text?.gameObject.SetActive(false);
        __instance.m_DescriptionText?.gameObject.SetActive(false);
        __instance.m_ArtistText?.gameObject.SetActive(false);
        __instance.m_FameText?.gameObject.SetActive(false);
        __instance.m_FirstEditionText?.gameObject.SetActive(false);
        __instance.m_ChampionText?.gameObject.SetActive(false);
        __instance.m_EvoGrp?.SetActive(false);
    }

    class EnterViewUpCloseState__State
    {
        public bool ready;
        public CardData cardData;
        public WankulCardData wankulCardData;
    }

    static void EnterViewUpCloseStatePrefix(out EnterViewUpCloseState__State __state,CollectionBinderFlipAnimCtrl __instance)
    {
        __state = new EnterViewUpCloseState__State();
        if (!__instance.m_IsHoldingCardCloseUp && (bool)__instance.m_CurrentRaycastedInteractableCard3d)
        {
            __state.ready = true;
            __state.cardData = __instance.m_CurrentRaycastedInteractableCard3d.m_Card3dUI.m_CardUI.GetCardData();
            __state.wankulCardData = WankulCardsData.Instance.GetFromMonster(__state.cardData, false);
        }
        else
        {
            __state.ready = false;
        }
    }

    static void EnterViewUpCloseStatePostfix(EnterViewUpCloseState__State __state, CollectionBinderFlipAnimCtrl __instance)
    {
        if (__state.ready)
        {
            WankulCardData wankulCardData = __state.wankulCardData;
            CardData inGameCard = __state.cardData;
            ECardExpansionType expansionType = inGameCard.expansionType;
            MonsterData monsterData = InventoryBase.GetMonsterData(inGameCard.monsterType);
            EElementIndex elementIndex = monsterData.ElementIndex;
            ERarity rarity = monsterData.Rarity;

            string key = inGameCard.monsterType.ToString() + "_" + inGameCard.borderType.ToString() + "_" + expansionType.ToString() + "_" + elementIndex.ToString() + "_" + rarity.ToString();

            if (__state.wankulCardData is EffigyCardData effigyCard)
            {
                __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = RaritiesContainer.Rarities[effigyCard.Rarity];
                __instance.m_CollectionBinderUI.m_CardNameText.text = effigyCard.Title + "\n" + effigyCard.Effigy;
            }
            else if (__state.wankulCardData is SpecialCardData) {
                __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = "SPECIAL";
                __instance.m_CollectionBinderUI.m_CardNameText.text = wankulCardData.Title;
            }
            else if(__state.wankulCardData is TerrainCardData)
            {
                __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = "Terrain";
                __instance.m_CollectionBinderUI.m_CardNameText.text = wankulCardData.Title;
            }
            else
            {
                __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = "Erreur";
                __instance.m_CollectionBinderUI.m_CardNameText.text = "Erreur";
            }

        }
    }

    public static bool SetSingleCard(int cardIndex, CardData cardData, int cardCount, ECollectionSortingType sortingType, BinderPageGrp __instance)
    {
        if (cardData == null)
        {
            __instance.m_CardList[cardIndex].SetVisibility(isVisible: false);
            return false;
        }
        cardCount = WankulInventory.GetWankulCardFormGameCard(cardData).amount;
        if (sortingType == ECollectionSortingType.DuplicatePrice)
        {
            cardCount--;
        }
        if (cardCount <= 0)
        {
            __instance.m_CardList[cardIndex].SetVisibility(isVisible: false);
            return false;
        }
        __instance.m_CardList[cardIndex].m_CardUI.SetCardUI(cardData);
        __instance.m_CardList[cardIndex].SetVisibility(isVisible: true);
        __instance.m_CardList[cardIndex].SetCardCountText(cardCount, sortingType == ECollectionSortingType.DuplicatePrice);
        __instance.m_CardList[cardIndex].SetCardCountTextVisibility(isVisible: true);
        return false;
    }

    public static bool GetIcon(ECardExpansionType cardExpansionType, MonsterData __instance, ref Sprite __result)
    {
        WankulCardData aJETER = WankulCardsData.GetAJETER();
;
        __result = aJETER.Sprite;
        return false;
    }

}
