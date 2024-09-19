using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;
using Logger = HarmonyLib.Tools.Logger;

namespace WankulCrazyPlugin;
public class ReplacingAllCards
{

    private static int cardIndex = 0; // Changed to static

    static void SetCardUI_patch(CardUI __instance)
    {
        WankulCardsData cardsData = WankulCardsData.Instance;
        if (cardsData == null)
        {
            Plugin.Logger.LogError("CardsData singleton instance is null in SetCardUI_patch");
            return;
        }

        int cardCount = cardsData.cards.Count;

        Plugin.Logger.LogInfo("SetCardUI_patch called");
        Plugin.Logger.LogInfo("Card count: " + cardCount);

        if (cardIndex >= cardCount)
        {
            cardIndex = 0;
        }

        Plugin.Logger.LogInfo("Setting card UI");
        Plugin.Logger.LogInfo("Count is : " + cardCount);
        Plugin.Logger.LogInfo("Index is : " + cardIndex);

        WankulCardData cardData = cardsData.cards[cardIndex];
        cardIndex++;

        Sprite cardSprite = Sprite.Create(cardData.Texture, new Rect(0, 0, cardData.Texture.width, cardData.Texture.height), Vector2.zero);

        if (cardSprite == null)
        {
            Plugin.Logger.LogError("Failed to create sprite");
        }
        else
        {
            Plugin.Logger.LogInfo("Sprite created");
        }

        __instance.m_CardBGImage.sprite = cardSprite;
    }

}
