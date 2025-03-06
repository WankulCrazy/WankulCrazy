using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.inventory
{

    public class WankulInventory : Singleton<WankulInventory>
    {
        public Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = [];

        public static Season ConvertPackTypeToSeason(ECollectionPackType packType)
        {
            // Récupère la valeur dynamique de "Stellar"
            ECollectionPackType stellarPack = EnumExtensions.SafeParseECollectionPackType("Stellar");
            ECollectionPackType stellarPackTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");

            if (packType == ECollectionPackType.BasicCardPack || packType == ECollectionPackType.DestinyBasicCardPack)
                return Season.S01;
            else if (packType == ECollectionPackType.RareCardPack || packType == ECollectionPackType.DestinyRareCardPack)
                return Season.S02;
            else if (packType == ECollectionPackType.EpicCardPack || packType == ECollectionPackType.DestinyEpicCardPack)
                return Season.S03;
            else if (packType == stellarPack || packType == stellarPackTaux)
                return Season.S04;
            else
                return Season.HS;
        }

        public static WankulCardData DropCard(ECollectionPackType packType, List<WankulCardData> alreadySelectedCards, bool isTerrain = false, bool isMinRare = false, bool isMinUR = false, bool isMinLegendary = false, bool isRare = false)
        {
            ECollectionPackType stellarPackTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");
            bool increaseRarity = false;
            Season season = ConvertPackTypeToSeason(packType);

            if (
                packType == ECollectionPackType.DestinyBasicCardPack ||
                packType == ECollectionPackType.DestinyRareCardPack ||
                packType == ECollectionPackType.DestinyEpicCardPack ||
                packType == ECollectionPackType.DestinyLegendaryCardPack ||
                packType == stellarPackTaux
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

            if (!isTerrain && (isMinRare || isMinLegendary || isMinUR || isRare))
            {
                List<EffigyCardData> effigyCardsData = seasonalCard
                    .FindAll(card => card is EffigyCardData)
                    .ConvertAll(card => (EffigyCardData)card);

                if (isMinRare)
                {
                    seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.R)
                        .ConvertAll(card => (WankulCardData)card);
                }
                else if (isMinUR)
                {
                    seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.UR1)
                        .ConvertAll(card => (WankulCardData)card);
                }
                else if (isMinLegendary)
                {
                    seasonalCard = effigyCardsData.FindAll(card => card.Rarity >= Rarity.LB)
                        .ConvertAll(card => (WankulCardData)card);
                }

                if (isRare)
                {
                    seasonalCard = effigyCardsData.FindAll(card => card.Rarity == Rarity.R)
                        .ConvertAll(card => (WankulCardData)card);
                }


                List<WankulCardData> specialCardsData = allCards
                .FindAll(card => card is SpecialCardData);
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
                                increaseFactor = 0.25f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 1f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 2f;
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
                            increaseFactor = 2;
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
                                increaseFactor = 0.25f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 1f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 2f;
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
                            increaseFactor = 2;
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

        public static WankulCardData DropCardGold(ECollectionPackType packType, List<WankulCardData> alreadySelectedCards)
        {
            ECollectionPackType stellarPackTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");
            bool increaseRarity = false;
            Season season = ConvertPackTypeToSeason(packType);

            if (
                packType == ECollectionPackType.DestinyBasicCardPack ||
                packType == ECollectionPackType.DestinyRareCardPack ||
                packType == ECollectionPackType.DestinyEpicCardPack ||
                packType == ECollectionPackType.DestinyLegendaryCardPack ||
                packType == stellarPackTaux
            )
            {
                increaseRarity = true;
            }

            List<WankulCardData> allCards = WankulCardsData.Instance.cards;


            allCards = allCards.FindAll(card => card is not TerrainCardData);

            List<WankulCardData> seasonalCard;

            List<int> BattleGoldCards = [
                357,
                358,
                359,
                360,
                361,
                362,
                363,
                364,
            ];

            List<int> StellardGoldCards = [
                734,
                735,
                736,
                737,
                738,
                739,
                740,
                741,
            ];

            if (season == Season.S03)
            {
                seasonalCard = allCards.FindAll(card => BattleGoldCards.Contains(card.Index));
            }
            else if (season == Season.S04) {
                seasonalCard = allCards.FindAll(card => StellardGoldCards.Contains(card.Index));
            }
            else
            {
                Plugin.Logger.LogError("No available cards to drop");
                return null;
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
                                increaseFactor = 0.25f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 1f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 2f;
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
                            increaseFactor = 2;
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
                                increaseFactor = 0.25f;
                                break;
                            case Rarity.UR1:
                            case Rarity.UR2:
                                increaseFactor = 1f;
                                break;
                            case Rarity.LB:
                            case Rarity.LA:
                            case Rarity.LO:
                                increaseFactor = 2f;
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
                            increaseFactor = 2;
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
                if (card.Value.Item1.MarketPrice > maxPrice)
                {
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

        public static float GetTotalPriceBySeason(Season season)
        {
            float totalPrice = 0f;
            foreach (var card in Instance.wankulCards)
            {
                if (card.Value.Item1.Season == season)
                {
                    totalPrice += card.Value.Item1.MarketPrice * card.Value.Item3;
                }
            }
            return totalPrice;
        }

        public static (WankulCardData wankulcard, CardData card, int amount) GetWankulCardFormGameCard(CardData cardData)
        {
            string key = $"{cardData.monsterType}_{cardData.borderType}_{cardData.expansionType}";
            return Instance.wankulCards.Values.FirstOrDefault(card => $"{card.card.monsterType}_{card.card.borderType}_{card.card.expansionType}" == key);
        }

        public static bool isNewWankulCard(WankulCardData wankulCardData)
        {
            Instance.wankulCards.TryGetValue(wankulCardData.Index, out var card);
            if (card.wankulcard == null)
            {
                return true;
            }
            return card.amount == 0;
        }


        public static (WankulCardData wankulcard, CardData card, int amount) GetWankulCardDataForTradeOffer() {
            List<ECollectionPackType> dropableExpansion = [
                ECollectionPackType.BasicCardPack
            ];

            EItemType stellarCardPack = EnumExtensions.SafeParseEItemType("BoosterStellar");
            EItemType stellarCardPackTaux = EnumExtensions.SafeParseEItemType("BoosterStellarTaux");
            ECollectionPackType stellarCardExpansion = EnumExtensions.SafeParseECollectionPackType("Stellar");
            ECollectionPackType stellarCardExpansionTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");

            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.RareCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.RareCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.EpicCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.RareCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(stellarCardPack))
            {
                dropableExpansion.Add(stellarCardExpansion);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.LegendaryCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.LegendaryCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.DestinyBasicCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.DestinyBasicCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.DestinyRareCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.DestinyRareCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.DestinyEpicCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.DestinyEpicCardPack);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(stellarCardPackTaux))
            {
                dropableExpansion.Add(stellarCardExpansionTaux);
            }
            if (CPlayerData.m_ShopLevel >= InventoryBase.GetUnlockItemLevelRequired(EItemType.DestinyLegendaryCardPack))
            {
                dropableExpansion.Add(ECollectionPackType.DestinyLegendaryCardPack);
            }

            ECollectionPackType selectedPackType = dropableExpansion[Random.Range(0, dropableExpansion.Count)];
            bool isTerrain = Random.Range(0, 1) == 1;
            bool isMinRare = Random.Range(0, 1) == 1;
            bool isMinUR = Random.Range(0, 100) < 50;
            bool isMinLegendary = Random.Range(0, 200) < 50;

            WankulCardData wankulCardData = DropCard(selectedPackType, new List<WankulCardData>(), isTerrain, isMinRare, isMinUR, isMinLegendary);

            CardData cardData = WankulCardsData.Instance.GetCardDataFromWankulCardData(wankulCardData);
            if (cardData == null) {
                cardData = WankulCardsData.Instance.GetUnassciatedCardData();
                WankulCardsData.Instance.SetFromMonster(cardData, wankulCardData);
            }
            
            int amount = Instance.wankulCards.ContainsKey(wankulCardData.Index) ? Instance.wankulCards[wankulCardData.Index].amount : 0;

            return (wankulCardData, cardData, amount);
        }

        public static (WankulCardData wankulcard, CardData card, int amount) GetWankulCardDataForTradeOfferByPrice(CardData fromCardData)
        {
            WankulCardData fromWankulCardData = WankulCardsData.Instance.GetFromMonster(fromCardData, true);

            if (fromWankulCardData == null)
            {
                Plugin.Logger.LogError("GetWankulCardDataForTradeOfferByPrice: No wankul card found for this card");
                return GetWankulCardDataForTradeOffer();
            }

            float minFactor = 0.75f;
            float maxFactor = 1.25f;

            float minPrice = fromWankulCardData.MarketPrice * minFactor;
            float maxPrice = fromWankulCardData.MarketPrice * maxFactor;

            List<WankulCardData> inPriceBoundCards = WankulCardsData.Instance.cards.FindAll(card => card.MarketPrice >= minPrice && card.MarketPrice <= maxPrice && card.Index != fromWankulCardData.Index);

            int randomValue = Random.Range(0, inPriceBoundCards.Count);
            WankulCardData wankulCardData = inPriceBoundCards[randomValue];

            int amount = Instance.wankulCards.ContainsKey(wankulCardData.Index) ? Instance.wankulCards[wankulCardData.Index].amount : 0;

            CardData cardData = WankulCardsData.Instance.GetCardDataFromWankulCardData(wankulCardData);
            if (cardData == null)
            {
                cardData = WankulCardsData.Instance.GetUnassciatedCardData();
                WankulCardsData.Instance.SetFromMonster(cardData, wankulCardData);
            }

            return (wankulCardData, cardData, amount);
        }
    }
}
