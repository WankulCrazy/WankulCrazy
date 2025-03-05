using I2.Loc;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WankulCrazyPlugin;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch
{
    public class CustomerTradeCardScreenPatch
    {

        public static bool SetCustomer(Customer customer, CustomerTradeData customerTradeData, CustomerTradeCardScreen __instance)
        {
            CSingleton<CustomerManager>.Instance.m_IsPlayerTrading = true;
            Plugin.SetPProperty(__instance, "m_CurrentCustomer", customer);
            Plugin.SetPProperty(__instance, "m_HasAccepted", false);
            Plugin.SetPProperty(__instance, "m_IsTrading", false);
            int num = CPlayerData.m_ShopLevel;
            if (num > 40)
            {
                num = 40;
            }
            if (Random.Range(0, 100) < num)
            {
                Plugin.SetPProperty(__instance, "m_IsTrading", true);
            }
            if (customerTradeData != null)
            {
                Plugin.SetPProperty(__instance, "m_IsTrading", customerTradeData.m_IsTrading);
            }
            if ((bool)Plugin.GetPProperty(__instance, "m_IsTrading"))
            {
                __instance.m_CustomerTopText.text = LocalizationManager.GetTranslation("CustomerTrade/" + __instance.m_CustomerTradeCardTextList[Random.Range(0, __instance.m_CustomerTradeCardTextList.Count)]);
                __instance.m_CustomerTopTextAnim.Rewind();
                __instance.m_CustomerTopTextAnim.Play();
            }
            else
            {
                Plugin.SetPProperty(__instance, "m_MaxDeclineCount", Random.Range(0, 5));
                Plugin.SetPProperty(__instance, "m_DeclineCount", 0);
                __instance.m_CustomerTopText.text = LocalizationManager.GetTranslation("CustomerTrade/" + __instance.m_CustomerSellCardTextList[Random.Range(0, __instance.m_CustomerSellCardTextList.Count)]);
                __instance.m_CustomerTopTextAnim.Rewind();
                __instance.m_CustomerTopTextAnim.Play();
            }
            __instance.m_CustomerTradingText.SetActive((bool)Plugin.GetPProperty(__instance, "m_IsTrading"));
            __instance.m_CustomerSellingText.SetActive(!(bool)Plugin.GetPProperty(__instance, "m_IsTrading"));
            __instance.m_TradeGrp_R.SetActive((bool)Plugin.GetPProperty(__instance, "m_IsTrading"));
            __instance.m_SellGrp_R.SetActive(!(bool)Plugin.GetPProperty(__instance, "m_IsTrading"));
            float num3 = 1f;
            


            


            if (customerTradeData != null)
            {
                Plugin.SetPProperty(__instance, "m_CardData_L", customerTradeData.m_CardData_L);
            }
            else {
                (WankulCardData wankulCardData, CardData cardData, int amount) dropL = WankulInventory.GetWankulCardDataForTradeOffer();
                Plugin.SetPProperty(__instance, "m_CardData_L", dropL.cardData);
            }

            bool active = ((int)CPlayerData.GetCardAmount((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"))) == 0;
            __instance.m_IsNewUI.SetActive(active);
            __instance.m_CardUI_L.SetCardUI((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));
            __instance.m_CardUI_Album_L.SetCardUI((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));
            __instance.m_AlbumCardCount_L.text = "X" + CPlayerData.GetCardAmount((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));
            __instance.m_AcceptBtn.SetActive(value: true);
            __instance.m_CancelBtn.SetActive(value: true);
            __instance.m_LetMeThinkBtn.SetActive(value: true);
            __instance.m_DoneBtn.SetActive(value: false);
            if (!(bool)Plugin.GetPProperty(__instance, "m_IsTrading"))
            {
                float num8 = Random.Range(0.6f, 1.3f);
                if (num8 < 0.7f)
                {
                    num8 = 0.7f;
                }
                if (num8 > 1.2f)
                {
                    num8 = 1.2f;
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 10f)
                {
                    num8 += Random.Range(0f, 0.03f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 20f)
                {
                    num8 += Random.Range(0f, 0.04f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 100f)
                {
                    num8 += Random.Range(0.01f, 0.05f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 300f)
                {
                    num8 += Random.Range(0.01f, 0.06f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 500f)
                {
                    num8 += Random.Range(0.01f, 0.08f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 1000f)
                {
                    num8 += Random.Range(0.01f, 0.1f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 2000f)
                {
                    num8 += Random.Range(0.02f, 0.11f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 5000f)
                {
                    num8 += Random.Range(0.03f, 0.12f);
                }
                if ((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") > 8000f)
                {
                    num8 += Random.Range(0.03f, 0.12f);
                }
                Plugin.SetPProperty(__instance, "m_SellCardMarketPrice", CPlayerData.GetCardMarketPrice((CardData)Plugin.GetPProperty(__instance, "m_CardData_L")));
                num3 = (float)(Plugin.SetPProperty(__instance, "m_SellCardAskPrice", (float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice") * num8));
                if (customerTradeData != null)
                {
                    Plugin.SetPProperty(__instance, "m_PriceSet", customerTradeData.m_PriceSet);
                    Plugin.SetPProperty(__instance, "m_LastPriceSet", customerTradeData.m_LastPriceSet);
                    Plugin.SetPProperty(__instance, "m_SellCardAskPrice", customerTradeData.m_SellCardAskPrice);
                    Plugin.SetPProperty(__instance, "m_MaxDeclineCount", customerTradeData.m_MaxDeclineCount);
                    Plugin.SetPProperty(__instance, "m_DeclineCount", customerTradeData.m_DeclineCount);
                    __instance.m_SetPrice.text = GameInstance.GetPriceString((float)Plugin.GetPProperty(__instance,"m_PriceSet"));
                    __instance.m_SetPriceInputDisplay.text = GameInstance.GetPriceString((float)Plugin.GetPProperty(__instance, "m_PriceSet"));
                }
                else
                {
                    __instance.OnInputTextUpdated("0");
                }
                __instance.m_MarketPrice_L.text = LocalizationManager.GetTranslation("Ask Price") + " : " + GameInstance.GetPriceString((float)Plugin.GetPProperty(__instance, "m_SellCardAskPrice"));
                __instance.m_MarketPrice_L.text += "\n";
                TextMeshProUGUI marketPrice_L = __instance.m_MarketPrice_L;
                marketPrice_L.text = marketPrice_L.text + LocalizationManager.GetTranslation("Market Price") + " : " + GameInstance.GetPriceString((float)Plugin.GetPProperty(__instance, "m_SellCardMarketPrice"));
                __instance.m_CardUI_Buying.SetCardUI((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));
            }
            else
            {
                num3 = CPlayerData.GetCardMarketPrice((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));
                __instance.m_MarketPrice_L.text = LocalizationManager.GetTranslation("Market Price") + " : " + GameInstance.GetPriceString(num3);
            }
            if (!(bool)Plugin.GetPProperty(__instance, "m_IsTrading"))
            {
                return false;
            }

            if (customerTradeData == null)
            {
                (WankulCardData wankulCardData, CardData cardData, int amount) dropR = WankulInventory.GetWankulCardDataForTradeOfferByPrice((CardData)Plugin.GetPProperty(__instance, "m_CardData_L"));

                Plugin.SetPProperty(__instance, "m_CardData_R", dropR.cardData);
            }
            else if (customerTradeData != null)
            {
                Plugin.SetPProperty(__instance, "m_CardData_R", customerTradeData.m_CardData_R);
            }

            __instance.m_CardUI_R.SetCardUI((CardData)Plugin.GetPProperty(__instance, "m_CardData_R"));
            __instance.m_CardUI_Album.SetCardUI((CardData)Plugin.GetPProperty(__instance, "m_CardData_R"));
            __instance.m_AlbumCardCount.text = "X" + CPlayerData.GetCardAmount((CardData)Plugin.GetPProperty(__instance, "m_CardData_R"));
            __instance.m_MarketPrice_R.text = LocalizationManager.GetTranslation("Market Price") + " : " + GameInstance.GetPriceString(CPlayerData.GetCardMarketPrice((CardData)Plugin.GetPProperty(__instance, "m_CardData_R")));

            return false;
        }

    }
}
