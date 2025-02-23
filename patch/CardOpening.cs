using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;
using WankulCrazyPlugin.utils;
using static UnityEngine.GraphicsBuffer;

namespace WankulCrazyPlugin.patch
{
    public class CardOpening
    {
        public static int totalExpGained = 0;
        public static List<int> LegendaryBoosters = new List<int>();
        public static int boosterSize = 10;

        public static void UpdatePreFix(ref List<CardData> ___m_RolledCardDataList, CardOpeningSequence __instance)
        {
            if (__instance.m_StateIndex == 11 && totalExpGained > 0)
            {
                ___m_RolledCardDataList.Clear();
                CEventManager.QueueEvent(new CEventPlayer_AddShopExp(totalExpGained));
                totalExpGained = 0;
            }
        }

        public static void Start(CardOpeningSequence __instance)
        {
            if (__instance.m_Card3dUIList.Count >= boosterSize)
            {
                return;
            }

            // Utilisation de l'instance singleton de Card3dUISpawner
            Card3dUISpawner card3dUISpawnerInstance = Card3dUISpawner.m_Instance;

            if (card3dUISpawnerInstance == null)
            {
                Plugin.Logger.LogError("Card3dUISpawner instance is null.");
                return;
            }

            MethodInfo addCardPrefabMethod = typeof(Card3dUISpawner).GetMethod("AddCardPrefab", BindingFlags.Instance | BindingFlags.NonPublic);

            if (addCardPrefabMethod == null)
            {
                Plugin.Logger.LogError("Failed to get AddCardPrefab method.");
                return;
            }

            int cardsToAdd = boosterSize - __instance.m_Card3dUIList.Count;
            for (int i = 0; i < cardsToAdd; i++)
            {


                Transform CardOpeningSequence_WorldUIGrp_Transform = Plugin.GetByPath("CanvasGrp/CardOpeningSequence_WorldUIGrp/CardOpeningGrp");

                Card3dUIGroup existingCard3dUIGroup = __instance.m_Card3dUIList[__instance.m_Card3dUIList.Count - 1];
                Card3dUIGroup newCard3dUIGroup = Card3dUISpawner.m_Instance.GetCardUI();
                newCard3dUIGroup.gameObject.SetActive(true);
                newCard3dUIGroup.transform.SetParent(CardOpeningSequence_WorldUIGrp_Transform);
                newCard3dUIGroup.transform.rotation = existingCard3dUIGroup.transform.rotation;
                newCard3dUIGroup.transform.localScale = existingCard3dUIGroup.transform.localScale;
                newCard3dUIGroup.transform.localPosition = existingCard3dUIGroup.transform.localPosition;

                RectTransform rectTransform = (RectTransform)newCard3dUIGroup.transform;
                Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
                anchoredPosition3D.z += 0.001f * (i + 1);

                rectTransform.anchoredPosition3D = anchoredPosition3D;

                __instance.m_Card3dUIList.Add(newCard3dUIGroup);


                Transform AnimGrp_Transform = Plugin.FindChildByPath(newCard3dUIGroup.transform, "AnimGrp");
                Transform existingAnimGrp_Transform = Plugin.FindChildByPath(existingCard3dUIGroup.transform, "AnimGrp");
                Animation existingAnimation = existingAnimGrp_Transform.GetComponent<Animation>();

                AnimationCopier.CopyAnimation(existingAnimGrp_Transform.gameObject, AnimGrp_Transform.gameObject, "OpenCardNewCard");
                AnimationCopier.CopyAnimation(existingAnimGrp_Transform.gameObject, AnimGrp_Transform.gameObject, "OpenCardSlideExit");
                AnimationCopier.CopyAnimation(existingAnimGrp_Transform.gameObject, AnimGrp_Transform.gameObject, "OpenCardFinalReveal");
                AnimationCopier.CopyAnimation(existingAnimGrp_Transform.gameObject, AnimGrp_Transform.gameObject, "OpenCardDefaultPos");

                Animation AnimGrp_Animation = AnimGrp_Transform.GetComponent<Animation>();

                __instance.m_CardAnimList.Add(AnimGrp_Animation);


                Transform ShowAllCardPosList_Transform = Plugin.GetByPath("CanvasGrp/CardOpeningSequence_WorldUIGrp/ShowAllCardPosList");

                RectTransform existingPos = (RectTransform)__instance.m_ShowAllCardPosList[__instance.m_ShowAllCardPosList.Count - 1];
                GameObject newGameObject = new GameObject($"ShowAllCardPos ({__instance.m_ShowAllCardPosList.Count + 1})");
                newGameObject.AddComponent<RectTransform>();
                RectTransform newPos = newGameObject.GetComponent<RectTransform>();
                newPos.gameObject.SetActive(true);
                newPos.SetParent(ShowAllCardPosList_Transform);
                newPos.position = existingPos.position;
                newPos.rotation = existingPos.rotation;
                newPos.localScale = existingPos.localScale;
                newPos.localPosition = existingPos.localPosition;

                __instance.m_ShowAllCardPosList.Add(newPos);
            }

            for (int i = 0; i < __instance.m_ShowAllCardPosList.Count; i++)
            {
                RectTransform rectTransform = (RectTransform)__instance.m_ShowAllCardPosList[i];
                float t = (float)i / (__instance.m_ShowAllCardPosList.Count - 1); // Interpolation linéaire
                float xPosition = Mathf.Lerp(-0.1f, 0.11f, t);
                Vector3 localPosition = rectTransform.localPosition;
                localPosition.x = xPosition;
                rectTransform.localPosition = localPosition;
            }
        }

        public static void OpenBooster(List<CardData> ___m_RolledCardDataList, List<float> ___m_CardValueList, ECollectionPackType ___m_CollectionPackType, Item ___m_CurrentItem, CardOpeningSequence __instance)
        {
            if (SavesManager.DebuggingSave)
            {
                return;
            }

            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            ___m_CardValueList.Clear();
            ___m_RolledCardDataList.Clear();
            totalExpGained = 0;

            List<WankulCardData> alreadySelectedCards = new List<WankulCardData>();

            for (int i = 0; i < boosterSize; i++)
            {
                bool isTerrain = i == 0;
                bool isMinRare = i == boosterSize - 1;
                bool isMinLegendary = false;
                int hash = ___m_CurrentItem.GetHashCode();

                if (isMinRare)
                {
                    foreach (int boosterHash in LegendaryBoosters)
                    {
                        if (boosterHash == hash)
                        {
                            LegendaryBoosters.Remove(boosterHash);
                            isMinLegendary = true;
                            isMinRare = false;
                            break;
                        }
                    }
                }
                WankulCardData wankulCard = WankulInventory.DropCard(___m_CollectionPackType, alreadySelectedCards, isTerrain, isMinRare, isMinLegendary);
                CardData associatedCard = wankulCardsData.GetCardDataFromWankulCardData(wankulCard);

                if (associatedCard == null)
                {
                    associatedCard = wankulCardsData.GetUnassciatedCardData();
                    ___m_RolledCardDataList.Add(associatedCard);

                    associatedCard.isFoil = false;
                    associatedCard.isChampionCard = false;

                    // Vérification de la rareté pour décider si la carte est foil
                    if (wankulCard is EffigyCardData)
                    {
                        EffigyCardData effigyCard = (EffigyCardData)wankulCard;

                        // Si la carte a une rareté de UR1 ou plus, elle devient foil
                        if (effigyCard.Rarity >= Rarity.UR1)
                        {
                            associatedCard.isFoil = true;
                        }
                    }

                    wankulCardsData.SetFromMonster(associatedCard, wankulCard);
                }
                else
                {
                    ___m_RolledCardDataList.Add(associatedCard);
                }

                // Calcul de l'XP gagnée
                totalExpGained += WankulCardsData.GetExperienceFromWankulCard(wankulCard);
                // Ajout de la valeur de la carte dans la liste des prix
                ___m_CardValueList.Add(wankulCard.MarketPrice);

            }
        }

        public class EvaluateOpenCardPack__State
        {
            public bool CanOpenCardBox;
        }

        public static void EvaluateOpenCardPackPreFix(out EvaluateOpenCardPack__State __state, InteractionPlayerController __instance)
        {
            __state = new EvaluateOpenCardPack__State();
            if (__instance.CanOpenCardBox())
            {
                __state.CanOpenCardBox = true;
            }
            else
            {
                __state.CanOpenCardBox = false;
            }
        }

        public static void EvaluateOpenCardPackPostFix(EvaluateOpenCardPack__State __state, InteractionPlayerController __instance)
        {
            if (__state.CanOpenCardBox)
            {
                List<Item> m_HoldItemList = (List<Item>)AccessTools.Field(__instance.GetType(), "m_HoldItemList").GetValue(__instance);
                int randomIndex = UnityEngine.Random.RandomRangeInt(0, m_HoldItemList.Count);
                Item item = m_HoldItemList[randomIndex];
                int hash = item.GetHashCode();
                LegendaryBoosters.Add(hash);
            }
        }

        private static IEnumerator DelayToState(int stateIndex, float delayTime, CardOpeningSequence __instance)
        {
            __instance.m_StateIndex = -1;
            yield return new WaitForSeconds(delayTime);
            __instance.m_StateIndex = stateIndex;
        }

        public static bool Update(CardOpeningSequence __instance)
        {
            MethodInfo InitOpenSequence = __instance.GetType().GetMethod("InitOpenSequence", BindingFlags.Instance | BindingFlags.NonPublic);

            Plugin.SetPProperty(__instance, "m_IsAutoFire", false);

            if (!(bool)Plugin.GetPProperty(__instance, "m_IsScreenActive"))
            {
                return false;
            }

            if (InputManager.GetKeyDownAction(EGameAction.OpenPack))
            {
                Plugin.SetPProperty(__instance, "m_IsAutoFireKeydown", true);
            }

            if (InputManager.GetKeyUpAction(EGameAction.OpenPack))
            {
                Plugin.SetPProperty(__instance, "m_IsAutoFireKeydown", false);
            }

            if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFireKeydown"))
            {
                Plugin.SetPProperty(__instance, "m_AutoFireTimer", (float)Plugin.GetPProperty(__instance, "m_AutoFireTimer") + Time.deltaTime);
                if ((float)Plugin.GetPProperty(__instance, "m_AutoFireTimer") >= 0.05f)
                {
                    Plugin.SetPProperty(__instance, "m_AutoFireTimer", 0f);
                    Plugin.SetPProperty(__instance, "m_IsAutoFire", true);
                }
            }
            else if ((float)Plugin.GetPProperty(__instance, "m_AutoFireTimer") > 0f)
            {
                Plugin.SetPProperty(__instance, "m_AutoFireTimer", 0f);
                Plugin.SetPProperty(__instance, "m_IsAutoFire", true);
            }

            if ((bool)Plugin.GetPProperty(__instance, "m_IsReadyingToOpen"))
            {
                if (!(bool)Plugin.GetPProperty(__instance, "m_IsReadyToOpen"))
                {
                    if ((bool)Plugin.GetPProperty(__instance, "m_IsCanceling"))
                    {
                        Plugin.SetPProperty(__instance, "m_LerpPosTimer", (float)Plugin.GetPProperty(__instance, "m_LerpPosTimer") - Time.deltaTime * (float)Plugin.GetPProperty(__instance, "m_LerpPosSpeed"));
                        if ((float)Plugin.GetPProperty(__instance, "m_LerpPosTimer") < 0f)
                        {
                            Plugin.SetPProperty(__instance, "m_LerpPosTimer", 0f);
                            Plugin.SetPProperty(__instance, "m_IsReadyToOpen", false);
                            Plugin.SetPProperty(__instance, "m_IsReadyingToOpen", false);
                            Plugin.SetPProperty(__instance, "m_IsCanceling", false);
                            Plugin.SetPProperty(__instance, "m_IsScreenActive", false);
                            __instance.m_CardPackAnimator.gameObject.SetActive(value: false);
                            CSingleton<InteractionPlayerController>.Instance.ExitLockMoveMode();
                            CSingleton<InteractionPlayerController>.Instance.OnExitOpenPackState();
                            InteractionPlayerController.RestoreHiddenToolTip();
                            ((Item)Plugin.GetPProperty(__instance, "m_CurrentItem")).gameObject.SetActive(value: true);
                            InteractionPlayerController.SetAllHoldItemVisibility(isVisible: true);
                            Plugin.SetPProperty(__instance, "m_CurrentItem", null);
                            TutorialManager.SetGameUIVisible(isVisible: true);
                            CenterDot.SetVisibility(isVisible: true);
                            GameUIScreen.ResetEnterGoNextDayIndicatorVisible();
                        }
                    }
                    else
                    {
                        Plugin.SetPProperty(__instance, "m_LerpPosTimer", (float)Plugin.GetPProperty(__instance, "m_LerpPosTimer") + Time.deltaTime * (float)Plugin.GetPProperty(__instance, "m_LerpPosSpeed"));
                        if ((float)Plugin.GetPProperty(__instance, "m_LerpPosTimer") > 1f)
                        {
                            Plugin.SetPProperty(__instance, "m_LerpPosTimer", 1f);
                            Plugin.SetPProperty(__instance, "m_IsReadyToOpen", true);
                        }
                    }

                    __instance.m_CardPackAnimator.transform.localPosition = Vector3.Lerp(__instance.m_StartLerpTransform.localPosition, Vector3.zero, (float)Plugin.GetPProperty(__instance, "m_LerpPosTimer"));
                    __instance.m_CardPackAnimator.transform.localRotation = Quaternion.Lerp(__instance.m_StartLerpTransform.localRotation, Quaternion.identity, (float)Plugin.GetPProperty(__instance, "m_LerpPosTimer"));
                    __instance.m_CardPackAnimator.transform.localScale = Vector3.Lerp(__instance.m_StartLerpTransform.localScale, Vector3.one, (float)Plugin.GetPProperty(__instance, "m_LerpPosTimer"));
                }
                else if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFire"))
                {
                    Plugin.SetPProperty(__instance, "m_IsReadyingToOpen", false);
                    ECollectionPackType collectionPackType = InventoryBase.ItemTypeToCollectionPackType(((Item)Plugin.GetPProperty(__instance, "m_CurrentItem")).GetItemType());
                    __instance.OpenScreen(collectionPackType, false);
                }
                else if (InputManager.GetKeyDownAction(EGameAction.CancelOpenPack) && !(bool)Plugin.GetPProperty(__instance, "m_IsCanceling"))
                {
                    CSingleton<InteractionPlayerController>.Instance.AddHoldItemToFront((Item)Plugin.GetPProperty(__instance, "m_CurrentItem"));
                    Plugin.SetPProperty(__instance, "m_IsCanceling", true);
                    Plugin.SetPProperty(__instance, "m_IsReadyToOpen", false);
                    CSingleton<InteractionPlayerController>.Instance.m_BlackBGWorldUIFade.SetFadeOut(3f);
                    InteractionPlayerController.RestoreHiddenToolTip();
                    CSingleton<InteractionPlayerController>.Instance.m_CameraFOVController.StopLerpFOV();
                    SoundManager.GenericPop(1f, 0.9f);
                }
            }
            else
            {
                if (!(bool)Plugin.GetPProperty(__instance, "m_IsScreenActive"))
                {
                    return false;
                }

                if (__instance.m_StateIndex == 0)
                {
                    InitOpenSequence.Invoke(__instance, []);
                    __instance.m_StateIndex++;
                }
                else if (__instance.m_StateIndex == 1)
                {
                    Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + Time.deltaTime * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                    if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") > 0.05f)
                    {
                        Plugin.SetPProperty(__instance, "m_StateTimer", 0f);
                        if ((int)Plugin.GetPProperty(__instance, "m_TempIndex") < __instance.m_Card3dUIList.Count)
                        {
                            __instance.m_Card3dUIList[(int)Plugin.GetPProperty(__instance, "m_TempIndex")].gameObject.SetActive(value: true);
                            Plugin.SetPProperty(__instance, "m_TempIndex", (int)Plugin.GetPProperty(__instance, "m_TempIndex") + 1);
                        }
                    }

                    if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFire") || (bool)Plugin.GetPProperty(__instance, "m_IsAutoFireKeydown") || CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + 0.0065f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                        __instance.m_CardPackAnimator.Play("PackOpenAnim", -1, (float)Plugin.GetPProperty(__instance, "m_Slider"));
                        if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.3f)
                        {
                            __instance.m_OpenPackVFX.Play();
                            SoundManager.PlayAudio("SFX_OpenPack", 0.6f);
                            SoundManager.PlayAudio("SFX_BoxOpen", 0.5f);
                            __instance.m_StateIndex++;
                        }
                    }
                }
                else if (__instance.m_StateIndex == 2)
                {
                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime * 1f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                    __instance.m_CardPackAnimator.Play("PackOpenAnim", -1, (float)Plugin.GetPProperty(__instance, "m_Slider"));
                    Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + Time.deltaTime);
                    if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") > 0.05f)
                    {
                        Plugin.SetPProperty(__instance, "m_StateTimer", 0f);
                        if ((int)Plugin.GetPProperty(__instance, "m_TempIndex") < __instance.m_Card3dUIList.Count)
                        {
                            __instance.m_Card3dUIList[(int)Plugin.GetPProperty(__instance, "m_TempIndex")].gameObject.SetActive(value: true);
                            Plugin.SetPProperty(__instance, "m_TempIndex", (int)Plugin.GetPProperty(__instance, "m_TempIndex") + 1);
                        }
                    }

                    if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 1f)
                    {
                        InteractionPlayerController.RemoveToolTip(EGameAction.OpenPack);
                        Plugin.SetPProperty(__instance, "m_TempIndex", 0);
                        Plugin.SetPProperty(__instance, "m_StateTimer", 0f);
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        __instance.m_StateIndex++;
                        for (int i = 0; i < __instance.m_Card3dUIList.Count; i++)
                        {
                            __instance.m_Card3dUIList[i].gameObject.SetActive(value: true);
                        }
                    }
                }
                else if (__instance.m_StateIndex == 3)
                {
                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime * 1f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                    if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.15f)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        __instance.m_StateIndex++;
                        __instance.m_CardOpeningRotateToFrontAnim.Play("CardOpenSeq1_RotateToFront");
                    }
                    else if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFire") || CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard)
                    {
                        float num = 0.002f * (float)(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        float num2 = 0.001f * (float)(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num2, 1f + num);
                        __instance.m_CardOpeningRotateToFrontAnim.Play("CardOpenSeq1_RotateToFront");
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        Plugin.SetPProperty(__instance, "m_StateTimer", 0f);
                        __instance.m_StateIndex++;
                    }
                }
                else if (__instance.m_StateIndex == 4)
                {
                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime * 1f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                    if (!__instance.m_CardOpeningSequenceUI.m_CardValueTextGrp.activeSelf && (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") < (boosterSize - 1) && (float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.45f && !((List<bool>)Plugin.GetPProperty(__instance, "m_IsNewlList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")] && ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")] < (float)Plugin.GetPProperty(__instance, "m_HighValueCardThreshold"))
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                    }

                    if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.8f)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        if (((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")] >= (float)Plugin.GetPProperty(__instance, "m_HighValueCardThreshold"))
                        {
                            SoundManager.PlayAudio("SFX_FinalizeCard", 0.6f, 1.2f);
                            __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].Play("OpenCardNewCard");
                            __instance.m_HighValueCardIcon.SetActive(value: true);
                            __instance.StartCoroutine(DelayToState(5, 0.9f, __instance));
                            Plugin.SetPProperty(__instance, "m_TotalCardValue", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                            Plugin.SetPProperty(__instance, "m_IsGetHighValueCard", true);
                        }
                        else if (((List<bool>)Plugin.GetPProperty(__instance, "m_IsNewlList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")])
                        {
                            SoundManager.PlayAudio("SFX_CardReveal0", 0.6f);
                            __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].Play("OpenCardNewCard");
                            __instance.m_NewCardIcon.SetActive(value: true);
                            __instance.StartCoroutine(DelayToState(5, 0.9f, __instance));
                            Plugin.SetPProperty(__instance, "m_TotalCardValue", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                        }
                        else
                        {
                            __instance.m_StateIndex++;
                        }
                    }
                }
                else if (__instance.m_StateIndex == 5)
                {
                    if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFire") || (!(bool)Plugin.GetPProperty(__instance,"m_IsGetHighValueCard") && CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard))
                    {
                        int num3 = UnityEngine.Random.Range(0, 3);
                        float num4 = 0.002f * (float)(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        float num5 = 0.001f * (float)(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        switch (num3)
                        {
                            case 0:
                                SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num5, 1f + num4);
                                break;
                            case 1:
                                SoundManager.PlayAudio("SFX_CardReveal2", 0.6f + num5, 1f + num4);
                                break;
                            default:
                                SoundManager.PlayAudio("SFX_CardReveal3", 0.6f + num5, 1f + num4);
                                break;
                        }
                        if ((int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") >= boosterSize)
                        {
                            __instance.m_StateIndex = 7;
                        }
                        else
                        {
                            __instance.m_StateIndex++;
                            __instance.m_NewCardIcon.SetActive(value: false);
                            __instance.m_HighValueCardIcon.SetActive(value: false);
                            __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].Play("OpenCardSlideExit");
                            __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]["OpenCardSlideExit"].speed = 1f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer");
                            __instance.m_CardOpeningSequenceUI.HideSingleCardValue();
                        }

                        Plugin.SetPProperty(__instance, "m_IsGetHighValueCard", false);
                    }
                }
                else if (__instance.m_StateIndex == 6)
                {
                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime * 1f * (float)Plugin.GetPProperty(__instance, "m_MultiplierStateTimer"));
                    if (!__instance.m_CardOpeningSequenceUI.m_CardValueTextGrp.activeSelf && (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") < (boosterSize - 1) && (float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.3f && !((List<bool>)Plugin.GetPProperty(__instance, "m_IsNewlList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1] && ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1] < (float)Plugin.GetPProperty(__instance, "m_HighValueCardThreshold"))
                    {
                        Plugin.SetPProperty(__instance, "m_TotalCardValue", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1]);
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1]);
                    }

                    if (!((float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.5f))
                    {
                        return false;
                    }

                    Plugin.SetPProperty(__instance, "m_Slider", 0f);
                    if (__instance.m_Card3dUIList.Count > (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex"))
                    {
                        __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].transform.localPosition = Vector3.zero;
                        __instance.m_Card3dUIList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].gameObject.SetActive(value: false);
                    }

                    Plugin.SetPProperty(__instance, "m_CurrentOpenedCardIndex", (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1);
                    if ((int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") >= boosterSize)
                    {
                        Plugin.SetPProperty(__instance, "m_IsGetHighValueCard", false);
                        __instance.m_StateIndex = 7;
                        return false;
                    }

                    if (__instance.m_Card3dUIList.Count > (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1)
                    {
                        __instance.m_Card3dUIList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1].gameObject.SetActive(value: true);
                    }

                    if (((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")] >= (float)Plugin.GetPProperty(__instance, "m_HighValueCardThreshold"))
                    {
                        SoundManager.PlayAudio("SFX_FinalizeCard", 0.6f, 1.2f);
                        __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].Play("OpenCardNewCard");
                        __instance.m_HighValueCardIcon.SetActive(value: true);
                        __instance.StartCoroutine(DelayToState(5, 0.9f, __instance));
                        Plugin.SetPProperty(__instance, "m_TotalCardValue", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                        Plugin.SetPProperty(__instance, "m_IsGetHighValueCard", true);
                    }
                    else if (((List<bool>)Plugin.GetPProperty(__instance, "m_IsNewlList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")])
                    {
                        SoundManager.PlayAudio("SFX_CardReveal0", 0.6f);
                        __instance.m_CardAnimList[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")].Play("OpenCardNewCard");
                        __instance.m_NewCardIcon.SetActive(value: true);
                        __instance.StartCoroutine(DelayToState(5, 0.9f, __instance));
                        Plugin.SetPProperty(__instance, "m_TotalCardValue", (float)Plugin.GetPProperty(__instance, "m_TotalCardValue") + ((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(((List<float>)Plugin.GetPProperty(__instance, "m_CardValueList"))[(int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex")]);
                    }
                    else
                    {
                        __instance.m_StateIndex = 5;
                    }
                }
                else if (__instance.m_StateIndex == 7)
                {
                    if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") == 0f && (float)Plugin.GetPProperty(__instance, "m_Slider") == 0f)
                    {
                        SoundManager.PlayAudio("SFX_PercStarJingle3", 0.6f);
                        SoundManager.PlayAudio("SFX_Gift", 0.6f);
                    }

                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime);
                    if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 0.05f)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", 0);
                        __instance.m_CardAnimList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].transform.position = __instance.m_ShowAllCardPosList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].position;
                        __instance.m_CardAnimList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].transform.rotation = __instance.m_ShowAllCardPosList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].rotation;
                        __instance.m_Card3dUIList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].gameObject.SetActive(value: true);
                        __instance.m_CardAnimList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].Play("OpenCardFinalReveal");
                        Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + 1f);
                        if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") >= (float)__instance.m_Card3dUIList.Count)
                        {
                            Plugin.SetPProperty(__instance, "m_StateTimer", 0f);
                            __instance.m_StateIndex++;
                            __instance.m_CardOpeningSequenceUI.StartShowTotalValue((float)Plugin.GetPProperty(__instance, "m_TotalCardValue"), (bool)Plugin.GetPProperty(__instance, "m_HasFoilCard"));
                        }
                    }
                }
                else if (__instance.m_StateIndex == 8)
                {
                    Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + Time.deltaTime);
                    if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") >= 0.02f)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        __instance.m_Card3dUIList[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].m_NewCardIndicator.gameObject.SetActive(((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[(int)(float)Plugin.GetPProperty(__instance, "m_StateTimer")].isNew);
                        Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + 1f);
                        if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") >= (float)__instance.m_Card3dUIList.Count)
                        {
                            __instance.m_StateIndex++;
                        }
                    }
                }
                else if (__instance.m_StateIndex == 9)
                {
                    Plugin.SetPProperty(__instance, "m_Slider", (float)Plugin.GetPProperty(__instance, "m_Slider") + Time.deltaTime);
                    if ((float)Plugin.GetPProperty(__instance, "m_Slider") >= 1f)
                    {
                        Plugin.SetPProperty(__instance, "m_Slider", 0f);
                        __instance.m_StateIndex++;
                    }
                }
                else if (__instance.m_StateIndex == 10)
                {
                    if ((bool)Plugin.GetPProperty(__instance, "m_IsAutoFire"))
                    {
                        __instance.m_StateIndex++;
                    }
                }
                else if (__instance.m_StateIndex == 11)
                {
                    Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + Time.deltaTime *1f);
                    if (!((float)Plugin.GetPProperty(__instance, "m_StateTimer") >= 0.01f))
                    {
                        return false;
                    }

                    Plugin.SetPProperty(__instance, "m_Slider", 0f);
                    Plugin.SetPProperty(__instance, "m_IsScreenActive", false);
                    Plugin.SetPProperty(__instance, "m_IsReadyToOpen", false);
                    __instance.m_CardPackAnimator.gameObject.SetActive(value: false);
                    __instance.m_CardOpeningUIGroup.SetActive(value: false);
                    __instance.m_CardOpeningSequenceUI.HideTotalValue();
                    CSingleton<InteractionPlayerController>.Instance.ExitLockMoveMode();
                    CSingleton<InteractionPlayerController>.Instance.OnExitOpenPackState();
                    if ((bool)(Item)Plugin.GetPProperty(__instance, "m_CurrentItem"))
                    {
                        ((Item)Plugin.GetPProperty(__instance, "m_CurrentItem")).DisableItem();
                    }

                    Plugin.SetPProperty(__instance, "m_CurrentItem", null);
                    int num6 = 0;
                    Plugin.SetPProperty(__instance, "m_TotalCardValue", 0f);
                    Plugin.SetPProperty(__instance, "m_TotalExpGained", 0);
                    bool isGet = false;
                    bool isGet2 = false;
                    for (int j = 0; j < ((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList")).Count; j++)
                    {
                        int num7 = (int)(((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].GetCardBorderType() + 1) * Mathf.CeilToInt((float)(((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].borderType + 1) / 2f);
                        if (((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].isFoil)
                        {
                            num7 *= 8;
                        }

                        Plugin.SetPProperty(__instance, "m_TotalExpGained", (int)Plugin.GetPProperty(__instance, "m_TotalExpGained") + num7);
                        if (((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].GetCardBorderType() == ECardBorderType.FullArt && ((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].isFoil)
                        {
                            isGet = true;
                            if (((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].expansionType == ECardExpansionType.Ghost)
                            {
                                isGet2 = true;
                            }
                        }

                        if (((List<CardData>)Plugin.GetPProperty(__instance, "m_RolledCardDataList"))[j].isNew)
                        {
                            num6++;
                        }
                    }

                    if ((int)Plugin.GetPProperty(__instance, "m_TotalExpGained") > 0)
                    {
                        CEventManager.QueueEvent(new CEventPlayer_AddShopExp((int)Plugin.GetPProperty(__instance, "m_TotalExpGained")));
                    }

                    for (int k = 0; k < __instance.m_CardAnimList.Count; k++)
                    {
                        __instance.m_CardAnimList[k].transform.localPosition = Vector3.zero;
                        __instance.m_CardAnimList[k].transform.localRotation = Quaternion.identity;
                        __instance.m_Card3dUIList[k].m_NewCardIndicator.gameObject.SetActive(value: false);
                        __instance.m_CardAnimList[k].Play("OpenCardDefaultPos");
                    }

                    if (CSingleton<InteractionPlayerController>.Instance.GetHoldItemCount() <= 0)
                    {
                        TutorialManager.SetGameUIVisible(isVisible: true);
                        CenterDot.SetVisibility(isVisible: true);
                        GameUIScreen.ResetEnterGoNextDayIndicatorVisible();
                        CSingleton<InteractionPlayerController>.Instance.m_BlackBGWorldUIFade.SetFadeOut(3f);
                        CSingleton<InteractionPlayerController>.Instance.m_CameraFOVController.StopLerpFOV();
                        Plugin.SetPProperty(__instance, "m_IsAutoFireKeydown", false);
                        Plugin.SetPProperty(__instance, "m_AutoFireTimer", 0f);
                    }

                    CSingleton<CustomerManager>.Instance.PlayerFinishOpenCardPack();
                    CSingleton<InteractionPlayerController>.Instance.EvaluateOpenCardPack();
                    TutorialManager.AddTaskValue(ETutorialTaskCondition.OpenPack, 1f);
                    CPlayerData.m_GameReportDataCollect.cardPackOpened++;
                    CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened++;
                    AchievementManager.OnCardPackOpened(CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened);
                    AchievementManager.OnGetFullArtFoil(isGet);
                    AchievementManager.OnGetFullArtGhostFoil(isGet2);
                    if (num6 > 0)
                    {
                        AchievementManager.OnCheckAlbumCardCount(CPlayerData.GetTotalCardCollectedAmount());
                    }
                }
                else if (__instance.m_StateIndex == 12)
                {
                    Plugin.SetPProperty(__instance, "m_IsScreenActive", false);
                }
                else if (__instance.m_StateIndex == 101)
                {
                    _ = (float)Plugin.GetPProperty(__instance, "m_StateTimer");
                    _ = 0f;
                    Plugin.SetPProperty(__instance, "m_StateTimer", (float)Plugin.GetPProperty(__instance, "m_StateTimer") + Time.deltaTime);
                    if ((float)Plugin.GetPProperty(__instance, "m_StateTimer") >= 0.05f)
                    {
                        int num8 = UnityEngine.Random.Range(0, 3);
                        float num9 = 0.002f * (float)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        float num10 = 0.001f * (float)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex");
                        switch (num8)
                        {
                            case 0:
                                SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num10, 1f + num9);
                                break;
                            case 1:
                                SoundManager.PlayAudio("SFX_CardReveal2", 0.6f + num10, 1f + num9);
                                break;
                            default:
                                SoundManager.PlayAudio("SFX_CardReveal3", 0.6f + num10, 1f + num9);
                                break;
                        }

                        Plugin.SetPProperty(__instance, "m_CurrentOpenedCardIndex", (int)Plugin.GetPProperty(__instance, "m_CurrentOpenedCardIndex") + 1);
                    }
                }
                else
                {
                    _ = __instance.m_StateIndex;
                    _ = -1;
                }
            }

            return false;
        }
    }
}
