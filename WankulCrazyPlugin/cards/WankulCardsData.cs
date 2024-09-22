using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
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


        // Patch pour la méthode avec un paramètre CardData
        public static void Postfix_GetCardMarketPrice_CardData(CardData cardData, ref float __result)
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            WankulCardData wankulCardData = wankulCardsData.GetFromMonster(cardData, false);

            if (wankulCardData != null)
            {
                __result = wankulCardData.MarketPrice; // Utilise le prix du marché de ta carte
            }
            else
            {
                __result = 0; // Valeur par défaut si la carte n'est pas trouvée
            }
        }

        // Patch pour la méthode avec trois paramètres
        public static void Postfix_GetCardMarketPrice_ThreeParams(ECardExpansionType expansionType, ref float __result)
        {
            float variation = UnityEngine.Random.Range(-0.3f, 0.3f);
            float marketPrice = 20f; // Valeur par défaut

            switch (expansionType)
            {
                case ECardExpansionType.Tetramon:
                    marketPrice = Mathf.Clamp((1 + variation) * 100f, 5f, 100f);
                    break;
                case ECardExpansionType.Destiny:
                    marketPrice = Mathf.Clamp((1 + variation) * 3000f, 100f, 3000f);
                    break;
                case ECardExpansionType.Ghost:
                    marketPrice = Mathf.Clamp((1 + variation) * 5000f, 1000f, 5000f);
                    break;
                default:
                    marketPrice = Mathf.Clamp((1 + variation) * 100f, 5f, 100f);
                    break;
            }

            __result = marketPrice; // Affecte le prix calculé à __result
        }

        public CardData GetUnassciatedCardData()
        {
                foreach (ECardBorderType border in Enum.GetValues(typeof(ECardBorderType)))
                {
                    foreach (ECardExpansionType expansion in Enum.GetValues(typeof(ECardExpansionType)))
                    {
                        if (
                            expansion == ECardExpansionType.None ||
                            expansion == ECardExpansionType.FantasyRPG ||
                            expansion == ECardExpansionType.CatJob ||
                            expansion == ECardExpansionType.Ghost ||
                            expansion == ECardExpansionType.FoodieGO ||
                            expansion == ECardExpansionType.MAX
                            )
                        {
                            continue;
                        }
                        foreach (EMonsterType monster in Enum.GetValues(typeof(EMonsterType)))
                        {
                            if (
                                monster == EMonsterType.EarlyPlayer ||
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
                                Plugin.Logger.LogInfo($"GetUnassciatedCardData : {key}");
                                return cardData; // Retourne le premier CardData manquant trouvé
                            }
                        }
                    }

                }
            return null; // Si aucune CardData manquante n'est trouvée
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
