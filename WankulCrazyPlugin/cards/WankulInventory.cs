using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.cards
{
    public class WankulInventory : Singleton<WankulInventory>
    {
        List<WankulCardData> cards = [];

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
        
        public static WankulCardData DropCard(ECollectionPackType packType)
        {
            List<WankulCardData> allCards = WankulCardsData.Instance.cards;
            Dictionary<string, WankulCardData> associatedCards = WankulCardsData.Instance.association;

            // Filtrer les cartes déjà associées
            List<WankulCardData> availableCards = allCards.FindAll(card => !associatedCards.ContainsValue(card));
            List<WankulCardData> seasonalCard =
                availableCards.FindAll(card => card.Season == ConvertPackTypeToSeason(packType));

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
                    Plugin.Logger.LogInfo($"Dropped card: {card.Title} from season {card.Season}");
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
            Dictionary<string, WankulCardData> associatedCards = WankulCardsData.Instance.association;

            // Filtrer les cartes déjà associées
            List<WankulCardData> availableCards = allCards.FindAll(card => !associatedCards.ContainsValue(card));
            List<WankulCardData> seasonalCard =
                availableCards.FindAll(card => card.Season == ConvertPackTypeToSeason(packType));

            int randomValue = UnityEngine.Random.Range(0, availableCards.Count);
            
            return availableCards[randomValue];
        }

        public static void AddCard(WankulCardData card)
        {
            Instance.cards.Add(card);
        }

        public static void CardOpening(List<CardData> ___m_RolledCardDataList, ECollectionPackType ___m_CollectionPackType)
        {
            Plugin.Logger.LogInfo("You Draw card from a type" + ___m_CollectionPackType.ToString());
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            Plugin.Logger.LogInfo("CardOpening");
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                CardData inGameCard = ___m_RolledCardDataList[i];
                WankulCardData card = wankulCardsData.GetFromMonster(inGameCard);
                if (card == null)
                {
                    card = DropCard(___m_CollectionPackType);
                    if (card != null)
                    {
                        wankulCardsData.SetFromMonster(inGameCard, card);
                        Plugin.Logger.LogInfo(card.Season);
                    }
                }


                if (card != null)
                {
                    Plugin.Logger.LogInfo("Card dropped : " + card.Title + " for : " + inGameCard.monsterType);
                    AddCard(card);
                } else
                {
                    Plugin.Logger.LogError("Failed to drop a card");
                }
            }
        }
    }
}
