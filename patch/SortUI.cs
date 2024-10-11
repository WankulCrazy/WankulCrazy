using CMF;
using HarmonyLib;
using System;
using System.Collections;
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
        static int currentGameSortMethod = 2;
        static int currentGameExpansionIndex = 0;
        static bool CanFlip = true;

        public static void OpenSortAlbumScreenPrefix(ref int sortingMethodIndex, ref int currentExpansionIndex, CollectionBinderUI __instance)
        {
            sortingMethodIndex = currentGameSortMethod;
            currentExpansionIndex = currentGameExpansionIndex;
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

                // Utiliser un facteur pour espacer (par exemple 1.5x la hauteur du bouton)
                float verticalSpacing = __instance.m_ExpansionBtnList[2].GetComponent<RectTransform>().anchoredPosition.y - __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y;
                // Positionner le 5ème bouton en dessous du 4ème, avec un espacement cohérent
                __instance.m_ExpansionBtnList[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.x,
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing  // Espacement vertical
                );
                // Changer le texte du 5ème bouton
                __instance.m_ExpansionBtnList[4].gameObject.SetActive(true);
                __instance.m_ExpansionBtnList[4].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.HS];

                __instance.m_SortAlbumBtnList[2].GetComponentInChildren<TextMeshProUGUI>().text = "Prix";
                __instance.m_SortAlbumBtnList[1].GetComponentInChildren<TextMeshProUGUI>().text = "Rareté";
                __instance.m_SortAlbumBtnList[0].GetComponentInChildren<TextMeshProUGUI>().text = "Numéro de Carte";
                __instance.m_SortAlbumBtnList[3].GetComponentInChildren<TextMeshProUGUI>().text = "Quantité";


                // Utiliser un facteur pour espacer (par exemple 1.5x la hauteur du bouton)
                float verticalSpacingSort = __instance.m_SortAlbumBtnList[3].GetComponent<RectTransform>().anchoredPosition.y - __instance.m_SortAlbumBtnList[3].GetComponent<RectTransform>().anchoredPosition.y;
                // Positionner le 5ème bouton en dessous du 4ème, avec un espacement cohérent
                __instance.m_SortAlbumBtnList[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    __instance.m_SortAlbumBtnList[3].GetComponent<RectTransform>().anchoredPosition.x,
                    __instance.m_SortAlbumBtnList[3].GetComponent<RectTransform>().anchoredPosition.y - verticalSpacingSort  // Espacement vertical
                );
                __instance.m_SortAlbumBtnList[4].GetComponentInChildren<TextMeshProUGUI>().text = "Doublon";
                __instance.m_SortAlbumBtnList[4].gameObject.SetActive(true);

                __instance.m_SortAlbumBtnList[5].gameObject.SetActive(false);
                __instance.m_SortAlbumBtnList[6].gameObject.SetActive(false);


                __instance.m_ExpansionBtnList[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.ALL;
                    __instance.OnPressSwitchExpansion(0);
                    currentGameExpansionIndex = 0;
                });
                __instance.m_ExpansionBtnList[1].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S01;
                    __instance.OnPressSwitchExpansion(1);
                    currentGameExpansionIndex = 1;
                });
                __instance.m_ExpansionBtnList[2].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S02;
                    __instance.OnPressSwitchExpansion(2);
                    currentGameExpansionIndex = 2;
                });
                __instance.m_ExpansionBtnList[3].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.S03;
                    __instance.OnPressSwitchExpansion(3);
                    currentGameExpansionIndex = 3;
                });
                __instance.m_ExpansionBtnList[4].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSeason = SortSeasonType.HS;
                    __instance.OnPressSwitchExpansion(4);
                    currentGameExpansionIndex = 4;
                });

                __instance.m_SortAlbumBtnList[2].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Price;
                    __instance.OnPressSwitchSortingMethod(2);
                    currentGameSortMethod = 2;
                });
                __instance.m_SortAlbumBtnList[1].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Rarity;
                    __instance.OnPressSwitchSortingMethod(1);
                    currentGameSortMethod = 1;
                });
                __instance.m_SortAlbumBtnList[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Number;
                    __instance.OnPressSwitchSortingMethod(0);
                    currentGameSortMethod = 0;
                });
                __instance.m_SortAlbumBtnList[3].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Amount;
                    __instance.OnPressSwitchSortingMethod(3);
                    currentGameSortMethod = 3;
                });

                __instance.m_SortAlbumBtnList[4].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    currentSortType = SortType.Double;
                    __instance.OnPressSwitchSortingMethod(4);
                    currentGameSortMethod = 4;
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

        public static void SortByDouble()
        {
            SortedCardIndies.Clear();

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = GetWankulCardsBySeason(currentSeason);

            SortedCardIndies = wankulCards
                .Select((card) => new { card })
                .Where(x => x.card.Value.amount > 1)
                .OrderByDescending(x => x.card.Value.wankulcard.MarketPrice)
                .Select(x => x.card.Value.wankulcard.Index)
                .ToList();
        }

        public static void UpdateBinderAllCardUI(int binderIndex, int pageIndex, int ___m_MaxIndex, CollectionBinderFlipAnimCtrl __instance)
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
                case SortType.Double:
                    SortByDouble();
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
                    __instance.m_BinderPageGrpList[binderIndex].SetSingleCard(i, null, 0, ECollectionSortingType.Default);
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

                __instance.m_BinderPageGrpList[binderIndex].SetSingleCard(i, cardData, wankulCardTuple.amount, ECollectionSortingType.Default);
            }
        }

        private static IEnumerator DelaySetBinderPageVisibility(bool isVisible, CollectionBinderFlipAnimCtrl __instance)
        {
            yield return new WaitForSeconds(0.5f);
            __instance.m_BinderPageGrpList[1].SetVisibility(isVisible);
            __instance.m_BinderPageGrpList[2].SetVisibility(isVisible);
        }

        private static IEnumerator DelayResetCanFlipBook(float delayTime, CollectionBinderFlipAnimCtrl __instance)
        {
            MethodInfo HideCurrentInteractableCard3dList = __instance.GetType().GetMethod("HideCurrentInteractableCard3dList", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo UpdateCurrentInteractableCard3dList = __instance.GetType().GetMethod("UpdateCurrentInteractableCard3dList", BindingFlags.Instance | BindingFlags.NonPublic);
            bool m_CanFlip = (bool)AccessTools.Field(__instance.GetType(), "m_CanFlip").GetValue(__instance);

            HideCurrentInteractableCard3dList.Invoke(__instance, new object[] {});
            m_CanFlip = false;
            CanFlip = false;
            AccessTools.Field(__instance.GetType(), "m_CanFlip").SetValue(__instance, m_CanFlip);
            yield return new WaitForSeconds(delayTime);
            m_CanFlip = true;
            CanFlip = true;
            AccessTools.Field(__instance.GetType(), "m_CanFlip").SetValue(__instance, m_CanFlip);
            yield return new WaitForSeconds(0.1f);
            UpdateCurrentInteractableCard3dList.Invoke(__instance, new object[] { });
        }

        private static IEnumerator DelaySetBinderPageCardIndex(int binderIndex, int pageIndex, CollectionBinderFlipAnimCtrl __instance)
        {
            int m_MaxIndex = (int)AccessTools.Field(__instance.GetType(), "m_MaxIndex").GetValue(__instance);
            yield return new WaitForSeconds(0.5f);
            UpdateBinderAllCardUI(binderIndex, pageIndex, m_MaxIndex, __instance);
        }

        public static bool OnSortingMethodUpdated(bool backToFirstPage, CollectionBinderFlipAnimCtrl __instance)
        {
            int m_Index = (int)AccessTools.Field(__instance.GetType(), "m_Index").GetValue(__instance);
            bool m_CanUpdateSort = (bool)AccessTools.Field(__instance.GetType(), "m_CanUpdateSort").GetValue(__instance);
            int m_MaxIndex = (int)AccessTools.Field(__instance.GetType(), "m_MaxIndex").GetValue(__instance);
            if (backToFirstPage)
            {
                m_Index = 1;
                AccessTools.Field(__instance.GetType(), "m_Index").SetValue(__instance, m_Index);
            }
            if (m_CanUpdateSort)
            {
                m_CanUpdateSort = false;
                AccessTools.Field(__instance.GetType(), "m_CanUpdateSort").SetValue(__instance, m_CanUpdateSort);
            }

            UpdateBinderAllCardUI(0, m_Index, m_MaxIndex, __instance);
            UpdateBinderAllCardUI(1, m_Index + 1, m_MaxIndex, __instance);
            UpdateBinderAllCardUI(2, m_Index - 1, m_MaxIndex, __instance);

            return false;
        }

        public static bool Update(
            CollectionBinderFlipAnimCtrl __instance
        )
        {
            bool ___m_IsBookOpen = (bool)AccessTools.Field(__instance.GetType(), "m_IsBookOpen").GetValue(__instance);
            bool ___m_IsHoldingCardCloseUp = (bool)AccessTools.Field(__instance.GetType(), "m_IsHoldingCardCloseUp").GetValue(__instance);
            bool ___m_IsExitingCardCloseUp = (bool)AccessTools.Field(__instance.GetType(), "m_IsExitingCardCloseUp").GetValue(__instance);
            bool ___m_OpenBinder = (bool)AccessTools.Field(__instance.GetType(), "m_OpenBinder").GetValue(__instance);
            Coroutine ___m_CanFlipCoroutine = (Coroutine)AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").GetValue(__instance);
            ECardExpansionType ___m_ExpansionType = (ECardExpansionType)AccessTools.Field(__instance.GetType(), "m_ExpansionType").GetValue(__instance);
            int ___m_MaxIndex = (int)AccessTools.Field(__instance.GetType(), "m_MaxIndex").GetValue(__instance);
            ECollectionSortingType ___m_SortingType = (ECollectionSortingType)AccessTools.Field(__instance.GetType(), "m_SortingType").GetValue(__instance);
            bool ___m_CloseBinder = (bool)AccessTools.Field(__instance.GetType(), "m_CloseBinder").GetValue(__instance);
            bool ___m_CanFlip = (bool)AccessTools.Field(__instance.GetType(), "m_CanFlip").GetValue(__instance);
            bool ___m_GoNext = (bool)AccessTools.Field(__instance.GetType(), "m_GoNext").GetValue(__instance);
            bool ___m_GoPrevious = (bool)AccessTools.Field(__instance.GetType(), "m_GoPrevious").GetValue(__instance);
            bool ___m_GoNext10 = (bool)AccessTools.Field(__instance.GetType(), "m_GoNext10").GetValue(__instance);
            bool ___m_GoPrevious10 = (bool)AccessTools.Field(__instance.GetType(), "m_GoPrevious10").GetValue(__instance);
            int ___m_Index = (int)AccessTools.Field(__instance.GetType(), "m_Index").GetValue(__instance);


            if (___m_IsBookOpen && ___m_IsHoldingCardCloseUp && !___m_IsExitingCardCloseUp)
            {
                float x = Mathf.Clamp(CSingleton<InteractionPlayerController>.Instance.m_CameraController.GetViewCardDeltaAngleX() * 1.5f, -15f, 15f);
                float num = Mathf.Clamp(CSingleton<InteractionPlayerController>.Instance.m_CameraController.GetViewCardDeltaAngleY(), -35f, 35f);
                Quaternion localRotation = __instance.m_CurrentSpawnedInteractableCard3d.m_Card3dUI.m_ScaleGrp.transform.localRotation;
                Vector3 eulerAngles = localRotation.eulerAngles;
                eulerAngles.x = x;
                eulerAngles.y = 0f - num;
                eulerAngles.z = 0f;
                localRotation.eulerAngles = eulerAngles;
                __instance.m_CurrentSpawnedInteractableCard3d.SetTargetRotation(localRotation);
            }

            if (___m_OpenBinder && !___m_IsBookOpen)
            {
                __instance.m_ShowHideAnim.gameObject.SetActive(value: true);
                ___m_OpenBinder = false;
                AccessTools.Field(__instance.GetType(), "m_OpenBinder").SetValue(__instance, ___m_OpenBinder);
                ___m_IsBookOpen = true;
                AccessTools.Field(__instance.GetType(), "m_IsBookOpen").SetValue(__instance, ___m_IsBookOpen);
                __instance.m_BookAnim.SetTrigger("OpenBinder");
                __instance.m_BinderThicknessAnim.SetTrigger("OpenBinder");
                __instance.m_BinderPageGrpList[0].m_Anim.SetTrigger("OpenBinder");
                __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("SetHideNextIdle");
                __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("SetHidePreviousIdle");
                __instance.StartCoroutine(DelaySetBinderPageVisibility(isVisible: true, __instance));
                if (___m_CanFlipCoroutine != null)
                {
                    __instance.StopCoroutine(___m_CanFlipCoroutine);
                }

                ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(CSingleton<InteractionPlayerController>.Instance.m_HideCardAlbumTime + 0.3f, __instance));
                AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);

                List<WankulCardData> wankulCardDatas = WankulCardsData.Instance.cards;
                float totalPrice = WankulInventory.GetTotalPrice();
                if (currentSeason != SortSeasonType.ALL)
                {
                    wankulCardDatas = WankulCardsData.GetCardsFromSeason((Season)currentSeason);
                    totalPrice = WankulInventory.GetTotalPriceBySeason((Season)currentSeason);
                }

                int num2 = wankulCardDatas.Count;

                ___m_MaxIndex = Mathf.CeilToInt((float)num2 / 12f);
                AccessTools.Field(__instance.GetType(), "m_MaxIndex").SetValue(__instance, ___m_MaxIndex);
                __instance.m_CollectionBinderUI.SetMaxPage(___m_MaxIndex);
                __instance.m_CollectionBinderUI.SetCurrentPage(___m_Index);
                __instance.m_CollectionBinderUI.SetMaxCardCollectCount(num2);
                __instance.m_CollectionBinderUI.SetCardCollected(SortedCardIndies.Count, ___m_ExpansionType);
                __instance.m_CollectionBinderUI.SetTotalValue(totalPrice);

                __instance.m_CollectionBinderUI.OpenScreen();
                ___m_SortingType = (ECollectionSortingType)CPlayerData.m_CollectionSortingMethodIndexList[(int)___m_ExpansionType];
                AccessTools.Field(__instance.GetType(), "m_SortingType").SetValue(__instance, ___m_SortingType);
                if (___m_SortingType < ECollectionSortingType.Default || ___m_SortingType >= ECollectionSortingType.MAX)
                {
                    ___m_SortingType = ECollectionSortingType.Price;
                    AccessTools.Field(__instance.GetType(), "m_SortingType").SetValue(__instance, ___m_SortingType);
                    CPlayerData.m_CollectionSortingMethodIndexList[(int)___m_ExpansionType] = (int)___m_SortingType;
                }

                OnSortingMethodUpdated(false, __instance);
                UnityAnalytic.OpenAlbum();
                if (!CPlayerData.m_HasGetGhostCard && (CPlayerData.GetCardCollectedAmount(ECardExpansionType.Ghost, isDimensionCard: false) > 0 || CPlayerData.GetCardCollectedAmount(ECardExpansionType.Ghost, isDimensionCard: true) > 0))
                {
                    __instance.m_CollectionBinderUI.OpenGhostCardTutorialScreen();
                }
            }

            if (___m_IsBookOpen)
            {
                if (___m_CloseBinder)
                {
                    ___m_IsBookOpen = false;
                    AccessTools.Field(__instance.GetType(), "m_IsBookOpen").SetValue(__instance, ___m_IsBookOpen);
                    __instance.m_BookAnim.Play("CollectionBookClose");
                    __instance.m_BinderThicknessAnim.Play("CollectionBookClose");
                    __instance.m_BinderPageGrpList[0].m_Anim.Play("BinderClose");
                    __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("SetHideNextIdle");
                    __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("SetHidePreviousIdle");
                    __instance.m_BinderPageGrpList[1].SetVisibility(isVisible: false);
                    __instance.m_BinderPageGrpList[2].SetVisibility(isVisible: false);
                    ___m_CloseBinder = false;
                    AccessTools.Field(__instance.GetType(), "m_CloseBinder").SetValue(__instance, ___m_CloseBinder);
                    if (___m_CanFlipCoroutine != null)
                    {
                        __instance.StopCoroutine(___m_CanFlipCoroutine);
                    }

                    ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(0.5f, __instance));
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    SoundManager.PlayAudio("SFX_AlbumFlip", 0.6f);
                }

                ___m_CanFlip = (bool)AccessTools.Field(__instance.GetType(), "m_CanFlip").GetValue(__instance);
                if (!___m_CanFlip || !CanFlip)
                {
                    ___m_GoNext = false;
                    AccessTools.Field(__instance.GetType(), "m_GoNext").SetValue(__instance, ___m_GoNext);
                    ___m_GoPrevious = false;
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious").SetValue(__instance, ___m_GoPrevious);
                    ___m_GoNext10 = false;
                    AccessTools.Field(__instance.GetType(), "m_GoNext10").SetValue(__instance, ___m_GoNext10);
                    ___m_GoPrevious10 = false;
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious10").SetValue(__instance, ___m_GoPrevious10);

                    AccessTools.Field(__instance.GetType(), "m_IsBookOpen").SetValue(__instance, ___m_IsBookOpen);
                    AccessTools.Field(__instance.GetType(), "m_IsHoldingCardCloseUp").SetValue(__instance, ___m_IsHoldingCardCloseUp);
                    AccessTools.Field(__instance.GetType(), "m_IsExitingCardCloseUp").SetValue(__instance, ___m_IsExitingCardCloseUp);
                    AccessTools.Field(__instance.GetType(), "m_OpenBinder").SetValue(__instance, ___m_OpenBinder);
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    AccessTools.Field(__instance.GetType(), "m_ExpansionType").SetValue(__instance, ___m_ExpansionType);
                    AccessTools.Field(__instance.GetType(), "m_MaxIndex").SetValue(__instance, ___m_MaxIndex);
                    AccessTools.Field(__instance.GetType(), "m_SortingType").SetValue(__instance, ___m_SortingType);
                    AccessTools.Field(__instance.GetType(), "m_CloseBinder").SetValue(__instance, ___m_CloseBinder);
                    AccessTools.Field(__instance.GetType(), "m_CanFlip").SetValue(__instance, ___m_CanFlip);
                    AccessTools.Field(__instance.GetType(), "m_GoNext").SetValue(__instance, ___m_GoNext);
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious").SetValue(__instance, ___m_GoPrevious);
                    AccessTools.Field(__instance.GetType(), "m_GoNext10").SetValue(__instance, ___m_GoNext10);
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious10").SetValue(__instance, ___m_GoPrevious10);
                    AccessTools.Field(__instance.GetType(), "m_Index").SetValue(__instance, ___m_Index);
                    return false;
                }

                if (___m_GoNext && ___m_Index < ___m_MaxIndex)
                {
                    __instance.m_BinderPageGrpList[0].m_Anim.SetTrigger("GoNextPage");
                    __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("GoNextPage");
                    __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("SetHideNextIdle");
                    BinderPageGrp item = __instance.m_BinderPageGrpList[0];
                    __instance.m_BinderPageGrpList.RemoveAt(0);
                    __instance.m_BinderPageGrpList.Add(item);
                    ___m_GoNext = false;
                    AccessTools.Field(__instance.GetType(), "m_GoNext").SetValue(__instance, ___m_GoNext);
                    ___m_Index++;
                    if (___m_CanFlipCoroutine != null)
                    {
                        __instance.StopCoroutine(___m_CanFlipCoroutine);
                    }

                    ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(0.55f, __instance));
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    __instance.m_CollectionBinderUI.SetCurrentPage(___m_Index);
                    SoundManager.PlayAudio("SFX_AlbumFlip", 0.6f);
                    if (___m_Index < ___m_MaxIndex)
                    {
                        UpdateBinderAllCardUI(1, ___m_Index + 1, ___m_MaxIndex, __instance);
                    }
                }

                if (___m_GoPrevious && ___m_Index > 1)
                {
                    __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("GoPreviousPage");
                    __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("SetHidePreviousIdle");
                    __instance.m_BinderPageGrpList[0].m_Anim.SetTrigger("GoPreviousPage");
                    BinderPageGrp item2 = __instance.m_BinderPageGrpList[2];
                    __instance.m_BinderPageGrpList.RemoveAt(2);
                    __instance.m_BinderPageGrpList.Insert(0, item2);
                    ___m_GoPrevious = false;
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious").SetValue(__instance, ___m_GoPrevious);
                    ___m_Index--;
                    if (___m_CanFlipCoroutine != null)
                    {
                        __instance.StopCoroutine(___m_CanFlipCoroutine);
                    }

                    ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(0.55f, __instance));
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    __instance.m_CollectionBinderUI.SetCurrentPage(___m_Index);
                    SoundManager.PlayAudio("SFX_AlbumFlip", 0.6f);
                    if (___m_Index > 1)
                    {
                        UpdateBinderAllCardUI(2, ___m_Index - 1, ___m_MaxIndex, __instance);
                    }
                }

                if (___m_GoNext10 && ___m_Index < ___m_MaxIndex)
                {
                    __instance.m_BinderPageGrpList[0].m_Anim.SetTrigger("GoNextPage");
                    __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("GoNextPage");
                    __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("SetHideNextIdle");
                    BinderPageGrp item3 = __instance.m_BinderPageGrpList[0];
                    __instance.m_BinderPageGrpList.RemoveAt(0);
                    __instance.m_BinderPageGrpList.Add(item3);
                    ___m_GoNext10 = false;
                    AccessTools.Field(__instance.GetType(), "m_GoNext10").SetValue(__instance, ___m_GoNext10);
                    ___m_Index += 10;
                    if (___m_Index > ___m_MaxIndex)
                    {
                        ___m_Index = ___m_MaxIndex;
                    }

                    if (___m_CanFlipCoroutine != null)
                    {
                        __instance.StopCoroutine(___m_CanFlipCoroutine);
                    }

                    ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(0.55f, __instance));
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    UpdateBinderAllCardUI(0, ___m_Index, ___m_MaxIndex, __instance);
                    __instance.StartCoroutine(DelaySetBinderPageCardIndex(2, ___m_Index - 1, __instance));
                    __instance.m_CollectionBinderUI.SetCurrentPage(___m_Index);
                    SoundManager.PlayAudio("SFX_AlbumFlip", 0.6f);
                    if (___m_Index < ___m_MaxIndex)
                    {
                        __instance.StartCoroutine(DelaySetBinderPageCardIndex(1, ___m_Index + 1, __instance));
                    }
                }

                if (___m_GoPrevious10 && ___m_Index > 1)
                {
                    __instance.m_BinderPageGrpList[2].m_Anim.SetTrigger("GoPreviousPage");
                    __instance.m_BinderPageGrpList[1].m_Anim.SetTrigger("SetHidePreviousIdle");
                    __instance.m_BinderPageGrpList[0].m_Anim.SetTrigger("GoPreviousPage");
                    BinderPageGrp item4 = __instance.m_BinderPageGrpList[2];
                    __instance.m_BinderPageGrpList.RemoveAt(2);
                    __instance.m_BinderPageGrpList.Insert(0, item4);
                    ___m_GoPrevious10 = false;
                    AccessTools.Field(__instance.GetType(), "m_GoPrevious10").SetValue(__instance, ___m_GoPrevious10);
                    ___m_Index -= 10;
                    if (___m_Index < 1)
                    {
                        ___m_Index = 1;
                    }

                    if (___m_CanFlipCoroutine != null)
                    {
                        __instance.StopCoroutine(___m_CanFlipCoroutine);
                    }

                    ___m_CanFlipCoroutine = __instance.StartCoroutine(DelayResetCanFlipBook(0.55f, __instance));
                    AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
                    UpdateBinderAllCardUI(0, ___m_Index, ___m_MaxIndex, __instance);
                    __instance.StartCoroutine(DelaySetBinderPageCardIndex( 1, ___m_Index + 1, __instance));
                    __instance.m_CollectionBinderUI.SetCurrentPage(___m_Index);
                    if (___m_Index > 1)
                    {
                        __instance.StartCoroutine(DelaySetBinderPageCardIndex( 2, ___m_Index - 1, __instance));
                    }

                    SoundManager.PlayAudio("SFX_AlbumFlip", 0.6f);
                }
            }

            ___m_OpenBinder = false;
            ___m_CloseBinder = false;
            ___m_GoNext = false;
            ___m_GoPrevious = false;
            ___m_GoNext10 = false;
            ___m_GoPrevious10 = false;

            AccessTools.Field(__instance.GetType(), "m_IsBookOpen").SetValue(__instance, ___m_IsBookOpen);
            AccessTools.Field(__instance.GetType(), "m_IsHoldingCardCloseUp").SetValue(__instance, ___m_IsHoldingCardCloseUp);
            AccessTools.Field(__instance.GetType(), "m_IsExitingCardCloseUp").SetValue(__instance, ___m_IsExitingCardCloseUp);
            AccessTools.Field(__instance.GetType(), "m_OpenBinder").SetValue(__instance, ___m_OpenBinder);
            AccessTools.Field(__instance.GetType(), "m_CanFlipCoroutine").SetValue(__instance, ___m_CanFlipCoroutine);
            AccessTools.Field(__instance.GetType(), "m_ExpansionType").SetValue(__instance, ___m_ExpansionType);
            AccessTools.Field(__instance.GetType(), "m_MaxIndex").SetValue(__instance, ___m_MaxIndex);
            AccessTools.Field(__instance.GetType(), "m_SortingType").SetValue(__instance, ___m_SortingType);
            AccessTools.Field(__instance.GetType(), "m_CloseBinder").SetValue(__instance, ___m_CloseBinder);
            AccessTools.Field(__instance.GetType(), "m_CanFlip").SetValue(__instance, ___m_CanFlip);
            AccessTools.Field(__instance.GetType(), "m_GoNext").SetValue(__instance, ___m_GoNext);
            AccessTools.Field(__instance.GetType(), "m_GoPrevious").SetValue(__instance, ___m_GoPrevious);
            AccessTools.Field(__instance.GetType(), "m_GoNext10").SetValue(__instance, ___m_GoNext10);
            AccessTools.Field(__instance.GetType(), "m_GoPrevious10").SetValue(__instance, ___m_GoPrevious10);
            AccessTools.Field(__instance.GetType(), "m_Index").SetValue(__instance, ___m_Index);

            return false;
        }
    }
}
