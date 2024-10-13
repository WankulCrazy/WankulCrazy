using HarmonyLib;
using UnityEngine;
using WankulCrazyPlugin.cards;

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
    }
}
