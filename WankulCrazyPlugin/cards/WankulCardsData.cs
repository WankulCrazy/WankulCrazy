using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.cards
{
    public class WankulCardsData : Singleton<WankulCardsData>
    {
        public List<WankulCardData> cards = [];
        public Dictionary<string, WankulCardData> association = [];

        public WankulCardData GetFromMonster(CardData monster)
        {
            ECardExpansionType expansionType = monster.expansionType;
            MonsterData monsterData = InventoryBase.GetMonsterData(monster.monsterType);
            EElementIndex elementIndex = monsterData.ElementIndex;
            ERarity rarity = monsterData.Rarity;

            string key = monster.monsterType.ToString() + "_" + monster.borderType.ToString() + "_" + expansionType.ToString() + "_" + elementIndex.ToString() + "_" + rarity.ToString();
            if (association.TryGetValue(key, out WankulCardData card))
            {
                return card;
            }
            else
            {
                ECollectionPackType packType = ECollectionPackType.BasicCardPack;

                if (expansionType == ECardExpansionType.Tetramon)
                {
                    if (rarity == ERarity.Common)
                    {
                        packType = ECollectionPackType.BasicCardPack;
                    }
                    else if (rarity == ERarity.Rare)
                    {
                        packType = ECollectionPackType.RareCardPack;
                    }
                    else if (rarity == ERarity.Epic)
                    {
                        packType = ECollectionPackType.EpicCardPack;
                    }
                    else if (rarity == ERarity.Legendary)
                    {
                        packType = ECollectionPackType.LegendaryCardPack;
                    }
                }
                else if (expansionType == ECardExpansionType.Destiny)
                {
                    if (rarity == ERarity.Common)
                    {
                        packType = ECollectionPackType.DestinyBasicCardPack;
                    }
                    else if (rarity == ERarity.Rare)
                    {
                        packType = ECollectionPackType.DestinyRareCardPack;
                    }
                    else if (rarity == ERarity.Epic)
                    {
                        packType = ECollectionPackType.DestinyEpicCardPack;
                    }
                    else if (rarity == ERarity.Legendary)
                    {
                        packType = ECollectionPackType.DestinyLegendaryCardPack;
                    }
                }

                Plugin.Logger.LogInfo($"Random card from pack type: {packType}");
                WankulCardData wankulCardData = WankulInventory.randFromPackType(packType);
                association[key] = wankulCardData;

                return wankulCardData;
            }
        }

        public void SetFromMonster(CardData monster, WankulCardData card)
        {
            ECardExpansionType expansionType = monster.expansionType;
            MonsterData monsterData = InventoryBase.GetMonsterData(monster.monsterType);
            EElementIndex elementIndex = monsterData.ElementIndex;
            ERarity rarity = monsterData.Rarity;

            string key = monster.monsterType.ToString() + "_" + monster.borderType.ToString() + "_" + expansionType.ToString() + "_" + elementIndex.ToString() + "_" + rarity.ToString();
            association[key] = card;
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
            Plugin.Logger.LogInfo("DEBUG display all cards association: \n" + json);
        }
    }
}
