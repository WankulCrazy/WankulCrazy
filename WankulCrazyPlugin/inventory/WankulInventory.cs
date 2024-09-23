using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.inventory
{

    public class WankulInventory : Singleton<WankulInventory>
    {
        public Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = [];

        public static Season ConvertPackTypeToSeason(ECollectionPackType packType)
        {
            switch (packType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Season.S01;

                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Season.S02;

                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Season.S03;

                default:
                    return Season.HS;
            }

        }

        public static WankulCardData DropCard(ECollectionPackType packType, bool isTerrain = false, bool isMinRare = false)
        {
            List<WankulCardData> allCards = WankulCardsData.Instance.cards;

            if (isTerrain)
            {
                allCards = allCards.FindAll(card => card is TerrainCardData);
            }
            else
            {
                allCards = allCards.FindAll(card => card is not TerrainCardData);
            }

            // Filtrer les cartes déjà associées
            List<WankulCardData> seasonalCard =
                allCards.FindAll(card => card.Season == ConvertPackTypeToSeason(packType));


            if (!isTerrain && isMinRare)
            {
                List<EffigyCardData> effigyCardsData = seasonalCard
                    .FindAll(card => card is EffigyCardData)
                    .ConvertAll(card => (EffigyCardData)card);

                seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.R)
                    .ConvertAll(card => (WankulCardData)card);
                ;
            }
            else if (!isTerrain && !isMinRare)
            {
                // Filtrer toutes les cartes avec une rareté inférieure à Rare, pas seulement les EffigyCardData
                seasonalCard = seasonalCard.FindAll(card =>
                    !(card is EffigyCardData effigyCard && effigyCard.Rarity >= Rarity.R)
                );
            }


            if (seasonalCard.Count == 0)
            {
                Plugin.Logger.LogError("No available cards to drop");
                return null;
            }

            float totalDropChance = 0f;
            foreach (var card in seasonalCard)
            {
                totalDropChance += card.Drop;
            }

            float randomValue = Random.Range(0f, totalDropChance);
            float cumulativeDropChance = 0f;

            foreach (var card in seasonalCard)
            {
                cumulativeDropChance += card.Drop;
                if (randomValue <= cumulativeDropChance)
                {
                    string raritystrg = card is EffigyCardData efcard ? efcard.Rarity.ToString() : "terrain";

                    Plugin.Logger.LogInfo($"Dropped card: {card.Title}, Season: {card.Season}, Rarity: {raritystrg}");
                    return card;
                }
            }

            // En cas d'erreur, retourner null
            Plugin.Logger.LogError("Failed to drop a card");
            return null;
        }

        public static WankulCardData randFromPackType(ECollectionPackType packType)
        {
            List<WankulCardData> allCards = WankulCardsData.Instance.cards;

            // Filtrer les cartes déjà associées
            Season season = ConvertPackTypeToSeason(packType);
            List<WankulCardData> seasonalCard =
                allCards.FindAll(card => card.Season == season);

            if (seasonalCard.Count == 0)
            {
                Plugin.Logger.LogError("No available cards to drop");
                return null;
            }

            int randomValue = Random.Range(0, seasonalCard.Count);

            return seasonalCard[randomValue];
        }

        public static void AddCard(WankulCardData wankulCardData, CardData cardData)
        {
            if (!Instance.wankulCards.ContainsKey(wankulCardData.Index))
            {
                Instance.wankulCards[wankulCardData.Index] = (wankulCardData, cardData, 1);
            }
            else
            {
                (WankulCardData, CardData, int) inventoryWankulCard = Instance.wankulCards[wankulCardData.Index];
                inventoryWankulCard.Item3 = inventoryWankulCard.Item3 + 1;
                Instance.wankulCards[wankulCardData.Index] = inventoryWankulCard;
            }
        }
    }
}
