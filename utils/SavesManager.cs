using System.Collections.Generic;
using WankulCrazyPlugin.cards;
using Newtonsoft.Json;
using UnityEngine;
using WankulCrazyPlugin.inventory;
using System.IO;

namespace WankulCrazyPlugin.utils
{
    public class Save
    {
        public Dictionary<string, int> associations = [];
        public Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = [];
        public Save(Dictionary<string, int> associations, Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards)
        {
            this.associations = associations;
            this.wankulCards = wankulCards;
        }
    }
    public class SavesManager
    {
        public static void SaveCardsAssociations()
        {
            Plugin.Logger.LogInfo("Saving cards associations");
            Dictionary<string, WankulCardData> associations = WankulCardsData.Instance.association;
            string pluginPath = Plugin.GetPluginPath();

            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;

            Plugin.Logger.LogInfo("Save index: " + saveIndex);

            string path = pluginPath + "/data/save_" + saveIndex + ".json";

            Dictionary<string, int> knewAssociations = new Dictionary<string, int>();
            foreach (var association in associations)
            {
                Plugin.Logger.LogInfo("Saving association: " + association.Key + " => " + association.Value.Index);
                knewAssociations.Add(association.Key, association.Value.Index);
            }

            Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = [];
            foreach (var item in WankulInventory.Instance.wankulCards)
            {
                string cardkey = $"{item.Value.card.monsterType}_{item.Value.card.borderType}_{item.Value.card.expansionType}";
                wankulCards[item.Key] = (item.Value.wankulcard.Index, cardkey, item.Value.amount);
            }

            Save save = new(knewAssociations, wankulCards);

            string json = JsonConvert.SerializeObject(save);
            System.IO.File.WriteAllText(path, json);
        }

        public static void LoadCardsAssociations()
        {
            Plugin.Logger.LogInfo("Loading cards associations");
            string pluginPath = Plugin.GetPluginPath();

            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;

            Plugin.Logger.LogInfo("Save index: " + saveIndex);

            string path = pluginPath + "/data/save_" + saveIndex + ".json";

            Plugin.Logger.LogInfo("Path: " + path);

            if (!System.IO.File.Exists(path))
            {
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            Save save = JsonConvert.DeserializeObject<Save>(json);

            Dictionary<string, int> associations = save.associations;
            foreach (var association in associations)
            {
                WankulCardData card = WankulCardsData.Instance.cards.Find(card => card.Index == association.Value);
                WankulCardsData.Instance.association[association.Key] = card;
            }

            Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = save.wankulCards;
            foreach (var item in wankulCards)
            {
                WankulCardData wankulCardData =WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                CardData cardData = WankulCardsData.Instance.GetCardDataFromKey(item.Value.cardkey);

                WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
            }
        }
    }
}
