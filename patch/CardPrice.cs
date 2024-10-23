using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.cards;
using System.Reflection;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch
{
    [System.Serializable]
    public class CardPrice
    {

        public static float generateMarketPrice(WankulCardData wankulCardData)
        {
            // Augmenter la plage de variation aléatoire entre -2% et +2%
            float variation = UnityEngine.Random.Range(-0.02f, 0.02f);

            float priceFactor = 1f;
            if (wankulCardData.Season == Season.S01)
            {
                priceFactor = 1f;
            }
            else if (wankulCardData.Season == Season.S02) {
                priceFactor = 1.25f;
            }
            else if (wankulCardData.Season == Season.S03)
            {
                priceFactor = 1.5f;
            }
            else if (wankulCardData.Season == Season.HS)
            {
                priceFactor = 2f;
            }

            // Calculer le prix maximum en fonction du segment du Drop
            float priceRangeMax;
            float priceRangeMin;

            if (wankulCardData.Drop >= 0.45f) // Commune
            {
                priceRangeMin = 0.01f;
                priceRangeMax = 0.5f;
            }
            else if (wankulCardData.Drop >= 0.3f) // Peu Commune
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 1f;
            }
            else if (wankulCardData.Drop >= 0.1f && wankulCardData is TerrainCardData) // terrain
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 1.9f;
            }
            else if (wankulCardData.Drop >= 0.1f && wankulCardData is EffigyCardData) // Rare
            {
                priceRangeMin = 3.5f;
                priceRangeMax = 10f;
            }
            else if (wankulCardData.Drop >= 0.0224f) // Ultra rare holo 1
            {
                priceRangeMin = 10f;
                priceRangeMax = 50f;
            }
            else if (wankulCardData.Drop >= 0.016f) // Ultra rare holo 2
            {
                priceRangeMin = 50f;
                priceRangeMax = 150f;
            }
            else if (wankulCardData.Drop >= 0.008f) // Légendaire Bronze
            {
                priceRangeMin = 150;
                priceRangeMax = 500;
            }
            else if (wankulCardData.Drop >= 0.0028f) // Légendaire Argent
            {
                priceRangeMin = 500;
                priceRangeMax = 1000;
            }
            else if (wankulCardData.Drop >= 0.0008f) // Légendaire Or
            {
                priceRangeMin = 1000;
                priceRangeMax = 2500f;
            }
            else if (wankulCardData.Drop >= 0.0005f) // Gagnant Ticket Or (les abo ayant gagne)
            {
                priceRangeMin = 2500f;
                priceRangeMax = 4000f;
            }
            else if (wankulCardData.Drop >= 0.0001f) // LE TICKET D'OR
            {
                priceRangeMin = 10000f;
                priceRangeMax = 100000f;
            }
            else
            {
                priceRangeMin = 0.01f;
                priceRangeMax = 0.5f;
            }

            // Calculer le prix avec la variation aléatoire (influencée par le prix max du segment)
            float minPrice = priceRangeMin * (1 + variation);
            float maxPrice = priceRangeMax * (1 + variation);
            return UnityEngine.Random.Range(minPrice, maxPrice) * priceFactor;
        }

        public static void UpdateCardPricePercent(WankulCardData wankulCardData)
        {
            
            var m_PriceChangeMin = (float)AccessTools.Field(PriceChangeManager.Instance.GetType(), "m_PriceChangeMin").GetValue(PriceChangeManager.Instance);
            var m_PriceChangeMax = (float)AccessTools.Field(PriceChangeManager.Instance.GetType(), "m_PriceChangeMax").GetValue(PriceChangeManager.Instance);

            float percentChange = Random.Range(m_PriceChangeMin, m_PriceChangeMax);
            float increaseFactor = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;

            wankulCardData.Percentage += (percentChange * increaseFactor);
            if (wankulCardData.Percentage < -80f)
            {
                wankulCardData.Percentage = -80f;
            }

            if (wankulCardData.Percentage > 200f)
            {
                wankulCardData.Percentage = 200f;
            }

            wankulCardData.PastPercent.Add(wankulCardData.Percentage);
            if (wankulCardData.PastPercent.Count > 30)
            {
                wankulCardData.PastPercent.RemoveAt(0);
            }
        }


        public static void UpdateAllCardsMarketPrice()
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;

            foreach (WankulCardData wankulCardData in wankulCardsData.cards)
            {
                UpdateCardPricePercent(wankulCardData);
            }
        }

        public static void OnDayStarted()
        {
            UpdateAllCardsMarketPrice();
        }

        public static void Postfix_GetCardMarketPrice_CardData(CardData cardData, ref float __result)
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            WankulCardData wankulCardData = wankulCardsData.GetFromMonster(cardData, false);

            if (wankulCardData != null)
            {
                __result = wankulCardData.MarketPrice; // Utilise le prix du marché de ta carte
            }
            else
            {
                __result = 0; // Valeur par défaut si la carte n'est pas trouvée
            }
        }

        public static void Postfix_GetCardMarketPrice_ThreeParams(ECardExpansionType expansionType, ref float __result)
        {
            float variation = UnityEngine.Random.Range(-0.3f, 0.3f);
            float marketPrice = 20f; // Valeur par défaut

            switch (expansionType)
            {
                case ECardExpansionType.Tetramon:
                    marketPrice = Mathf.Clamp((1 + variation) * 100f, 5f, 100f);
                    break;
                case ECardExpansionType.Destiny:
                    marketPrice = Mathf.Clamp((1 + variation) * 3000f, 100f, 3000f);
                    break;
                case ECardExpansionType.Ghost:
                    marketPrice = Mathf.Clamp((1 + variation) * 5000f, 1000f, 5000f);
                    break;
                default:
                    marketPrice = Mathf.Clamp((1 + variation) * 100f, 5f, 100f);
                    break;
            }

            __result = marketPrice; // Affecte le prix calculé à __result
        }

        public static IEnumerator DelayRemoveCustomerFromQueue(float waitTime, Customer instance)
        {
            yield return new WaitForSeconds(waitTime);
            InteractableCashierCounter m_CurrentQueueCashierCounter = (InteractableCashierCounter)AccessTools.Field(instance.GetType(), "m_CurrentQueueCashierCounter").GetValue(instance);
            m_CurrentQueueCashierCounter.RemoveCustomerFromQueue(instance);
            m_CurrentQueueCashierCounter.RemoveCurrentCustomerFromQueue();
        }

        public static bool OnPayingDone(Customer __instance)
        {
            bool m_IsAtPayingPosition = (bool)AccessTools.Field(__instance.GetType(), "m_IsAtPayingPosition").GetValue(__instance);
            m_IsAtPayingPosition = false;
            AccessTools.Field(__instance.GetType(), "m_IsAtPayingPosition").SetValue(__instance, m_IsAtPayingPosition);

            bool m_HasCheckedOut = (bool)AccessTools.Field(__instance.GetType(), "m_HasCheckedOut").GetValue(__instance);
            m_HasCheckedOut = true;
            AccessTools.Field(__instance.GetType(), "m_HasCheckedOut").SetValue(__instance, m_HasCheckedOut);

            var m_Path = AccessTools.Field(__instance.GetType(), "m_Path").GetValue(__instance);
            m_Path = null;
            AccessTools.Field(__instance.GetType(), "m_Path").SetValue(__instance, m_Path);

            InteractableCashierCounter m_CurrentQueueCashierCounter = (InteractableCashierCounter)AccessTools.Field(__instance.GetType(), "m_CurrentQueueCashierCounter").GetValue(__instance);
            List<Item> m_ItemInBagList = (List<Item>)AccessTools.Field(__instance.GetType(), "m_ItemInBagList").GetValue(__instance);
            List<InteractableCard3d> m_CardInBagList = (List<InteractableCard3d>)AccessTools.Field(__instance.GetType(), "m_CardInBagList").GetValue(__instance);
            if (m_ItemInBagList.Count + m_CardInBagList.Count > 0)
            {
                __instance.m_ShoppingBagTransform.gameObject.SetActive(value: true);
            }

            __instance.m_Anim.SetBool("HoldingBag", value: true);
            m_CurrentQueueCashierCounter.SetPlsaticBagVisibility(isShow: false);
            m_CurrentQueueCashierCounter.UpdateCashierCounterState(ECashierCounterState.Idle);
            m_CurrentQueueCashierCounter.UpdateCurrentCustomer(null);
            float num = 0f;
            for (int i = 0; i < m_ItemInBagList.Count; i++)
            {
                m_ItemInBagList[i].transform.parent = __instance.m_ShoppingBagTransform;
                m_ItemInBagList[i].transform.position = __instance.m_ShoppingBagTransform.position;
                m_ItemInBagList[i].transform.rotation = __instance.m_ShoppingBagTransform.rotation;
                m_ItemInBagList[i].gameObject.SetActive(value: false);
                m_ItemInBagList[i].m_Collider.enabled = false;
                m_ItemInBagList[i].m_Rigidbody.isKinematic = true;
                m_ItemInBagList[i].m_InteractableScanItem.enabled = false;
                num += m_ItemInBagList[i].GetItemVolume();
            }


            int totalCardExp = 0;
            for (int j = 0; j < m_CardInBagList.Count; j++)
            {
                m_CardInBagList[j].transform.parent = __instance.m_ShoppingBagTransform;
                m_CardInBagList[j].transform.position = __instance.m_ShoppingBagTransform.position;
                m_CardInBagList[j].transform.rotation = __instance.m_ShoppingBagTransform.rotation;
                m_CardInBagList[j].m_Card3dUI.gameObject.SetActive(value: false);
                m_CardInBagList[j].gameObject.SetActive(value: false);
                m_CardInBagList[j].m_Collider.enabled = false;
                m_CardInBagList[j].m_Rigidbody.isKinematic = true;

                CardUI cardUi = m_CardInBagList[j].m_Card3dUI.m_CardUI;
                CardData cardData = (CardData)AccessTools.Field(cardUi.GetType(), "m_CardData").GetValue(cardUi);
                WankulCardData wankulCardData = WankulInventory.GetWankulCardFormGameCard(cardData).wankulcard;
                int exp = WankulCardsData.GetExperienceFromWankulCard(wankulCardData);
                totalCardExp += exp;

                Plugin.Logger.LogInfo($"Card exp: {exp} from card: {wankulCardData.Title}");
            }

            Plugin.Logger.LogInfo($"Total exp from cards: {totalCardExp}");

            __instance.StartCoroutine(DelayRemoveCustomerFromQueue(Random.Range(0.25f, 1f), __instance));
            MethodInfo DetermineShopAction = __instance.GetType().GetMethod("DetermineShopAction", BindingFlags.Instance | BindingFlags.NonPublic);
            DetermineShopAction.Invoke(__instance, new object[] {});



            CEventManager.QueueEvent(new CEventPlayer_AddShopExp(m_ItemInBagList.Count * 4 + Mathf.RoundToInt(num) + totalCardExp));
            return false;
        }
    }
}
