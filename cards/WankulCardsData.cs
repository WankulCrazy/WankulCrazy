using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.inventory;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.cards
{
    public class WankulCardsData : Singleton<WankulCardsData>
    {
        public List<WankulCardData> cards = [];
        public Dictionary<string, WankulCardData> association = [];


        public WankulCardData GetFromMonster(CardData monster, bool allowNull)
        {
            ECardExpansionType expansionType = monster.expansionType;
            MonsterData monsterData = InventoryBase.GetMonsterData(monster.monsterType);
            if (monsterData == null)
            {
                return null;
            }
            ERarity rarity = monsterData.Rarity;

            string key = $"{monster.monsterType}_{monster.borderType}_{expansionType}";

            // Vérification de l'association déjà existante
            if (association.TryGetValue(key, out WankulCardData card))
            {
                return card;
            }
            else if (allowNull)
            {
                // dans les drop on peut avoir des cartes qui ne sont pas dans l'association
                return null;
            }

            // Si pas trouvé dans l'association, déterminer le pack de carte
            ECollectionPackType packType = ECollectionPackType.BasicCardPack;

            switch (expansionType)
            {
                case ECardExpansionType.Tetramon:
                    packType = rarity switch
                    {
                        ERarity.Common => ECollectionPackType.BasicCardPack,
                        ERarity.Rare => ECollectionPackType.RareCardPack,
                        ERarity.Epic => ECollectionPackType.EpicCardPack,
                        ERarity.Legendary => ECollectionPackType.LegendaryCardPack,
                        _ => packType
                    };
                    break;

                case ECardExpansionType.Destiny:
                    packType = rarity switch
                    {
                        ERarity.Common => ECollectionPackType.DestinyBasicCardPack,
                        ERarity.Rare => ECollectionPackType.DestinyRareCardPack,
                        ERarity.Epic => ECollectionPackType.DestinyEpicCardPack,
                        ERarity.Legendary => ECollectionPackType.DestinyLegendaryCardPack,
                        _ => packType
                    };
                    break;
                    // Ajouter d'autres types d'extensions ici si nécessaire
            }

            // Sélection aléatoire d'une carte si elle n'a pas été trouvée dans l'association
            WankulCardData wankulCardData = WankulInventory.randFromPackType(packType);

            // On ne stocke pas dans l'association pour de futures drops
            if (wankulCardData != null)
            {
                association[key] = wankulCardData;
            }

            return wankulCardData;
        }

        public CardData GetCardDataFromWankulCardData(WankulCardData card)
        {
            foreach (var entry in association)
            {
                if (entry.Value.Index == card.Index)
                {
                    return GetCardDataFromKey(entry.Key);
                }
            }
            return null;
        }

        public CardData GetCardDataFromKey(string key)
        {
            // Découper la clé en utilisant l'underscore comme séparateur
            string[] parts = key.Split('_');

            if (parts.Length != 3)
            {
                Debug.LogError("La clé ne contient pas le bon nombre de parties.");
                return null;
            }

            // Extraire les valeurs
            EMonsterType monsterType = (EMonsterType)Enum.Parse(typeof(EMonsterType), parts[0]);
            ECardBorderType borderType = (ECardBorderType)Enum.Parse(typeof(ECardBorderType), parts[1]);
            ECardExpansionType expansionType = (ECardExpansionType)Enum.Parse(typeof(ECardExpansionType), parts[2]);

            // Récupérer les données du monstre
            CardData cardData = new CardData();
            cardData.monsterType = monsterType;
            cardData.borderType = borderType;
            cardData.expansionType = expansionType;

            return cardData;
        }

        public void SetFromMonster(CardData monster, WankulCardData card)
        {
            ECardExpansionType expansionType = monster.expansionType;
            MonsterData monsterData = InventoryBase.GetMonsterData(monster.monsterType);

            string key = monster.monsterType.ToString() + "_" + monster.borderType.ToString() + "_" + expansionType.ToString();
            // Vérifiez si la clé existe déjà
            if (!association.ContainsKey(key))
            {
                association[key] = card;  // Créez une nouvelle association
            }
            else
            {
                Debug.LogError("La carte existe déjà dans l'association.");
            }
        }

        public CardData GetUnassciatedCardData()
        {
                
                foreach (ECardExpansionType expansion in Enum.GetValues(typeof(ECardExpansionType)))
                {
                    if (
                                expansion == ECardExpansionType.None ||
                                expansion == ECardExpansionType.FantasyRPG ||
                                expansion == ECardExpansionType.Megabot ||
                                expansion == ECardExpansionType.CatJob ||
                                expansion == ECardExpansionType.Ghost ||
                                expansion == ECardExpansionType.FoodieGO ||
                                expansion == ECardExpansionType.MAX
                        )
                    {
                        continue;
                    }
                    foreach (ECardBorderType border in Enum.GetValues(typeof(ECardBorderType)))
                    {
                        foreach (EMonsterType monster in Enum.GetValues(typeof(EMonsterType)))
                        {
                            if (
                                monster == EMonsterType.EarlyPlayer ||
                                monster == EMonsterType.START_CATJOB ||
                                monster == EMonsterType.START_FANTASYRPG ||
                                monster == EMonsterType.START_MEGABOT ||
                                monster == EMonsterType.None ||
                                monster == EMonsterType.MAX ||
                                monster == EMonsterType.Max ||
                                monster == EMonsterType.MaxArt ||
                                monster == EMonsterType.MAX_CATJOB ||
                                monster == EMonsterType.MAX_FANTASYRPG ||
                                monster == EMonsterType.MAX_MEGABOT
                                )
                            {
                                continue;
                            }
                            // Récupérer les données du monstre
                            MonsterData monsterData = InventoryBase.GetMonsterData(monster);

                            CardData cardData = new CardData();
                            cardData.borderType = border;
                            cardData.expansionType = expansion;
                            cardData.monsterType = monster;

                            string key = $"{cardData.monsterType.ToString()}_{cardData.borderType.ToString()}_{cardData.expansionType.ToString()}";
                            if (!association.ContainsKey(key))
                            {
                                return cardData; // Retourne le premier CardData manquant trouvé
                            }
                        }
                    }

                }
            return null; // Si aucune CardData manquante n'est trouvée
        }

        public static int GetExperienceFromWankulCard(WankulCardData wankulCardData)
        {
            // Base XP multiplier based on card type and rarity
            float experienceFloat = 1;

            // Handle different card types and their XP multipliers
            if (wankulCardData is TerrainCardData)
            {
                // Terrain cards have slightly higher XP
                experienceFloat = 1.5f;
            }
            else if (wankulCardData is EffigyCardData effigyCardData)
            {
                // Significant XP boost based on rarity
                switch (effigyCardData.Rarity)
                {
                    case Rarity.C:
                        experienceFloat = 1.0f;  // Common cards - baseline XP
                        break;
                    case Rarity.UC:
                        experienceFloat = 2.0f;  // Uncommon - much higher than common
                        break;
                    case Rarity.R:
                        experienceFloat = 4.0f;  // Rare cards - significantly boosted
                        break;
                    case Rarity.UR1:
                        experienceFloat = 7.0f;  // Ultra Rare 1 - large XP boost
                        break;
                    case Rarity.UR2:
                        experienceFloat = 10.0f;  // Ultra Rare 2 - even larger boost
                        break;
                    case Rarity.LB:
                        experienceFloat = 15.0f;  // Legendary B - very high XP
                        break;
                    case Rarity.LA:
                        experienceFloat = 20.0f;  // Legendary A - extreme XP gain
                        break;
                    case Rarity.LO:
                        experienceFloat = 25.0f;  // Legendary O - maximum XP for this category
                        break;
                    // Special event cards with massive XP boosts
                    case Rarity.PGW23:
                    case Rarity.NOEL23:
                    case Rarity.SPCIV:
                    case Rarity.SPLEG:
                    case Rarity.ED:
                    case Rarity.SPPOP:
                    case Rarity.GP:
                    case Rarity.SPTV:
                    case Rarity.SPJV:
                    case Rarity.EG:
                    case Rarity.SPCAR:
                    case Rarity.TOR:
                        experienceFloat = 50.0f;  // Special event cards with huge XP
                        break;
                    default:
                        experienceFloat = 1.0f;  // Default XP for unrecognized rarities
                        break;
                }
            }
            else if (wankulCardData is SpecialCardData)
            {
                // Special cards give a massive amount of XP
                experienceFloat = 100f;
            }

            // Factor of XP based on the shop's level (dynamically adjusted)
            int shopLevel = CPlayerData.m_ShopLevel;

            // New formula to calculate XP factor based on the shop level
            float shopXpFactor = 1f + shopLevel + (shopLevel * shopLevel * 0.001f);

            // Ensuring a minimum shop factor of 1
            if (shopXpFactor < 1)
            {
                shopXpFactor = 1;
            }

            // Logging for debugging purposes
            Plugin.Logger.LogInfo($"Experience before shop factor: {experienceFloat}");
            Plugin.Logger.LogInfo($"shop factor: {shopXpFactor}");

            // Final experience calculation
            int experience = Mathf.CeilToInt(experienceFloat * shopXpFactor);

            Plugin.Logger.LogInfo($"Experience after shop factor: {experience}");

            return experience;
        }


        public static int GetTotalCardsCount()
        {
            return Instance.cards.Count;
        }

        public static List<WankulCardData> GetCardsFromSeason(Season season)
        {
            return Instance.cards.FindAll(wankulCard => wankulCard.Season == season);
        }

        public static WankulCardData GetAJETER()
        {
            return WankulCardsData.Instance.cards.Find(wankulCard => wankulCard is SpecialCardData special && special.Special == Specials.AJETER);
        }

        public void DebugDisplayAllCardsAssociations()
        {
            Dictionary<string, WankulCardData> associations = WankulCardsData.Instance.association;

            Dictionary<string, string> knewAssociations = new Dictionary<string, string>();
            foreach (var association in associations)
            {
                knewAssociations.Add(association.Key, association.Value.Title);
            }
            string json = JsonConvert.SerializeObject(knewAssociations);
        }
    }
}
