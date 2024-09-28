using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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

                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                default:
                    return Season.HS;
            }

        }

        public static WankulCardData DropCard(ECollectionPackType packType, bool isTerrain = false, bool isMinRare = false)
        {
            bool increaseRarity = false;

            if (
                packType == ECollectionPackType.DestinyBasicCardPack ||
                packType == ECollectionPackType.DestinyRareCardPack ||
                packType == ECollectionPackType.DestinyEpicCardPack ||
                packType == ECollectionPackType.DestinyLegendaryCardPack
            )
            {
                increaseRarity = true;
            }

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

                List<WankulCardData> specialCardsData = seasonalCard
                    .FindAll(card => card is SpecialCardData);

                seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.R)
                    .ConvertAll(card => (WankulCardData)card);
                ;

                seasonalCard = [.. seasonalCard, .. specialCardsData];
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
                float increasefactor = increaseRarity ? 5f : 1f;
                totalDropChance += card.Drop * increasefactor;
            }

            float randomValue = Random.Range(0f, totalDropChance);
            float cumulativeDropChance = 0f;

            foreach (var card in seasonalCard)
            {
                cumulativeDropChance += card.Drop;
                if (randomValue <= cumulativeDropChance)
                {
                    string raritystrg = card is EffigyCardData efcard ? efcard.Rarity.ToString() : card is SpecialCardData ? "SPECIAL" : card is TerrainCardData ? "terrain" : "Erreur";

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

        public static void AddCard(WankulCardData wankulCardData, CardData cardData, int amount)
        {
            if (!Instance.wankulCards.ContainsKey(wankulCardData.Index))
            {
                Instance.wankulCards[wankulCardData.Index] = (wankulCardData, cardData, amount);
            }
            else
            {
                (WankulCardData, CardData, int) inventoryWankulCard = Instance.wankulCards[wankulCardData.Index];
                inventoryWankulCard.Item3 = inventoryWankulCard.Item3 + amount;
                Instance.wankulCards[wankulCardData.Index] = inventoryWankulCard;
            }
        }

        public static void RemoveCard(WankulCardData wankulCardData, int amount)
        {
            if (Instance.wankulCards.ContainsKey(wankulCardData.Index))
            {
                (WankulCardData, CardData, int) inventoryWankulCard = Instance.wankulCards[wankulCardData.Index];
                inventoryWankulCard.Item3 = inventoryWankulCard.Item3 - amount;
                if (inventoryWankulCard.Item3 <= 0)
                {
                    Instance.wankulCards.Remove(wankulCardData.Index);
                }
                else
                {
                    Instance.wankulCards[wankulCardData.Index] = inventoryWankulCard;
                }
            }
        }

        public static float GetTotalPrice()
        {
            float totalPrice = 0f;
            foreach (var card in Instance.wankulCards)
            {
                totalPrice += card.Value.Item1.MarketPrice * card.Value.Item3;
            }
            return totalPrice;
        }
    }
}
