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

    public class OldSave
    {
        public Dictionary<string, int> associations;
        public Dictionary<int, (int WankulCardIndex, string cardkey, int amount, List<float> pastPrices)> wankulCards;
        public bool savedebug = false;
    }
    public class Save
    {
        public Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)> associationsWithPercents;
        public Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards;
        public bool savedebug = false;

        // Constructeur pour la nouvelle version
        public Save(Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)> associationsWithPercents, Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards, bool savedebug = false)
        {
            this.associationsWithPercents = associationsWithPercents;
            this.wankulCards = wankulCards;
            this.savedebug = savedebug;
        }

        // Constructeur pour l'ancienne version
        public Save(Dictionary<string, int> associations, Dictionary<int, (int WankulCardIndex, string cardkey, int amount, List<float> pastPrices)> oldWankulCards, bool savedebug = false)
        {
            this.associationsWithPercents = new Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)>();
            foreach (var association in associations)
            {
                this.associationsWithPercents[association.Key] = (association.Value, new List<float>(), 0f);
            }

            this.wankulCards = new Dictionary<int, (int WankulCardIndex, string cardkey, int amount)>();
            foreach (var card in oldWankulCards)
            {
                this.wankulCards[card.Key] = (card.Value.WankulCardIndex, card.Value.cardkey, card.Value.amount);
            }

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

            Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)> knewAssociations = new Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)>();
            foreach (var association in associations)
            {
                Plugin.Logger.LogInfo("Saving association: " + association.Key + " => " + association.Value.Index);
                knewAssociations.Add(association.Key, (association.Value.Index, association.Value.PastPercent, association.Value.generatedMarketPrice));
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
            Save save = null;

            try
            {
                save = JsonConvert.DeserializeObject<Save>(json);
            }
            catch (JsonSerializationException)
            {
                // Tentative de désérialisation de l'ancienne version
                var oldSave = JsonConvert.DeserializeObject<OldSave>(json);
                save = new Save(oldSave.associations, oldSave.wankulCards, oldSave.savedebug);
            }

            if (save == null)
            {
                Plugin.Logger.LogError("Failed to deserialize save file.");
                return;
            }

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
                List<CardData> debugCardsData = new List<CardData>();
                int debugIndex = 0;
                foreach (var item in wankulCardsToDebug)
                {
                    if (debugCardsData.Count == 0 || debugIndex >= debugCardsData.Count)
                    {
                        debugCardsData = debubPackContent();
                        debugIndex = 0;
                    }

                    if (debugIndex < debugCardsData.Count)
                    {
                        Plugin.Logger.LogInfo($"debugIndex: {debugIndex}");
                        Plugin.Logger.LogInfo($"debugCardsData Count: {debugCardsData.Count}");
                        WankulCardData wankulCardData = WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                        if (wankulCardData != null)
                        {
                            CardData cardData = debugCardsData[debugIndex];
                            Plugin.Logger.LogInfo($"DEBUG Adding card: {cardData.monsterType}_{cardData.borderType}_{cardData.expansionType} for {wankulCardData.Title} => {item.Value.amount}");
                            WankulCardsData.Instance.association[cardData.monsterType + "_" + cardData.borderType + "_" + cardData.expansionType] = wankulCardData;
                            WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
                            debugIndex++;
                        }
                        else
                        {
                            Plugin.Logger.LogError($"WankulCardData not found for index: {item.Value.WankulCardIndex}");
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogError("Index out of range while accessing debugCardsData.");
                    }
                }
                DebuggingSave = false;
                return;
            }

            Dictionary<string, (int WankulCardIndex, List<float> pastPercent, float generatedMarketPrice)> associationsWithPercents = save.associationsWithPercents;
            foreach (var associationWithPercents in associationsWithPercents)
            {
                WankulCardData card = WankulCardsData.Instance.cards.Find(card => card.Index == associationWithPercents.Value.WankulCardIndex);
                if (card != null)
                {
                    if (associationWithPercents.Value.pastPercent != null && associationWithPercents.Value.pastPercent.Count > 0)
                    {
                        card.PastPercent = associationWithPercents.Value.pastPercent;
                        card.Percentage = card.PastPercent[card.PastPercent.Count - 1];
                    }
                    if (associationWithPercents.Value.generatedMarketPrice != 0)
                    {
                        card.MarketPrice = associationWithPercents.Value.generatedMarketPrice;
                    }
                    WankulCardsData.Instance.association[associationWithPercents.Key] = card;
                }
                else
                {
                    Plugin.Logger.LogError($"WankulCardData not found for index: {associationWithPercents.Value.WankulCardIndex}");
                }
            }

            Dictionary<int, (int WankulCardIndex, string cardkey, int amount)> wankulCards = save.wankulCards;
            foreach (var item in wankulCards)
            {
                WankulCardData wankulCardData = WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                if (wankulCardData != null)
                {
                    CardData cardData = WankulCardsData.Instance.GetCardDataFromKey(item.Value.cardkey);
                    WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
                }
                else
                {
                    Plugin.Logger.LogError($"WankulCardData not found for index: {item.Value.WankulCardIndex}");
                }
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
