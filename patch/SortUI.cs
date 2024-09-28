using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch
{
    public class SortUI
    {
        public static bool inited = false;
        static List<int> SortedCardIndies = [];
        static SortSeasonType currentSeason = SortSeasonType.ALL;
        static SortType currentSortType = SortType.Price;

        public static void OpenSortAlbumScreenPrefix(ref int sortingMethodIndex, ref int currentExpansionIndex, CollectionBinderUI __instance)
        {
            if (!inited)
            {
                sortingMethodIndex = 2;
                currentExpansionIndex = 0;
            }
        }

        public static void OpenSortAlbumScreen(int sortingMethodIndex, int currentExpansionIndex, CollectionBinderUI __instance)
        {
            if (!inited)
            {
                // Position et texte des boutons initiaux
                __instance.m_ExpansionBtnList[0].GetParent().GetComponentInParent<RectTransform>().anchoredPosition = new Vector2(
                    0,
                    75
                );

                __instance.m_ExpansionBtnList[0].GetComponentInChildren<TextMeshProUGUI>().text = "Tout";
                __instance.m_ExpansionBtnList[1].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S01];
                __instance.m_ExpansionBtnList[2].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S02];
                __instance.m_ExpansionBtnList[3].gameObject.SetActive(true);
                __instance.m_ExpansionBtnList[3].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S03];

                // Calculer l'espacement vertical constant entre les boutons
                float buttonHeight = __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().sizeDelta.y;
                // Utiliser un facteur pour espacer (par exemple 1.5x la hauteur du bouton)
                float verticalSpacing = __instance.m_ExpansionBtnList[2].GetComponent<RectTransform>().anchoredPosition.y - __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y;

                // Positionner le 5ème bouton en dessous du 4ème, avec un espacement cohérent
                __instance.m_ExpansionBtnList[4].gameObject.SetActive(true);
                __instance.m_ExpansionBtnList[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.x,
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing  // Espacement vertical
                );

                // Changer le texte du 5ème bouton
                __instance.m_ExpansionBtnList[4].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.HS];

                __instance.m_SortAlbumBtnList[2].GetComponentInChildren<TextMeshProUGUI>().text = "Prix";
                __instance.m_SortAlbumBtnList[1].GetComponentInChildren<TextMeshProUGUI>().text = "Rareté";
                __instance.m_SortAlbumBtnList[0].GetComponentInChildren<TextMeshProUGUI>().text = "Numéro de Carte";
                __instance.m_SortAlbumBtnList[3].GetComponentInChildren<TextMeshProUGUI>().text = "Quantité";
                __instance.m_SortAlbumBtnList[4].gameObject.SetActive(false);
                __instance.m_SortAlbumBtnList[5].gameObject.SetActive(false);
                __instance.m_SortAlbumBtnList[6].gameObject.SetActive(false);


                __instance.m_ExpansionBtnList[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.ALL;
                    __instance.OnPressSwitchExpansion(0);
                });
                __instance.m_ExpansionBtnList[1].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S01;
                    __instance.OnPressSwitchExpansion(1);
                });
                __instance.m_ExpansionBtnList[2].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S02;
                    __instance.OnPressSwitchExpansion(2);
                });
                __instance.m_ExpansionBtnList[3].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S03;
                    __instance.OnPressSwitchExpansion(3);
                });
                __instance.m_ExpansionBtnList[4].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.HS;
                    __instance.OnPressSwitchExpansion(4);
                });

                __instance.m_SortAlbumBtnList[2].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Price;
                    __instance.OnPressSwitchSortingMethod(2);
                });
                __instance.m_SortAlbumBtnList[1].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Rarity;
                    __instance.OnPressSwitchSortingMethod(1);
                });
                __instance.m_SortAlbumBtnList[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Number;
                    __instance.OnPressSwitchSortingMethod(0);
                });
                __instance.m_SortAlbumBtnList[3].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Amount;
                    __instance.OnPressSwitchSortingMethod(3);
                });

                // Marquer l'initialisation comme terminée
                inited = true;
            }
        }

        public static Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> GetWankulCardsBySeason(SortSeasonType season)
        {
            List<WankulCardData> wankulCards = WankulInventory.Instance.wankulCards.Values.Select(x => x.wankulcard).ToList();

            if (season == SortSeasonType.ALL)
            {
                return WankulInventory.Instance.wankulCards;
            }
            else
            {
                var filteredWankulCards = WankulInventory.Instance.wankulCards
                    .Where(kvp => kvp.Value.wankulcard.Season == (Season)season)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return filteredWankulCards;
            }
        }

        public static void SortByPriceAmount()
        {
            SortedCardIndies.Clear();

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = GetWankulCardsBySeason(currentSeason);

            SortedCardIndies = wankulCards
                .Select((card) => new { card })
                .OrderByDescending(x => x.card.Value.wankulcard.MarketPrice)
                .Select(x => x.card.Value.wankulcard.Index)
                .ToList();
        }

        public static void SortByRarity()
        {
            SortedCardIndies.Clear();

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = GetWankulCardsBySeason(currentSeason);

            SortedCardIndies = wankulCards
                .Select((card) => new { card })
                .OrderByDescending(x =>
                {
                    if (x.card.Value.wankulcard is EffigyCardData)
                    {
                        return ((EffigyCardData)x.card.Value.wankulcard).Rarity;
                    }
                    else
                    {
                        return Rarity.C;
                    }
                })
                .Select(x => x.card.Value.wankulcard.Index)
                .ToList();
        }

        public static void SortBySeasonAndNumber()
        {
            SortedCardIndies.Clear();

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = GetWankulCardsBySeason(currentSeason);

            SortedCardIndies = wankulCards
                .Select((card) => new { card })
                .OrderBy(x => x.card.Value.wankulcard.Season)
                .ThenBy(x => x.card.Value.wankulcard.NumberInt)
                .Select(x => x.card.Value.wankulcard.Index)
                .ToList();
        }

        public static void SortByAmount()
        {
            SortedCardIndies.Clear();

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = GetWankulCardsBySeason(currentSeason);

            SortedCardIndies = wankulCards
                .Select((card) => new { card })
                .OrderByDescending(x => x.card.Value.amount)
                .Select(x => x.card.Value.wankulcard.Index)
                .ToList();
        }

        public static void UpdateBinderAllCardUI(int binderIndex, int pageIndex, ref int ___m_MaxIndex, CollectionBinderFlipAnimCtrl __instance)
        {
            switch (currentSortType)
            {
                case SortType.Price:
                    SortByPriceAmount();
                    break;
                case SortType.Rarity:
                    SortByRarity();
                    break;
                case SortType.Number:
                    SortBySeasonAndNumber();
                    break;
                case SortType.Amount:
                    SortByAmount();
                    break;
            }

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

                // fixing broken saves
                if (cardData == null)
                {
                    Plugin.Logger.LogWarning("gameCardData is null");

                    cardData = WankulCardsData.Instance.GetUnassciatedCardData();
                    WankulCardData debugwankulCardData = WankulCardsData.GetAJETER();

                    string debugkey = $"{cardData.monsterType}_{cardData.borderType}_{cardData.expansionType}";
                    Plugin.Logger.LogWarning($"gameCardData is null, adding to inventory {debugwankulCardData.Title} {debugwankulCardData.Index}, {debugkey}");

                    WankulCardsData.Instance.SetFromMonster(cardData, debugwankulCardData);
                    CPlayerData.AddCard(cardData, 1);
                }

                __instance.m_BinderPageGrpList[binderIndex].SetSingleCard(i, cardData, wankulCardTuple.amount);
            }
        }
    }
}
