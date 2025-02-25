using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.patch
{
    internal class ExpansionScreen
    {
        public static bool inited = false;
        public static int currentExpensionIndex = 0;
        public static void OpenExpansionScreen(ECardExpansionType initCardExpansion)
        {
            if (inited)
            {
                for (int i = 0; i < CardExpansionSelectScreen.Instance.m_BtnHighlightList.Count; i++)
                {
                    CardExpansionSelectScreen.Instance.m_BtnHighlightList[i].SetActive(false);
                }
                CardExpansionSelectScreen.Instance.m_BtnHighlightList[currentExpensionIndex].SetActive(true);
                return;
            }

            inited = true;

            Transform tetramonButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Tetramon_Button");
            tetramonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Origins";
            tetramonButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                0,
                170
            );

            Transform destinyButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Destiny_Button");
            destinyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Campus";
            
            Transform ghostButton = FindChildByPath(CSingleton<CardExpansionSelectScreen>.Instance.m_ScreenGrp.transform, "AnimGrp/Mask/UIGroup/Ghost_Button");
            ghostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Battle";

            float verticalSpacing = destinyButton.gameObject.GetComponent<RectTransform>().anchoredPosition.y - ghostButton.gameObject.GetComponent<RectTransform>().anchoredPosition.y - 15;


            destinyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                tetramonButton.GetComponent<RectTransform>().anchoredPosition.x,
                tetramonButton.GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing
            );

            ghostButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                destinyButton.GetComponent<RectTransform>().anchoredPosition.x,
                destinyButton.GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing
            );


            GameObject s04GameObject = GameObject.Instantiate(destinyButton.gameObject);
            s04GameObject.name = "S04_Button";
            s04GameObject.transform.SetParent(tetramonButton.GetParent().transform);
            s04GameObject.transform.localScale = tetramonButton.localScale;
            s04GameObject.transform.localPosition = tetramonButton.localPosition;
            s04GameObject.transform.localRotation = tetramonButton.localRotation;
            s04GameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                ghostButton.GetComponent<RectTransform>().anchoredPosition.x,
                ghostButton.GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing  // Espacement vertical
            );
            s04GameObject.GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S04];
            Button s04Button = Plugin.FindChildByPath(s04GameObject.transform, "AnimGrp/BGBarGrp/BtnRaycast").GetComponent<Button>();
            GameObject s04BHHighlight = Plugin.FindChildByPath(s04GameObject.transform, "AnimGrp/BGHighlight").gameObject;
            CardExpansionSelectScreen.Instance.m_BtnHighlightList.Add(s04BHHighlight);

            s04Button.onClick.AddListener(() =>
            {
                s04BHHighlight.SetActive(true);
                Plugin.SetPProperty(CardExpansionSelectScreen.Instance, "m_CurrentIndex", (int)Season.S04);
                currentExpensionIndex = (int)Season.S04;
            });
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
