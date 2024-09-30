using HarmonyLib;
using System.Collections.Generic;
using System;
using UnityEngine;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;
using System.Linq;
using System.Threading;
using UnityEngine.UIElements;
using System.Reflection;

namespace WankulCrazyPlugin.patch
{
    public class CheckPriceUI
    {
        //public static Dictionary<int, int> indexesAssociation = new();
        public static List<WankulCardData> wankulCardsSet = new List<WankulCardData>();
        public static bool EvaluateCardPanelUI(int cardPageIndex, CheckPriceScreen __instance)
        {
            var m_PosX = (float)AccessTools.Field(__instance.GetType(), "m_PosX").GetValue(__instance);
            var m_LerpPosX = (float)AccessTools.Field(__instance.GetType(), "m_LerpPosX").GetValue(__instance);
            var m_CurrentExpansionType = (ECardExpansionType)AccessTools.Field(__instance.GetType(), "m_CurrentExpansionType").GetValue(__instance);
            var m_CardPageMaxIndex = (int)AccessTools.Field(__instance.GetType(), "m_CardPageMaxIndex").GetValue(__instance);
            var m_ScrollEndPosParent = (GameObject)AccessTools.Field(__instance.GetType(), "m_ScrollEndPosParent").GetValue(__instance);
            var m_CardPageIndex = (int)AccessTools.Field(__instance.GetType(), "m_CardPageIndex").GetValue(__instance);

            Season[] seasons = (Season[])Enum.GetValues(typeof(Season));
            Season currentSeason = seasons[ExpansionScreen.currentExpensionIndex];
            string currentSeasonText = SeasonsContainer.Seasons[currentSeason];
            List<WankulCardData> wankulCards = WankulCardsData.GetCardsFromSeason(currentSeason);

            m_PosX = 0f;
            m_LerpPosX = 0f;
            for (int i = 0; i < __instance.m_CheckPricePanelUIList.Count; i++)
            {
                __instance.m_CheckPricePanelUIList[i].SetActive(isActive: false);
            }

            __instance.m_CardExpansionText.text = currentSeasonText;
            int cardIndexOffset = cardPageIndex * __instance.m_MaxCardUICountPerPage;
            int totalCardsCount = wankulCards.Count();

            m_CardPageMaxIndex = Mathf.CeilToInt((float)totalCardsCount / (float)__instance.m_MaxCardUICountPerPage) - 1;

            for (int j = 0; j < __instance.m_MaxCardUICountPerPage; j++)
            {
                if (cardIndexOffset >= totalCardsCount)
                {
                    __instance.m_CheckPricePanelUIList[j].SetActive(isActive: false);
                    continue;
                }

                wankulCardsSet.Insert(cardIndexOffset, wankulCards[cardIndexOffset]);

                __instance.m_CheckPricePanelUIList[j].InitCard(__instance, cardIndexOffset, m_CurrentExpansionType, false);
                __instance.m_CheckPricePanelUIList[j].SetActive(isActive: true);
                __instance.m_ScrollEndParent.transform.parent = __instance.m_CheckPricePanelUIList[j].transform;
                __instance.m_ScrollEndParent.transform.position = __instance.m_CheckPricePanelUIList[j].transform.position;
                Vector3 position = __instance.m_ScrollEndParent.transform.position;
                position.y += __instance.m_CardScrollOffsetPosEnd.position.y - __instance.m_CardScrollOffsetPosStart.position.y;
                __instance.m_ScrollEndParent.transform.position = position;
                m_ScrollEndPosParent = __instance.m_ScrollEndParent;
                cardIndexOffset++;
            }

            __instance.m_PageText.text = m_CardPageIndex + 1 + " / " + (m_CardPageMaxIndex + 1);
            __instance.m_CardPageOptionGrp.SetActive(value: true);

            AccessTools.Field(__instance.GetType(), "m_PosX").SetValue(__instance, m_PosX);
            AccessTools.Field(__instance.GetType(), "m_LerpPosX").SetValue(__instance, m_LerpPosX);
            AccessTools.Field(__instance.GetType(), "m_CurrentExpansionType").SetValue(__instance, m_CurrentExpansionType);
            AccessTools.Field(__instance.GetType(), "m_CardPageMaxIndex").SetValue(__instance, m_CardPageMaxIndex);
            AccessTools.Field(__instance.GetType(), "m_ScrollEndPosParent").SetValue(__instance, m_ScrollEndPosParent);
            AccessTools.Field(__instance.GetType(), "m_CardPageIndex").SetValue(__instance, m_CardPageIndex);

            return false;
        }
        public static bool CheckPricePanelInitCard(CheckPriceScreen checkPriceScreen, int cardIndex, ECardExpansionType expansionType, bool isDestiny, CheckPricePanelUI __instance)
        {
            var m_IsItem = (bool)AccessTools.Field(__instance.GetType(), "m_IsItem").GetValue(__instance);
            var m_IsCard = (bool)AccessTools.Field(__instance.GetType(), "m_IsCard").GetValue(__instance);
            var m_CheckPriceScreen = (CheckPriceScreen)AccessTools.Field(__instance.GetType(), "m_CheckPriceScreen").GetValue(__instance);
            var m_CardIndex = (int)AccessTools.Field(__instance.GetType(), "m_CardIndex").GetValue(__instance);
            var m_CardExpansionType = (ECardExpansionType)AccessTools.Field(__instance.GetType(), "m_CardExpansionType").GetValue(__instance);
            var m_IsDestiny = (bool)AccessTools.Field(__instance.GetType(), "m_IsDestiny").GetValue(__instance);
            var m_TotalPrice = (float)AccessTools.Field(__instance.GetType(), "m_TotalPrice").GetValue(__instance);

            WankulCardData wankulCardData = wankulCardsSet.ElementAt(cardIndex);

            CardData cardData = WankulCardsData.Instance.GetCardDataFromWankulCardData(wankulCardData);

            if (cardData == null)
            {
                cardData = WankulCardsData.Instance.GetUnassciatedCardData();
                WankulCardsData.Instance.SetFromMonster(cardData, wankulCardData);
            }

            cardData.isDestiny = false;
            __instance.m_UIGrp.SetActive(value: true);
            __instance.m_PrologueUIGrp.SetActive(value: false);

            m_IsItem = false;
            m_IsCard = true;
            m_CheckPriceScreen = checkPriceScreen;
            m_CardIndex = cardIndex;
            m_CardExpansionType = expansionType;
            m_IsDestiny = isDestiny;
            __instance.m_CardUI.SetCardUI(cardData);
            m_TotalPrice = wankulCardData.MarketPrice;

            if (wankulCardData is EffigyCardData effigyCard)
            {
                __instance.m_NameText.text = effigyCard.Title + "\n" + effigyCard.Effigy;
                if (effigyCard.Rarity > Rarity.R)
                {
                    cardData.isFoil = true;
                }
            }
            else if (wankulCardData is SpecialCardData)
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "SPECIAL";
            }
            else if (wankulCardData is TerrainCardData)
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "Terrain";
            }
            else
            {
                __instance.m_NameText.text = wankulCardData.Title + "\n" + "Erreur";
            }

            __instance.m_TotalPriceText.text = GameInstance.GetPriceString(m_TotalPrice);
            __instance.m_ItemImage.enabled = false;
            __instance.m_CardUI.gameObject.SetActive(value: true);

            AccessTools.Field(__instance.GetType(), "m_IsItem").SetValue(__instance, m_IsItem);
            AccessTools.Field(__instance.GetType(), "m_IsCard").SetValue(__instance, m_IsCard);
            AccessTools.Field(__instance.GetType(), "m_CheckPriceScreen").SetValue(__instance, m_CheckPriceScreen);
            AccessTools.Field(__instance.GetType(), "m_CardIndex").SetValue(__instance, m_CardIndex);
            AccessTools.Field(__instance.GetType(), "m_CardExpansionType").SetValue(__instance, m_CardExpansionType);
            AccessTools.Field(__instance.GetType(), "m_IsDestiny").SetValue(__instance, m_IsDestiny);
            AccessTools.Field(__instance.GetType(), "m_TotalPrice").SetValue(__instance, m_TotalPrice);

            List<float> pastCardPricePercentChange = CPlayerData.GetPastCardPricePercentChange(cardIndex, expansionType, isDestiny);
            if (pastCardPricePercentChange.Count > 1)
            {
                float cardMarketPriceCustomPercent = CPlayerData.GetCardMarketPriceCustomPercent(cardIndex, expansionType, isDestiny, pastCardPricePercentChange[pastCardPricePercentChange.Count - 2]);
                float num = m_TotalPrice - cardMarketPriceCustomPercent;
                if (num > 0.005f)
                {
                    __instance.m_UpArrow.SetActive(value: true);
                    __instance.m_DownArrow.SetActive(value: false);
                    __instance.m_NoChangeArrow.SetActive(value: false);
                    __instance.m_PriceChangeText.text = "+" + GameInstance.GetPriceString(num);
                    __instance.m_PriceChangeText.color = m_CheckPriceScreen.m_PositiveColor;
                }
                else if (num < -0.005f)
                {
                    __instance.m_UpArrow.SetActive(value: false);
                    __instance.m_DownArrow.SetActive(value: true);
                    __instance.m_NoChangeArrow.SetActive(value: false);
                    __instance.m_PriceChangeText.text = GameInstance.GetPriceString(num);
                    __instance.m_PriceChangeText.color = m_CheckPriceScreen.m_NegativeColor;
                }
                else
                {
                    __instance.m_UpArrow.SetActive(value: false);
                    __instance.m_DownArrow.SetActive(value: false);
                    __instance.m_NoChangeArrow.SetActive(value: true);
                    __instance.m_PriceChangeText.text = "+" + GameInstance.GetPriceString(0f);
                    __instance.m_PriceChangeText.color = m_CheckPriceScreen.m_NeutralColor;
                }
                return false;
            }
            else
            {
                __instance.m_UpArrow.SetActive(value: false);
                __instance.m_DownArrow.SetActive(value: false);
                __instance.m_NoChangeArrow.SetActive(value: true);
                __instance.m_PriceChangeText.text = "+" + GameInstance.GetPriceString(0f);


                return false;
            }
        }

        public static bool OnPressOpenCardPriceGraph(int cardIndex, ECardExpansionType expansionType, bool isDestiny, CheckPriceScreen __instance)
        {
            __instance.m_ItemPriceGraphScreen.ShowCardPriceChart(cardIndex, expansionType, isDestiny);

            MethodInfo openChildScreenMethod = __instance.GetType().GetMethod("OpenChildScreen", BindingFlags.Instance | BindingFlags.NonPublic);
            openChildScreenMethod.Invoke(__instance, new object[] { __instance.m_ItemPriceGraphScreen });

            return false;
        }

        public static bool ShowCardPriceChart(int cardIndex, ECardExpansionType expansionType, bool isDestiny, ItemPriceGraphScreen __instance)
        {
            __instance.m_CurrentScaleLineIndex = 0;
            WankulCardData wankulCardData = wankulCardsSet[cardIndex];
            List<float> list = new List<float>();

            for (int i = 0; i < wankulCardData.PastPrices.Count; i++)
            {
                list.Add(wankulCardData.PastPrices[i]);
            }


            MethodInfo EvaluatePriceChartMethod = __instance.GetType().GetMethod("EvaluatePriceChart", BindingFlags.Instance | BindingFlags.NonPublic);
            EvaluatePriceChartMethod.Invoke(__instance, new object[] { list });

            CardData cardData = WankulCardsData.Instance.GetCardDataFromWankulCardData(wankulCardData);
            if (cardData == null)
            {
                cardData = WankulCardsData.Instance.GetUnassciatedCardData();
                WankulCardsData.Instance.SetFromMonster(cardData, wankulCardData);
            }

            __instance.m_CardName.text = wankulCardData.Title;
            __instance.m_CardUI.SetCardUI(cardData);
            __instance.m_ItemGrp.SetActive(value: false);
            __instance.m_CardGrp.SetActive(value: true);

            return false;
        }
    }
}
