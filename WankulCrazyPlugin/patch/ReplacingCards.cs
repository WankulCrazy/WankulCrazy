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

namespace WankulCrazyPlugin.patch;
public class ReplacingCards
{
    static List<int> SortedCardIndies = [];

    static void SetCardUIPrefix(CardData cardData)
    {
        CardData gameCardData = cardData;
        WankulCardsData cardsData = WankulCardsData.Instance;

        WankulCardData wankulCardData = cardsData.GetFromMonster(gameCardData, false);

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
        CardData gameCardData = cardData;
        WankulCardsData cardsData = WankulCardsData.Instance;

        WankulCardData wankulCardData = cardsData.GetFromMonster(gameCardData, false);

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

        if (__instance.m_FullArtGrp != null)
        {
            __instance.m_FullArtGrp.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_MonsterImage is null");
        }

        if (__instance.m_GhostCard != null)
        {
            __instance.m_GhostCard.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_MonsterImage is null");
        }

        if (__instance.m_MonsterImage != null)
        {
            __instance.m_MonsterImage.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_MonsterImage is null");
        }

        if (__instance.m_MonsterMaskImage != null)
        {
            __instance.m_MonsterMaskImage.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_MonsterMaskImage is null");
        }

        if (__instance.m_RarityImage != null)
        {
            __instance.m_RarityImage.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_RarityImage is null");
        }

        if (__instance.m_AncientArtifactImage != null)
        {
            __instance.m_AncientArtifactImage.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_AncientArtifactImage is null");
        }

        if (__instance.m_NumberText != null)
        {
            __instance.m_NumberText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_NumberText is null");
        }

        if (__instance.m_MonsterNameText != null)
        {
            __instance.m_MonsterNameText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_MonsterNameText is null");
        }

        if (__instance.m_RarityText != null)
        {
            __instance.m_RarityText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_RarityText is null");
        }

        if (__instance.m_Stat1Text != null)
        {
            __instance.m_Stat1Text.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_Stat1Text is null");
        }

        if (__instance.m_Stat2Text != null)
        {
            __instance.m_Stat2Text.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_Stat2Text is null");
        }

        if (__instance.m_Stat3Text != null)
        {
            __instance.m_Stat3Text.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_Stat3Text is null");
        }

        if (__instance.m_Stat4Text != null)
        {
            __instance.m_Stat4Text.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_Stat4Text is null");
        }

        if (__instance.m_DescriptionText != null)
        {
            __instance.m_DescriptionText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_DescriptionText is null");
        }

        if (__instance.m_ArtistText != null)
        {
            __instance.m_ArtistText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_ArtistText is null");
        }

        if (__instance.m_FameText != null)
        {
            __instance.m_FameText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_FameText is null");
        }

        if (__instance.m_FirstEditionText != null)
        {
            __instance.m_FirstEditionText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_FirstEditionText is null");
        }

        if (__instance.m_ChampionText != null)
        {
            __instance.m_ChampionText.gameObject.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_ChampionText is null");
        }

        if (__instance.m_EvoGrp != null)
        {
            __instance.m_EvoGrp.SetActive(false);
        }
        else
        {
            Plugin.Logger.LogError("m_EvoGrp is null");
        }


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
            else
            {
                Plugin.Logger.LogInfo("WankulCard FullRarityName : Terrain");
                __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = "Terrain";
                __instance.m_CollectionBinderUI.m_CardNameText.text = wankulCardData.Title;
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

}
