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

        public static WankulCardData DropCard(ECollectionPackType packType, HashSet<WankulCardData> alreadySelectedCards, bool isTerrain = false, bool isMinRare = false)
        {
            bool increaseRarity = false;
            Season season = ConvertPackTypeToSeason(packType);

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

            List<WankulCardData> seasonalCard;

            if (season != Season.HS)
            {
                seasonalCard = allCards.FindAll(card => card.Season == season);
            }
            else
            {
                seasonalCard = allCards;
            }

            if (!isTerrain && isMinRare)
            {
                List<EffigyCardData> effigyCardsData = seasonalCard
                    .FindAll(card => card is EffigyCardData)
                    .ConvertAll(card => (EffigyCardData)card);

                List<WankulCardData> specialCardsData = seasonalCard
                    .FindAll(card => card is SpecialCardData);

                seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.R)
                    .ConvertAll(card => (WankulCardData)card);

                seasonalCard.AddRange(specialCardsData);
            }
            else if (!isTerrain && !isMinRare)
            {
                seasonalCard = seasonalCard.FindAll(card =>
                    !(card is EffigyCardData effigyCard && effigyCard.Rarity >= Rarity.R)
                );
            }

            if (seasonalCard.Count == 0)
            {
                Plugin.Logger.LogError("No available cards to drop");
                return null;
            }

            // Filtrer les cartes déjà sélectionnées pour éviter les doublons
            seasonalCard = seasonalCard.Where(card => !alreadySelectedCards.Contains(card)).ToList();

            if (seasonalCard.Count == 0)
            {
                Plugin.Logger.LogError("No available unique cards to drop");
                return null;
            }

            float totalDropChance = 0f;
            foreach (var card in seasonalCard)
            {
                float increaseFactor = 1f;
                if (increaseRarity)
                {
                    if (card is EffigyCardData effigyCard)
                    {
                        switch (effigyCard.Rarity)
                        {
                            case Rarity.R:
                                increaseFactor = 1.5f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 5f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 10f;
                                break;
                            default:
                                increaseFactor = 1f;
                                break;
                        }
                    }
                }
                if (season == Season.HS)
                {
                    if (card is EffigyCardData effigyCard)
                    {
                        if (effigyCard.Rarity >= Rarity.PGW23)
                        {
                            increaseFactor = 10;
                        }
                    }
                }
                totalDropChance += card.Drop * increaseFactor;
            }

            float randomValue = Random.Range(0f, totalDropChance);
            float cumulativeDropChance = 0f;

            foreach (var card in seasonalCard)
            {
                float increaseFactor = 1f;
                if (increaseRarity)
                {
                    if (card is EffigyCardData effigyCard)
                    {
                        switch (effigyCard.Rarity)
                        {
                            case Rarity.R:
                                increaseFactor = 1.5f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 5f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 10f;
                                break;
                            default:
                                increaseFactor = 1f;
                                break;
                        }
                    }
                }
                if (season == Season.HS)
                {
                    if (card is EffigyCardData effigyCard)
                    {
                        if (effigyCard.Rarity >= Rarity.PGW23)
                        {
                            increaseFactor = 10;
                        }
                    }
                }

                cumulativeDropChance += card.Drop * increaseFactor;
                if (randomValue <= cumulativeDropChance)
                {
                    // Ajouter la carte sélectionnée aux cartes déjà sélectionnées pour éviter un doublon
                    alreadySelectedCards.Add(card);
                    return card;
                }
            }

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

        public static Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> GetCardsBySeason(Season season)
        {
            return Instance.wankulCards.Where(card => card.Value.wankulcard.Season == season).ToDictionary(card => card.Key, card => card.Value);
        }

        public static float GetMaxPrice()
        {
            float maxPrice = 0f;
            foreach (var card in Instance.wankulCards)
            {
                if (card.Value.Item1.MarketPrice > maxPrice) {
                    maxPrice = card.Value.wankulcard.MarketPrice;
                }
            }
            return maxPrice;
        }

        public static float GetAveragePrice()
        {
            float totalPrice = 0f;
            foreach (var card in Instance.wankulCards)
            {
                totalPrice += card.Value.Item1.MarketPrice;
            }
            return totalPrice / Instance.wankulCards.Count;
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
