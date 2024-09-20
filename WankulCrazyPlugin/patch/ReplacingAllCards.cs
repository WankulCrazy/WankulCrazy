using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;
using Logger = HarmonyLib.Tools.Logger;

namespace WankulCrazyPlugin;
public class ReplacingAllCards
{


    // ...

    static void SetCardUI_patch(CardData cardData, CardUI __instance)
    {
        WankulCardsData cardsData = WankulCardsData.Instance;

        WankulCardData wankulCardData = cardsData.GetFromMonster(cardData);

        __instance.m_CardBGImage.sprite = wankulCardData.Sprite;


        __instance.m_MonsterImage.gameObject.SetActive(false);
        __instance.m_MonsterMaskImage.gameObject.SetActive(false);
        __instance.m_RarityImage.gameObject.SetActive(false);
        __instance.m_AncientArtifactImage.gameObject.SetActive(false);
        __instance.m_NumberText.gameObject.SetActive(false);
        __instance.m_MonsterNameText.gameObject.SetActive(false);
        __instance.m_RarityText.gameObject.SetActive(false);
        __instance.m_Stat1Text.gameObject.SetActive(false);
        __instance.m_Stat2Text.gameObject.SetActive(false);
        __instance.m_Stat3Text.gameObject.SetActive(false);
        __instance.m_Stat4Text.gameObject.SetActive(false);
        __instance.m_DescriptionText.gameObject.SetActive(false);
        __instance.m_ArtistText.gameObject.SetActive(false);
        __instance.m_FameText.gameObject.SetActive(false);
        __instance.m_FirstEditionText.gameObject.SetActive(false);
        __instance.m_ChampionText.gameObject.SetActive(false);
        __instance.m_EvoGrp.SetActive(false);
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
            __state.wankulCardData = WankulCardsData.Instance.GetFromMonster(__state.cardData);
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
            Plugin.Logger.LogInfo("EnterViewUpCloseState");
            Plugin.Logger.LogInfo("WankulCard Title : " + __state.wankulCardData.Title);

            if (__state.wankulCardData is EffigyCardData) {
                EffigyCardData effigyCard = (EffigyCardData)__state.wankulCardData;
                Plugin.Logger.LogInfo("WankulCard FullRarityName : " + effigyCard.Rarity);
            } else
            {
                Plugin.Logger.LogInfo("WankulCard FullRarityName : Terrain");
            }

            __instance.m_CollectionBinderUI.m_CardNameText.text = "Wankul";
            __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text = "Wankul";
            Plugin.Logger.LogInfo("CardName : " + __instance.m_CollectionBinderUI.m_CardNameText.text);
            Plugin.Logger.LogInfo("CardFullRarityName : " + __instance.m_CollectionBinderUI.m_CardFullRarityNameText.text);
        }
    }

}
