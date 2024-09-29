using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace WankulCrazyPlugin.patch
{
    internal class ExpansionScreen
    {
        public static int currentExpensionIndex = 0;
        public static void OpenExpansionScreen(ECardExpansionType initCardExpansion)
        {
            Transform tetramonButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Tetramon_Button");
            if (tetramonButton != null)
            {
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
