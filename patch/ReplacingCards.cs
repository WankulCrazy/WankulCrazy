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
            __instance.m_CardFoilMaskImage.sprite = wankulCardData.SpriteMask;
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

        __instance.m_CardFront.transform.localPosition = new Vector3(-4.2517f, 0.3211f, 0.2817f);

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
        __instance.m_MonsterMask?.gameObject.SetActive(false);

        if (__instance.m_NormalGrp != null)
        {
            int childCount = __instance.m_NormalGrp.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = __instance.m_NormalGrp.transform.GetChild(i);
                if (child.gameObject.name == "CardFront")
                {
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform subChild = child.GetChild(j);
                        if (subChild.gameObject.name == "CardFoilMask")
                        {
                            subChild.gameObject.SetActive(false);
                        }
                    }
                }
            }
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

    public static bool CollectionBinderFlipAnimCtrlOnRightMouseButtonUp(CollectionBinderFlipAnimCtrl __instance)
    {
        ECollectionSortingType m_SortingType = (ECollectionSortingType)AccessTools.Field(__instance.GetType(), "m_SortingType").GetValue(__instance);
        if ((__instance.m_IsHoldingCardCloseUp || __instance.m_IsExitingCardCloseUp) && __instance.m_IsHoldingCardCloseUp)
        {
            _ = __instance.m_IsExitingCardCloseUp;
        }
        if (!__instance.m_CurrentRaycastedInteractableCard3d)
        {
            return false;
        }
        if (!InteractionPlayerController.HasEnoughSlotToHoldCard())
        {
            NotEnoughResourceTextPopup.ShowText(ENotEnoughResourceText.HandFull);
            return false;
        }
        Card3dUIGroup cardUI = CSingleton<Card3dUISpawner>.Instance.GetCardUI();
        __instance.m_CurrentSpawnedInteractableCard3d = ShelfManager.SpawnInteractableObject(EObjectType.Card3d).GetComponent<InteractableCard3d>();
        cardUI.m_IgnoreCulling = true;
        cardUI.m_CardUI.SetFoilCullListVisibility(isActive: true);
        cardUI.m_CardUI.ResetFarDistanceCull();
        cardUI.m_CardUI.SetFoilMaterialList(CSingleton<Card3dUISpawner>.Instance.m_FoilMaterialWorldView);
        cardUI.m_CardUI.SetFoilBlendedMaterialList(CSingleton<Card3dUISpawner>.Instance.m_FoilBlendedMaterialWorldView);
        cardUI.m_CardUI.SetCardUI(__instance.m_CurrentRaycastedInteractableCard3d.m_Card3dUI.m_CardUI.GetCardData());
        cardUI.transform.position = __instance.m_CurrentRaycastedInteractableCard3d.transform.position;
        cardUI.transform.rotation = __instance.m_CurrentRaycastedInteractableCard3d.transform.rotation;
        __instance.m_CurrentSpawnedInteractableCard3d.transform.position = __instance.m_CurrentRaycastedInteractableCard3d.transform.position;
        __instance.m_CurrentSpawnedInteractableCard3d.transform.rotation = __instance.m_CurrentRaycastedInteractableCard3d.transform.rotation;
        __instance.m_CurrentSpawnedInteractableCard3d.SetCardUIFollow(cardUI);
        __instance.m_CurrentSpawnedInteractableCard3d.SetEnableCollision(isEnable: false);
        CardData cardData = __instance.m_CurrentRaycastedInteractableCard3d.m_Card3dUI.m_CardUI.GetCardData();
        CPlayerData.ReduceCard(cardData, 1);

        (WankulCardData wankulCardData, CardData cardData, int amount) inventoryCard = WankulInventory.GetWankulCardFormGameCard(cardData);
        int count = inventoryCard.amount;
        int m_CurrentRaycastedCardIndex = (int)AccessTools.Field(__instance.GetType(), "m_CurrentRaycastedCardIndex").GetValue(__instance);
        __instance.m_BinderPageGrpList[0].SetSingleCard(m_CurrentRaycastedCardIndex, cardData, count, m_SortingType);
        if (count <= 0)
        {
            __instance.m_CurrentRaycastedInteractableCard3d.m_Card3dUI.gameObject.SetActive(value: false);
            __instance.m_CurrentRaycastedInteractableCard3d.gameObject.SetActive(value: false);
            __instance.m_CurrentRaycastedInteractableCard3d.OnRaycastEnded();
            __instance.m_CurrentRaycastedInteractableCard3d = null;
        }
        InteractionPlayerController.AddHoldCard(__instance.m_CurrentSpawnedInteractableCard3d);
        InteractionPlayerController.RemoveToolTip(EGameAction.ViewAlbumCard);

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
