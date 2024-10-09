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
        [JsonConstructor]
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
            if (associations != null)
            {
                foreach (var association in associations)
                {
                    this.associationsWithPercents[association.Key] = (association.Value, new List<float>(), 0f);
                }
            }

            this.wankulCards = new Dictionary<int, (int WankulCardIndex, string cardkey, int amount)>();
            if (oldWankulCards != null)
            {
                foreach (var card in oldWankulCards)
                {
                    this.wankulCards[card.Key] = (card.Value.WankulCardIndex, card.Value.cardkey, card.Value.amount);
                }
            }

            this.savedebug = savedebug;
        }
    }
    public class SavesManager
    {
        public static bool DebuggingSave = false;
        public static void ModSave()
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

        public static void ModLoad()
        {
            SortUI.inited = false;

            Plugin.Logger.LogInfo("Loading cards associations");
            string pluginPath = Plugin.GetPluginPath();
            int saveIndex = CGameManager.Instance.m_CurrentSaveLoadSlotSelectedIndex;
            string path = $"{pluginPath}/data/save_{saveIndex}.json";

            Plugin.Logger.LogInfo($"Save index: {saveIndex}");
            Plugin.Logger.LogInfo($"Path: {path}");

            if (!System.IO.File.Exists(path))
            {
                Plugin.Logger.LogError("Save file does not exist.");
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            Save save = DeserializeSave(json);

            if (save.associationsWithPercents == null)
            {
                Plugin.Logger.LogError("Failed to deserialize save file.");
                Plugin.Logger.LogInfo("Trying to deserialize old save file.");
                save = DeserializeOldSave(json);
            }

            if (save == null)
            {
                Plugin.Logger.LogError("Failed to deserialize save file.");
                return;
            }

            Plugin.Logger.LogInfo("Deserialized Save object successfully.");
            Plugin.Logger.LogInfo($"Associations count: {save.associationsWithPercents.Count}");

            if (save.savedebug)
            {
                HandleDebugSave(save);
            }
            else
            {
                LoadAssociations(save);
                LoadWankulCards(save);
            }
        }

        private static Save DeserializeSave(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Save>(json);
            }
            catch (JsonSerializationException ex)
            {
                Plugin.Logger.LogError($"Failed to deserialize Save object: {ex.Message}");
                return DeserializeOldSave(json);
            }
        }

        private static Save DeserializeOldSave(string json)
        {
            try
            {
                var oldSave = JsonConvert.DeserializeObject<OldSave>(json);
                Plugin.Logger.LogInfo("Deserialized OldSave object successfully.");
                return new Save(oldSave.associations, oldSave.wankulCards, oldSave.savedebug);
            }
            catch (JsonSerializationException ex)
            {
                Plugin.Logger.LogError($"Failed to deserialize OldSave object: {ex.Message}");
                return null;
            }
        }

        private static void HandleDebugSave(Save save)
        {
            DebuggingSave = true;
            Plugin.Logger.LogWarning("Debugging Save");
            Plugin.Logger.LogInfo("Clearing save");

            ClearPlayerData();

            var debugCardsData = debubPackContent();
            int debugIndex = 0;

            foreach (var item in save.wankulCards)
            {
                if (debugIndex >= debugCardsData.Count)
                {
                    debugCardsData = debubPackContent();
                    debugIndex = 0;
                }

                AddDebugCard(item, debugCardsData, ref debugIndex);
            }

            DebuggingSave = false;
        }

        private static void ClearPlayerData()
        {
            CPlayerData.m_CardCollectedList.Clear();
            CPlayerData.m_CardCollectedListDestiny.Clear();
            CPlayerData.m_CardCollectedListGhost.Clear();
            CPlayerData.m_CardCollectedListGhostBlack.Clear();
            CPlayerData.m_CardCollectedListMegabot.Clear();
            CPlayerData.m_CardCollectedListFantasyRPG.Clear();
            CPlayerData.m_CardCollectedListCatJob.Clear();
        }

        private static void AddDebugCard(KeyValuePair<int, (int WankulCardIndex, string cardkey, int amount)> item, List<CardData> debugCardsData, ref int debugIndex)
        {
            Plugin.Logger.LogInfo($"debugIndex: {debugIndex}");
            Plugin.Logger.LogInfo($"debugCardsData Count: {debugCardsData.Count}");

            var wankulCardData = WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
            if (wankulCardData != null)
            {
                var cardData = debugCardsData[debugIndex];
                Plugin.Logger.LogInfo($"DEBUG Adding card: {cardData.monsterType}_{cardData.borderType}_{cardData.expansionType} for {wankulCardData.Title} => {item.Value.amount}");
                WankulCardsData.Instance.association[$"{cardData.monsterType}_{cardData.borderType}_{cardData.expansionType}"] = wankulCardData;
                WankulInventory.Instance.wankulCards[item.Key] = (wankulCardData, cardData, item.Value.amount);
                debugIndex++;
            }
            else
            {
                Plugin.Logger.LogError($"WankulCardData not found for index: {item.Value.WankulCardIndex}");
            }
        }

        private static void LoadAssociations(Save save)
        {
            foreach (var association in save.associationsWithPercents)
            {
                var card = WankulCardsData.Instance.cards.Find(c => c.Index == association.Value.WankulCardIndex);
                if (card != null)
                {
                    if (association.Value.pastPercent != null && association.Value.pastPercent.Count > 0)
                    {
                        card.PastPercent = association.Value.pastPercent;
                        card.Percentage = card.PastPercent[^1];
                    }
                    if (association.Value.generatedMarketPrice != 0)
                    {
                        card.MarketPrice = association.Value.generatedMarketPrice;
                    }

                    UpdateCardPriceIfNeeded(card);
                    WankulCardsData.Instance.association[association.Key] = card;
                }
                else
                {
                    Plugin.Logger.LogError($"WankulCardData not found for index: {association.Value.WankulCardIndex}");
                }
            }
        }

        private static void UpdateCardPriceIfNeeded(WankulCardData card)
        {
            int currentDay = CSaveLoad.m_SavedGame.m_CurrentDay + 1;
            Plugin.Logger.LogInfo($"Current day: {currentDay}");
            while (card.PastPercent.Count < currentDay)
            {
                CardPrice.UpdateCardPricePercent(card);
            }
        }

        private static void LoadWankulCards(Save save)
        {
            foreach (var item in save.wankulCards)
            {
                var wankulCardData = WankulCardsData.Instance.cards.Find(card => card.Index == item.Value.WankulCardIndex);
                if (wankulCardData != null)
                {
                    var cardData = WankulCardsData.Instance.GetCardDataFromKey(item.Value.cardkey);
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
