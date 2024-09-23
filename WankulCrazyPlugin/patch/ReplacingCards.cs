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

namespace WankulCrazyPlugin.patch;
public class ReplacingCards
{
    static List<int> SortedCardIndies = [];

    static void SetCardUIPrefix(CardData cardData)
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
            return;
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


    public static void SortByPriceAmount()
    {
        SortedCardIndies.Clear();

        List<WankulCardData> wankulCards = WankulInventory.Instance.wankulCards.Values.Select(x => x.wankulcard).ToList();

        SortedCardIndies = wankulCards
            .Select((card) => new { card })
            .OrderByDescending(x => x.card.MarketPrice)
            .Select(x => x.card.Index)
            .ToList();
    }

    public static void UpdateBinderAllCardUI(int binderIndex, int pageIndex, ref int ___m_MaxIndex, CollectionBinderFlipAnimCtrl __instance)
    {
        SortByPriceAmount();

        if (pageIndex <= 0 || pageIndex > ___m_MaxIndex)
        {
            return;
        }
        for (int i = 0; i < __instance.m_BinderPageGrpList[binderIndex].m_CardList.Count; i++)
        {

            int num = (pageIndex - 1) * 12 + i;
            if (num >= SortedCardIndies.Count)
            {
                __instance.m_BinderPageGrpList[binderIndex].SetSingleCard(i, null, 0);
                continue;
            }
            int index = SortedCardIndies[num];

            var wankulCardTuple = WankulInventory.Instance.wankulCards[index];
            WankulCardData wankulCardData = wankulCardTuple.wankulcard;

            CardData cardData = WankulCardsData.Instance.GetCardDataFromWankulCardData(wankulCardData);
            __instance.m_BinderPageGrpList[binderIndex].SetSingleCard(i, cardData, wankulCardTuple.amount);
        }
    }

    public static void InitCard(int cardIndex, ECardExpansionType expansionType, CheckPricePanelUI __instance)
    {
        EMonsterType monsterType = CPlayerData.GetMonsterTypeFromCardSaveIndex(cardIndex, expansionType);
        ECardBorderType borderType = (ECardBorderType)(cardIndex % CPlayerData.GetCardAmountPerMonsterType(expansionType, includeFoilCount: false));

        CardData cardData = new CardData();
        cardData.monsterType = monsterType;
        cardData.borderType = borderType;
        cardData.expansionType = expansionType;

        WankulCardData wankulCardData = WankulCardsData.Instance.GetFromMonster(cardData, true);

        if (wankulCardData != null)
        {
            if (wankulCardData is EffigyCardData effigyCard)
            {
                __instance.m_NameText.text = effigyCard.Title + "\n" + effigyCard.Effigy;
            }
            else if (wankulCardData is SpecialCardData)
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "SPECIAL";
            } else if (wankulCardData is TerrainCardData)
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "Terrain";
            }
            else
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "Erreur";
            }
        }
    }

}
