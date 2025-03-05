using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.patch
{
    public class UI_CashCounterScreenPatch
    {
        public static bool OnCardScanned(float value, CardData cardData, float totalItemCost, UI_CashCounterScreen __instance)
        {
            if (cardData == null || cardData.monsterType == EMonsterType.None)
            {
                return false;
            }
            for (int i = 0; i < __instance.m_CheckoutItemBarList.Count; i++)
            {
                if (!__instance.m_CheckoutItemBarList[i].gameObject.activeSelf)
                {
                    WankulCardData wankulCardData = WankulCardsData.Instance.GetFromMonster(cardData, true);
                    string text;
                    if (wankulCardData == null)
                    {
                        text = InventoryBase.GetMonsterData(cardData.monsterType).GetName() + " - " + CPlayerData.GetFullCardTypeName(cardData, ignoreRarity: true);
                    }
                    else if (wankulCardData is EffigyCardData effigyCard) {
                        text = effigyCard.Title + " - " + effigyCard.Rarity + " - " + effigyCard.Season;
                    }
                    else if (wankulCardData is WankulCardData wankulCard)
                    {
                        text = wankulCard.Title + " - " + wankulCard.CardType + " - " + wankulCard.Season;
                    }
                    else
                    {
                        text = wankulCardData.Title + " - " + wankulCardData.CardType + " - " + wankulCardData.Season;
                    }
                    __instance.m_CheckoutItemBarList[i].SetItemName(text, value);
                    __instance.m_CheckoutItemBarList[i].gameObject.SetActive(value: true);
                    Plugin.SetPProperty(__instance, "m_ActiveBarCount", (int)Plugin.GetPProperty(__instance, "m_ActiveBarCount") + 1);
                    Plugin.SetPProperty(__instance, "m_MaxPosX", Mathf.Clamp((float)((int)Plugin.GetPProperty(__instance, "m_ActiveBarCount") - 8) * 7.5f, 0f, 240f));
                    ((List<EItemType>)Plugin.GetPProperty(__instance, "m_ItemTypeList")).Add(EItemType.None);
                    break;
                }
            }
            Plugin.SetPProperty(__instance, "m_TotalItemCost", totalItemCost);
            __instance.m_TotalItemListCostText.text = GameInstance.GetPriceString((float)Plugin.GetPProperty(__instance, "m_TotalItemCost"));
            __instance.m_ScaledUpTotalText.text = __instance.m_TotalItemListCostText.text;

            return false;
        }
    }
}
