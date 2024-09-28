using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch.workbench
{
    public class WorkbenchPatch
    {
        public static int currentExpensionIndex = 0;
        public static int currentRarityIndex = 0;
        public static Dictionary<int, (string label, List<Rarity> rarities)> rarityGroups = new Dictionary<int, (string label, List<Rarity>)>
        {
            { 0, ("Toute rareté", new List<Rarity>((Rarity[])Enum.GetValues(typeof(Rarity)))) },
            { 1, ("Commune et Peu Commune", new List<Rarity> { Rarity.C, Rarity.UC }) },
            { 2, ("Rare", new List<Rarity> { Rarity.R }) },
            { 3, ("Ultra rare 1/2", new List<Rarity> { Rarity.UR1, Rarity.UR2 }) },
            { 4, ("Légendaire Or/Argent/Bronze", new List<Rarity> { Rarity.LO, Rarity.LA, Rarity.LB }) }
        };

        public static void OpenExpansionScreen(ECardExpansionType initCardExpansion)
        {
            Transform tetramonButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Tetramon_Button");
            if (tetramonButton != null) {
                tetramonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Origins";
            }
            Transform destinyButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Destiny_Button");
            if (destinyButton != null)
            {
                destinyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Campus";
            }
            Transform ghostButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Ghost_Button");
            if (ghostButton != null)
            {
                ghostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Battle";
            }
        }

        public static void OnExpansionPressButton(int index)
        {
            currentExpensionIndex = index;
            Plugin.Logger.LogInfo($"OnExpansionPressButton {index}");
        }


        public static void OpenRarityScreen(ERarity initCardRarity)
        {
            
            Transform anyRarityButton = FindChildByPath(CSingleton<CardRaritySelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/AnyRarity_Button");
            if (anyRarityButton != null)
            {
                anyRarityButton.GetComponentInChildren<TextMeshProUGUI>().text = rarityGroups[0].label;
            }
            Transform commonButton = FindChildByPath(CSingleton<CardRaritySelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Common_Button");
            if (commonButton != null)
            {
                commonButton.GetComponentInChildren<TextMeshProUGUI>().text = rarityGroups[1].label;
            }
            Transform rareButton = FindChildByPath(CSingleton<CardRaritySelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Rare_Button");
            if (rareButton != null)
            {
                rareButton.GetComponentInChildren<TextMeshProUGUI>().text = rarityGroups[2].label;
            }
            Transform epicButton = FindChildByPath(CSingleton<CardRaritySelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Epic_Button");
            if (epicButton != null)
            {
                epicButton.GetComponentInChildren<TextMeshProUGUI>().text = rarityGroups[3].label;
            }
            Transform legendaryButton = FindChildByPath(CSingleton<CardRaritySelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Legendary_Button");
            if (legendaryButton != null)
            {
                legendaryButton.GetComponentInChildren<TextMeshProUGUI>().text = rarityGroups[4].label;
            }

        }

        public static void OnRarityPressButton(int index)
        {
            currentRarityIndex = index + 1;
            Plugin.Logger.LogInfo($"OnRarityPressButton {currentRarityIndex}, label : {rarityGroups[currentRarityIndex].label}");
        }



        public static void OnCardExpansionUpdated(WorkbenchUIScreen __instance)
        {
            __instance.m_CardExpansionText.text = SeasonsContainer.Seasons[(Season)currentExpensionIndex];
        }

        public static void OnRarityLimitUpdated(WorkbenchUIScreen __instance)
        {
            __instance.m_RarityLimitText.text = rarityGroups[currentRarityIndex].label;
        }

        public static void OpenWorkBenchScreen()
        {
            WorkbenchUIScreen.Instance.m_CardExpansionText.text = SeasonsContainer.Seasons[(Season)currentExpensionIndex];
            WorkbenchUIScreen.Instance.m_RarityLimitText.text = rarityGroups[currentRarityIndex].label;
        }

        public static bool RunBundleCardBulkFunction(WorkbenchUIScreen __instance)
        {
            // Utilisation de la réflexion pour récupérer les champs privés
            var currentInteractableWorkbench = (InteractableWorkbench)AccessTools.Field(__instance.GetType(), "m_CurrentInteractableWorkbench").GetValue(__instance);
            var isWorkingOnTask = (bool)AccessTools.Field(__instance.GetType(), "m_IsWorkingOnTask").GetValue(__instance);
            var currentCardExpansionType = (ECardExpansionType)AccessTools.Field(__instance.GetType(), "m_CurrentCardExpansionType").GetValue(__instance);

            // Accès à d'autres champs ou méthodes si nécessaire
            var sliderPriceLimit = (UnityEngine.UI.Slider)AccessTools.Field(__instance.GetType(), "m_SliderPriceLimit").GetValue(__instance);
            var sliderMinCard = (UnityEngine.UI.Slider)AccessTools.Field(__instance.GetType(), "m_SliderMinCard").GetValue(__instance);
            var taskFinishCircleGrp = (UnityEngine.GameObject)AccessTools.Field(__instance.GetType(), "m_TaskFinishCirlceGrp").GetValue(__instance);

            Season[] seasons = (Season[])Enum.GetValues(typeof(Season));
            Season currentSeason = seasons[currentExpensionIndex];
            List<Rarity> currentRarities = rarityGroups[currentRarityIndex].rarities;

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> wankulCards = WankulInventory.GetCardsBySeason(currentSeason);

            Dictionary<int, (EffigyCardData wankulcard, CardData card, int amount)> effigyCards = wankulCards
                .Where(card => (card.Value.wankulcard is EffigyCardData))
                .ToDictionary(card => card.Key, card => (card.Value.wankulcard as EffigyCardData, card.Value.card, card.Value.amount));

            Dictionary<int, (TerrainCardData wankulcard, CardData card, int amount)> terrainCards = wankulCards
                .Where(card => (card.Value.wankulcard is TerrainCardData))
                .ToDictionary(card => card.Key, card => (card.Value.wankulcard as TerrainCardData, card.Value.card, card.Value.amount));

            int terrainAmount = 0;
            int maxTerrainAmount = 1;
            int totalSelectedAmount = 0;
            int maxSelectedAmount = 10;
            int totalEffigyAmount = effigyCards.Sum(card => card.Value.amount);
            int totalTerrainAmount = terrainCards.Sum(card => card.Value.amount);
            int totalCardsAmount = totalEffigyAmount + totalTerrainAmount;

            // Désactivation des sliders avec réflexion
            sliderPriceLimit.interactable = false;
            sliderMinCard.interactable = false;

            Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)> SelectedCards = new Dictionary<int, (WankulCardData wankulcard, CardData card, int amount)>();
            List<CardData> selectedCardsData = new List<CardData>();

            if (totalTerrainAmount < 1 && totalCardsAmount < 10)
            {
                NotEnoughResourceTextPopup.ShowText(ENotEnoughResourceText.NotEnoughCardForBundle);
                return false;
            }

            while (totalSelectedAmount < maxSelectedAmount)
            {
                Plugin.Logger.LogInfo($"totalSelectedAmount : {totalSelectedAmount}, maxSelectedAmount : {maxSelectedAmount}");
                if (terrainAmount < maxTerrainAmount && terrainCards.Count > 0)
                {
                    int randomTerrainIndex = UnityEngine.Random.Range(0, terrainCards.Count);
                    KeyValuePair<int, (TerrainCardData wankulcard, CardData card, int amount)> randomTerrain = terrainCards.ElementAt(randomTerrainIndex);

                    //var updatedCard = randomTerrain.Value;
                    //updatedCard.amount -= 1;
                    //if (updatedCard.amount >= 0)
                    //{
                        //terrainCards[randomTerrain.Key] = updatedCard;
                        selectedCardsData.Add(randomTerrain.Value.card);
                        CPlayerData.ReduceCard(randomTerrain.Value.card, 1);
                        totalSelectedAmount += 1;
                    //}
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, effigyCards.Count);
                    KeyValuePair<int, (EffigyCardData wankulcard, CardData card, int amount)> randomCard = effigyCards.ElementAt(randomIndex);
                    if (currentRarities.Contains(randomCard.Value.wankulcard.Rarity))
                    {
                        //var updatedCard = randomCard.Value;
                        //updatedCard.amount -= 1;
                        //if (updatedCard.amount >= 0)
                        //{
                        //    effigyCards[randomCard.Key] = updatedCard;
                            selectedCardsData.Add(randomCard.Value.card);
                            CPlayerData.ReduceCard(randomCard.Value.card, 1);
                            totalSelectedAmount += 1;
                        //}
                    }
                }
            }

            Plugin.Logger.LogInfo($"Selected cards : {selectedCardsData.Count}, expansion : {currentCardExpansionType}");

            currentInteractableWorkbench.PlayBundlingCardBoxSequence(selectedCardsData, currentCardExpansionType);

            AccessTools.Field(__instance.GetType(), "m_IsWorkingOnTask").SetValue(__instance, true);

            // Activation du GameObject via réflexion
            taskFinishCircleGrp.SetActive(true);

            return false;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform;

            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }

        private static Transform FindChildByPath(Transform parent, string path)
        {
            string[] segments = path.Split('/');
            Transform current = parent;

            foreach (string segment in segments)
            {
                current = current.Find(segment);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
    }
}
