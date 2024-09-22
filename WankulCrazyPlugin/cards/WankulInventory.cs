using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.cards
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

            float randomValue = UnityEngine.Random.Range(0f, totalDropChance);
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

            int randomValue = UnityEngine.Random.Range(0, seasonalCard.Count);

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

        public static void CardOpening(List<CardData> ___m_RolledCardDataList, List<float> ___m_CardValueList, ECollectionPackType ___m_CollectionPackType)
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            ___m_CardValueList.Clear();
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                bool isTerrain = i == 0;
                bool isMinRare = i == ___m_RolledCardDataList.Count - 1;
                CardData inGameCard = ___m_RolledCardDataList[i];
                WankulCardData wankulCard = DropCard(___m_CollectionPackType, isTerrain, isMinRare);
                CardData associatedCard = wankulCardsData.GetCardDataFromWankulCardData(wankulCard);

                if (associatedCard != null)
                {
                    inGameCard = wankulCardsData.GetUnassciatedCardData();
                    ___m_RolledCardDataList[i] = inGameCard;
                }

                if (wankulCard != null)
                {
                    inGameCard.isFoil = false;
                    inGameCard.isChampionCard = false;
                    if (wankulCard is EffigyCardData)
                    {
                        EffigyCardData effigyCard = (EffigyCardData)wankulCard;

                        if (effigyCard.Rarity >= Rarity.UR1)
                        {
                            inGameCard.isFoil = true;
                        }
                    }

                    ECardExpansionType expansionType = inGameCard.expansionType;
                    MonsterData monsterData = InventoryBase.GetMonsterData(inGameCard.monsterType);

                    string key = inGameCard.monsterType.ToString() + "_" + inGameCard.borderType.ToString() + "_" + expansionType.ToString();
                    wankulCardsData.SetFromMonster(inGameCard, wankulCard);
                }
                else
                {
                    Plugin.Logger.LogError("Failed to drop a card");
                }

                if (wankulCard != null)
                {
                    AddCard(wankulCard, inGameCard);

                    ___m_CardValueList.Add(wankulCard.MarketPrice);
                }
                else
                {
                    Plugin.Logger.LogError("Failed to drop a card");
                }
            }
        }
    }
}
