using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.patch
{
    public class SortUI
    {
        public static bool inited = false;
        public static void OpenSortAlbumScreen(int sortingMethodIndex, int currentExpansionIndex, CollectionBinderUI __instance)
        {
            if (!inited)
            {
                // Position et texte des boutons initiaux
                __instance.m_ExpansionBtnList[0].GetParent().GetComponentInParent<RectTransform>().anchoredPosition = new Vector2(
                    0,
                    75
                );

                __instance.m_ExpansionBtnList[0].GetComponentInChildren<TextMeshProUGUI>().text = "Tout";
                __instance.m_ExpansionBtnList[1].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S01];
                __instance.m_ExpansionBtnList[2].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S02];

                __instance.m_ExpansionBtnList[3].gameObject.SetActive(true);
                __instance.m_ExpansionBtnList[3].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.S03];
                Button button3 = __instance.m_ExpansionBtnList[3].GetComponentInChildren<Button>();
                if (button3 != null)
                {
                    Plugin.Logger.LogInfo("button3 trouve");
                    button3.onClick.AddListener(() =>
                    {
                        Plugin.Logger.LogInfo("button4 clicked");
                        __instance.OnPressSwitchExpansion(3);
                    });
                }

                // Calculer l'espacement vertical constant entre les boutons
                float buttonHeight = __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().sizeDelta.y;
                // Utiliser un facteur pour espacer (par exemple 1.5x la hauteur du bouton)
                float verticalSpacing = __instance.m_ExpansionBtnList[2].GetComponent<RectTransform>().anchoredPosition.y - __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y;

                // Positionner le 5ème bouton en dessous du 4ème, avec un espacement cohérent
                __instance.m_ExpansionBtnList[4].gameObject.SetActive(true);
                __instance.m_ExpansionBtnList[4].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.x,
                    __instance.m_ExpansionBtnList[3].GetComponent<RectTransform>().anchoredPosition.y - verticalSpacing  // Espacement vertical
                );

                // Changer le texte du 5ème bouton
                __instance.m_ExpansionBtnList[4].GetComponentInChildren<TextMeshProUGUI>().text = SeasonsContainer.Seasons[Season.HS];
                Button button4 = __instance.m_ExpansionBtnList[4].GetComponentInChildren<Button>();
                if (button4 != null)
                {
                    Plugin.Logger.LogInfo("button4 trouve");
                    button4.onClick.AddListener(() =>
                    {
                        Plugin.Logger.LogInfo("button4 clicked");
                        __instance.OnPressSwitchExpansion(4);
                    });
                }

                // Marquer l'initialisation comme terminée
                inited = true;
            }
        }

        public static void CloseSortAlbumScreen()
        {

        }
    }
}
