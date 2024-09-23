using UnityEngine;
using WankulCrazyPlugin.cards;

namespace WankulCrazyPlugin.patch
{
    [System.Serializable]
    public class CardPrice
    {

        public static void UpdateMarketPrice(WankulCardData wankulCardData)
        {
            // Augmenter la plage de variation aléatoire entre -2% et +2%
            float variation = UnityEngine.Random.Range(-0.02f, 0.02f);

            // Calculer le prix maximum en fonction du segment du Drop
            float priceRangeMax;
            float priceRangeMin;

            if (wankulCardData.Drop >= 0.9f)
            {
                priceRangeMin = 0.01f;
                priceRangeMax = 0.05f;
            }
            else if (wankulCardData.Drop >= 0.8f)
            {
                priceRangeMin = 0.05f;
                priceRangeMax = 0.02f;
            }
            else if (wankulCardData.Drop >= 0.7f)
            {
                priceRangeMin = 0.02f;
                priceRangeMax = 0.05f;
            }
            else if (wankulCardData.Drop >= 0.6f)
            {
                priceRangeMin = 0.05f;
                priceRangeMax = 0.1f;
            }
            else if (wankulCardData.Drop >= 0.5f)
            {
                priceRangeMin = 0.1f;
                priceRangeMax = 0.15f;
            }
            else if (wankulCardData.Drop >= 0.4f)
            {
                priceRangeMin = 0.15f;
                priceRangeMax = 0.2f;
            }
            else if (wankulCardData.Drop >= 0.3f)
            {
                priceRangeMin = 0.2f;
                priceRangeMax = 0.25f;
            }
            else if (wankulCardData.Drop >= 0.2f)
            {
                priceRangeMin = 0.25f;
                priceRangeMax = 0.3f;
            }
            else if (wankulCardData.Drop >= 0.1f)
            {
                priceRangeMin = 0.3f;
                priceRangeMax = 0.5f;
            }
            else if (wankulCardData.Drop >= 0.08f)
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 0.8f;
            }
            else if (wankulCardData.Drop >= 0.06f)
            {
                priceRangeMin = 0.8f;
                priceRangeMax = 1f;
            }
            else if (wankulCardData.Drop >= 0.05f)
            {
                priceRangeMin = 1f;
                priceRangeMax = 2f;
            }
            else if (wankulCardData.Drop >= 0.04f)
            {
                priceRangeMin = 2f;
                priceRangeMax = 2.5f;
            }
            else if (wankulCardData.Drop >= 0.03f)
            {
                priceRangeMin = 4.5f;
                priceRangeMax = 8.0f;
            }
            else if (wankulCardData.Drop >= 0.02f)
            {
                priceRangeMin = 10.0f;
                priceRangeMax = 25.0f;
            }
            else if (wankulCardData.Drop >= 0.01f)
            {
                priceRangeMin = 100.0f;
                priceRangeMax = 200.0f;
            }
            else
            {
                priceRangeMin = 200.0f;
                priceRangeMax = 300.0f;
            }

            // Calculer le prix avec la variation aléatoire (influencée par le prix max du segment)
            float minPrice = priceRangeMin * (1 + variation);
            float maxPrice = priceRangeMax * (1 + variation);
            wankulCardData.MarketPrice = UnityEngine.Random.Range(minPrice, maxPrice);
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
