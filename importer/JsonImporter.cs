using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BepInEx.Logging;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.importer;
public class JsonImporter
{

    public static void ImportJson()
    {
        string pluginPath = Plugin.GetPluginPath();
        string path = Path.Combine(pluginPath, "data/formated_wankul_cards.json");

        string jsonContent = File.ReadAllText(path);

        try
        {
            JObject jsonObject = JObject.Parse(jsonContent);
            List<WankulCardData> cards = DeserializeCards(jsonObject);

            if (cards.Count > 0)
            {
                CreateCardsData(cards);
                WankulCardsData wankulCardsData = WankulCardsData.Instance;
                Plugin.Logger.LogInfo("Cards data loaded: " + wankulCardsData.cards.Count + " cards");
            }
            else
            {
                Plugin.Logger.LogError("Failed to deserialize JSON: cards list is empty");
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError("Failed to deserialize JSON: " + ex.Message);
        }

    }

    private static List<WankulCardData> DeserializeCards(JObject jsonObject)
    {
        List<WankulCardData> cards = [];

        JToken wankulsToken = jsonObject["wankuls"];
        if (wankulsToken != null)
        {
            List<EffigyCardData> wankuls = wankulsToken.ToObject<List<EffigyCardData>>(new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            cards.AddRange(wankuls);
        }

        JToken terrainsToken = jsonObject["terrains"];
        if (terrainsToken != null)
        {
            List<TerrainCardData> terrains = terrainsToken.ToObject<List<TerrainCardData>>(new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            cards.AddRange(terrains);
        }

        JToken specialsToken = jsonObject["specials"];
        if (specialsToken != null)
        {
            List<SpecialCardData> specials = specialsToken.ToObject<List<SpecialCardData>>(new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            cards.AddRange(specials);
        }

        return cards;
    }

    private static void CreateCardsData(List<WankulCardData> cards)
    {
        WankulCardsData cardsData = WankulCardsData.Instance;

        cardsData.cards = cards;
        foreach (var card in cardsData.cards)
        {
            if (!string.IsNullOrEmpty(card.TexturePath))
            {
                string pluginPath = Plugin.GetPluginPath();
                string texturepath = Path.Combine(pluginPath, "data", card.TexturePath);
                string texturepathmask = Path.Combine(pluginPath, "data/masks", card.TexturePath);

                Texture2D texture = LoadTexture(texturepath);
                Texture2D texturemask = null;

                if (File.Exists(texturepathmask))
                {
                    texturemask = LoadTexture(texturepathmask);
                }

                if (texture != null)
                {
                    card.Texture = texture;
                }
                else
                {
                    Plugin.Logger.LogError("Failed to load texture: " + texturepath);
                }

                if (texturemask != null)
                {
                    card.TextureMask = texturemask;
                }
                else if (File.Exists(texturepathmask))
                {
                    Plugin.Logger.LogError("Failed to load texture mask: " + texturepathmask);
                }

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                Sprite spritemask = null;

                if (texturemask != null)
                {
                    spritemask = Sprite.Create(texturemask, new Rect(0, 0, texturemask.width, texturemask.height), Vector2.zero);
                }

                if (sprite != null)
                {
                    card.Sprite = sprite;
                }
                else
                {
                    Plugin.Logger.LogError("Failed to create sprite: " + texturepath);
                }

                if (spritemask != null)
                {
                    card.SpriteMask = spritemask;
                }
                else if (texturemask != null)
                {
                    Plugin.Logger.LogError("Failed to create sprite mask: " + texturepathmask);
                }
            }
        }
    }

    private static Texture2D LoadTexture(string path)
    {
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        return texture;
    }
}
