using System;
using System.Collections.Generic;
using System.Text;
using WankulCrazyPlugin.cards;
using Newtonsoft.Json;
using UnityEngine;

namespace WankulCrazyPlugin.utils
{
    public class SavesManager
    {
        public static void SaveCardsAssociations()
        {
            Dictionary<string, WankulCardData> associations = WankulCardsData.Instance.association;
            string pluginPath = Application.dataPath + "/../BepInEx/plugins";

            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;

            Plugin.Logger.LogInfo("Save index: " + saveIndex);

            string path = pluginPath + "/data/save_" + saveIndex + ".json";

            Dictionary<string, int> knewAssociations = new Dictionary<string, int>();
            foreach (var assocation in associations)
            {
                knewAssociations.Add(assocation.Key, assocation.Value.Index);
            }
            string json = JsonConvert.SerializeObject(knewAssociations);
            System.IO.File.WriteAllText(path, json);
        }

        public static void LoadCardsAssociations()
        {
            string pluginPath = Application.dataPath + "/../BepInEx/plugins";

            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;

            Plugin.Logger.LogInfo("Save index: " + saveIndex);

            string path = pluginPath + "/data/save_" + saveIndex + ".json";

            if (!System.IO.File.Exists(path))
            {
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            Dictionary<string, int> associations = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

            foreach (var association in associations)
            {
                WankulCardData card = WankulCardsData.Instance.cards.Find(card => card.Index == association.Value);
                WankulCardsData.Instance.association[association.Key] = card;
            }
        }
    }
}
