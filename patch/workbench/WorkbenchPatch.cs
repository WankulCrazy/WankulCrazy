using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.patch.workbench
{
    public class WorkbenchPatch
    {
        public static int currentExpensionIndex = 0;

        public static void OpenExpansionScreen(ECardExpansionType initCardExpansion)
        {
            string path = GetGameObjectPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp);
            Plugin.Logger.LogInfo($"OpenScreen : {path}");


            Transform tetramonButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Tetramon_Button");
            if (tetramonButton != null) {
                Plugin.Logger.LogInfo("Tetramon_Button found");
                tetramonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Origins";
            }
            Transform destinyButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Destiny_Button");
            if (destinyButton != null)
            {
                Plugin.Logger.LogInfo("Destiny_Button found");
                destinyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Campus";
            }
            Transform ghostButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Ghost_Button");
            if (ghostButton != null)
            {
                Plugin.Logger.LogInfo("Ghost_Button found");
                ghostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Battle";
            }
        }

        public static void OnExpansionPressButton(int index)
        {
            currentExpensionIndex = index;
            Plugin.Logger.LogInfo($"OnPressButton {index}");
        }

        public static void OnCardExpansionUpdated(WorkbenchUIScreen __instance)
        {
            __instance.m_CardExpansionText.text = SeasonsContainer.Seasons[(Season)currentExpensionIndex];
        }


        public static void OpenWorkBenchScreen()
        {
            WorkbenchUIScreen.Instance.m_CardExpansionText.text = SeasonsContainer.Seasons[(Season)currentExpensionIndex];
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
