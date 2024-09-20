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
        if (cardsData == null)
        {
            Plugin.Logger.LogError("CardsData singleton instance is null in SetCardUI_patch");
            return;
        }

        Plugin.Logger.LogInfo("Setting card UI");

        WankulCardData wankulCardData = cardsData.GetFromMonster(cardData.monsterType);

        __instance.m_CardBGImage.sprite = wankulCardData.Sprite;


        __instance.m_MonsterImage.gameObject.SetActive(false);
        __instance.m_MonsterMaskImage.gameObject.SetActive(false);
        __instance.m_RarityImage.gameObject.SetActive(false);
        __instance.m_AncientArtifactImage.gameObject.SetActive(false);

        __instance.m_NumberText.text = "";
        __instance.m_NumberText.enabled = false;
        __instance.m_NumberText.gameObject.SetActive(false);

        __instance.m_MonsterNameText.text = "";
        __instance.m_MonsterNameText.enabled = false;
        __instance.m_MonsterNameText.gameObject.SetActive(false);

        __instance.m_RarityText.text = "";
        __instance.m_RarityText.enabled = false;
        __instance.m_RarityText.gameObject.SetActive(false);

        __instance.m_Stat1Text.text = "";
        __instance.m_Stat1Text.enabled = false;
        __instance.m_Stat1Text.gameObject.SetActive(false);

        __instance.m_Stat2Text.text = "";
        __instance.m_Stat2Text.enabled = false;
        __instance.m_Stat2Text.gameObject.SetActive(false);

        __instance.m_Stat3Text.text = "";
        __instance.m_Stat3Text.enabled = false;
        __instance.m_Stat3Text.gameObject.SetActive(false);

        __instance.m_Stat4Text.text = "";
        __instance.m_Stat4Text.enabled = false;
        __instance.m_Stat4Text.gameObject.SetActive(false);

        __instance.m_DescriptionText.text = "";
        __instance.m_DescriptionText.enabled = false;
        __instance.m_DescriptionText.gameObject.SetActive(false);

        __instance.m_ArtistText.text = "";
        __instance.m_ArtistText.enabled = false;
        __instance.m_ArtistText.gameObject.SetActive(false);


        __instance.m_FameText.text = "";
        __instance.m_FameText.enabled = false;
        __instance.m_FameText.gameObject.SetActive(false);

        __instance.m_FirstEditionText.text = "";
        __instance.m_FirstEditionText.enabled = false;
        __instance.m_FirstEditionText.gameObject.SetActive(false);

        __instance.m_ChampionText.text = "";
        __instance.m_ChampionText.enabled = false;
        __instance.m_ChampionText.gameObject.SetActive(false);
    }

}
