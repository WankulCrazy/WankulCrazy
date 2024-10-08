using System.Collections.Generic;
using WankulCrazyPlugin.cards;
using Newtonsoft.Json;
using WankulCrazyPlugin.inventory;
using System.Reflection;
using System;
using System.Security.Cryptography;
using System.Transactions;
using WankulCrazyPlugin.patch;

namespace WankulCrazyPlugin.utils
{
    public class Save
    {
        public Dictionary<string, (int WankulCardIndex, List<float> pastPercent)> associationsWithPercents = [];
        public Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = [];
        public bool savedebug = false;
        public Save(Dictionary<string, (int WankulCardIndex, List<float> pastPercent)> associationsWithPercents, Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards, bool savedebug = false)
        {
            this.associationsWithPercents = associationsWithPercents;
            this.wankulCards = wankulCards;
            this.savedebug = savedebug;
        }
    }
    public class SavesManager
    {
        public static bool DebuggingSave = false;
        public static void SaveCardsAssociations()
        {
            Plugin.Logger.LogInfo("Saving cards associations");
            Dictionary<string, WankulCardData> associations = WankulCardsData.Instance.association;
            string pluginPath = Plugin.GetPluginPath();

            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;

            Plugin.Logger.LogInfo("Save index: " + saveIndex);

            string path = pluginPath + "/data/save_" + saveIndex + ".json";

            Dictionary<string, (int WankulCardIndex, List<float> pastPercent)> knewAssociations = new Dictionary<string, (int WankulCardIndex, List<float> pastPercent)>();
            foreach (var association in associations)
            {
                Plugin.Logger.LogInfo("Saving association: " + association.Key + " => " + association.Value.Index);
                knewAssociations.Add(association.Key, (association.Value.Index, association.Value.PastPercent));
            }

            Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = [];
            foreach (var item in WankulInventory.Instance.wankulCards)
            {
                string cardkey = $"{item.Value.card.monsterType}_{item.Value.card.borderType}_{item.Value.card.expansionType}";
                wankulCards[item.Key] = (item.Value.wankulcard.Index, cardkey, item.Value.amount);
            }

            Save save = new(knewAssociations, wankulCards, false);

            string json = JsonConvert.SerializeObject(save);
            System.IO.File.WriteAllText(path, json);
        }

        public static void LoadCardsAssociations()
        {
            SortUI.inited = false;

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

            bool savedebug = save.savedebug;
            Plugin.Logger.LogInfo("savedebug: " + savedebug);
            if (savedebug)
            { // MODE SANS ECHEC
                DebuggingSave = true;
                Plugin.Logger.LogWarning("No associations found in save file");
                Plugin.Logger.LogInfo("Clearing save");
                CPlayerData.m_CardCollectedList.Clear();
                CPlayerData.m_CardCollectedListDestiny.Clear();
                CPlayerData.m_CardCollectedListGhost.Clear();
                CPlayerData.m_CardCollectedListGhostBlack.Clear();
                CPlayerData.m_CardCollectedListMegabot.Clear();
                CPlayerData.m_CardCollectedListFantasyRPG.Clear();
                CPlayerData.m_CardCollectedListCatJob.Clear();
                Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCardsToDebug = save.wankulCards;
                List<CardData> debugCardsData = [];
                int debugIndex = 0;
                foreach (var item in wankulCardsToDebug)
                {
                    if (debugCardsData.Count == 0 || debugIndex == debugCardsData.Count - 1)
                    {
                        debugCardsData = debubPackContent();
                        debugIndex = 0;
                    }
                    Plugin.Logger.LogInfo($"debugImdex: {debugIndex}");
                    Plugin.Logger.LogInfo($"debugCardsData Count: {debugCardsData.Count}");
                    WankulCardData wankulCardData = WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                    CardData cardData = debugCardsData[debugIndex];
                    Plugin.Logger.LogInfo($"DEBUG Adding card: {cardData.monsterType}_{cardData.borderType}_{cardData.expansionType} for {wankulCardData.Title} => {item.Value.amount}");
                    WankulCardsData.Instance.association[cardData.monsterType + "_" + cardData.borderType + "_" + cardData.expansionType] = wankulCardData;
                    WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
                    debugIndex++;
                }
                DebuggingSave = false;
                return;
            }

            Dictionary<string, (int WankulCardIndex, List<float> pastPercent)> associationsWithPercents = save.associationsWithPercents;
            foreach (var associationWithPercents in associationsWithPercents)
            {
                WankulCardData card = WankulCardsData.Instance.cards.Find(card => card.Index == associationWithPercents.Value.WankulCardIndex);
                card.PastPercent = associationWithPercents.Value.pastPercent;
                WankulCardsData.Instance.association[associationWithPercents.Key] = card;
            }

            Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = save.wankulCards;
            foreach (var item in wankulCards)
            {
                WankulCardData wankulCardData =WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                CardData cardData = WankulCardsData.Instance.GetCardDataFromKey(item.Value.cardkey);

                WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
            }
        }

        public static List<CardData> debubPackContent()
        {
            Type type = typeof(CardOpeningSequence);
            MethodInfo methodInfo = type.GetMethod("GetPackContent", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo rolledCardDataList = type.GetField("m_RolledCardDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo cardValueList = type.GetField("m_CardValueList", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo collectionPackType = type.GetField("m_CollectionPackType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (collectionPackType != null)
            {
                Plugin.Logger.LogInfo("Setting m_CollectionPackType");
                collectionPackType.SetValue(CardOpeningSequence.Instance, ECollectionPackType.BasicCardPack);
                Plugin.Logger.LogInfo("Set m_CollectionPackType");
            }
            else
            {
                Plugin.Logger.LogError($"Le champ m_CollectionPackType n'a pas été trouvé.");
            }

            if (methodInfo != null)
            {
                Plugin.Logger.LogInfo("Calling GetPackContent");
                methodInfo.Invoke(CardOpeningSequence.Instance, new object[] { true, false, false, ECollectionPackType.BasicCardPack });
                Plugin.Logger.LogInfo("GetPackContent called");
            }
            else
            {
                Plugin.Logger.LogError($"Le champ GetPackContent n'a pas été trouvé.");
            }

            if ( rolledCardDataList != null ) {
                Plugin.Logger.LogInfo("Getting m_RolledCardDataList");
                List<CardData> m_RolledCardDataList = (List<CardData>)rolledCardDataList.GetValue(CardOpeningSequence.Instance);
                Plugin.Logger.LogInfo($"Get m_RolledCardDataList count: {m_RolledCardDataList.Count}");
                return m_RolledCardDataList;
            }
            else
            {
                Plugin.Logger.LogError($"Le champ m_RolledCardDataList n'a pas été trouvé.");
            }

            return null;
        }
    }
}
