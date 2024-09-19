using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;
using Logger = HarmonyLib.Tools.Logger;

namespace WankulCrazyPlugin;
public class ReplacingAllCards
{

    private static int cardIndex = 0; // Changed to static

    static bool Start_patch(TitleScreen __instance)
    {
        Plugin.Logger.LogInfo("Harmony Loaded and functionning");
        // Import JSON data
        JsonImporter.ImportJson();
        Plugin.Logger.LogInfo("JSON data imported");
        return true;
    }

    static void SetCardUI_patch(CardUI __instance)
    {
        CardsData cardsData = CardsData.Instance;
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

        CardData cardData = cardsData.cards[cardIndex];
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
