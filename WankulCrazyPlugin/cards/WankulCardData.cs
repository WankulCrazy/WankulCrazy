using System;
using UnityEngine;

namespace WankulCrazyPlugin.cards
{
    public class WankulCardData
    {
        public int Index;

        public string Number;

        public string Title;

        public string Artist;

        public CardType CardType;

        public Season Season;

        public string TexturePath;

        public Texture2D Texture;

        public Sprite Sprite;

        public float Drop;

        private DateTime lastPriceUpdate;

        private float marketPrice;

        public float MarketPrice
        {
            get
            {
                // Met à jour le prix si plus de 10 minutes se sont écoulées
                if ((DateTime.Now - lastPriceUpdate).TotalMinutes > 10)
                {
                    UpdateMarketPrice();
                }
                return marketPrice;
            }
            set
            {
                // Permet également de définir manuellement le prix du marché
                marketPrice = value;
                lastPriceUpdate = DateTime.Now;
            }
        }

        // Méthode privée pour mettre à jour le prix du marché
        private void UpdateMarketPrice()
        {
            // Augmenter la plage de variation aléatoire entre -2% et +2%
            float variation = UnityEngine.Random.Range(-0.02f, 0.02f);

            // Calculer le prix maximum en fonction du segment du Drop
            float priceRangeMax;
            float priceRangeMin;

            if (Drop >= 0.9f)
            {
                priceRangeMin = 0.1f;
                priceRangeMax = 0.5f;
            }
            else if (Drop >= 0.8f)
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 0.2f;
            }
            else if (Drop >= 0.7f)
            {
                priceRangeMin = 0.2f;
                priceRangeMax = 0.5f;
            }
            else if (Drop >= 0.6f)
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 1f;
            }
            else if (Drop >= 0.5f)
            {
                priceRangeMin = 1f;
                priceRangeMax = 1.5f;
            }
            else if (Drop >= 0.4f)
            {
                priceRangeMin = 1.5f;
                priceRangeMax = 2f;
            }
            else if (Drop >= 0.3f)
            {
                priceRangeMin = 2f;
                priceRangeMax = 2.5f;
            }
            else if (Drop >= 0.2f)
            {
                priceRangeMin = 2.5f;
                priceRangeMax = 3f;
            }
            else if (Drop >= 0.1f)
            {
                priceRangeMin = 3f;
                priceRangeMax = 5f;
            }
            else if (Drop >= 0.08f)
            {
                priceRangeMin = 5f;
                priceRangeMax = 8f;
            }
            else if (Drop >= 0.06f)
            {
                priceRangeMin = 8f;
                priceRangeMax = 10f;
            }
            else if (Drop >= 0.05f)
            {
                priceRangeMin = 10f;
                priceRangeMax = 20f;
            }
            else if (Drop >= 0.04f)
            {
                priceRangeMin = 20f;
                priceRangeMax = 25f;
            }
            else if (Drop >= 0.03f)
            {
                priceRangeMin = 25f;
                priceRangeMax = 50f;
            }
            else if (Drop >= 0.02f)
            {
                priceRangeMin = 50f;
                priceRangeMax = 100f;
            }
            else if (Drop >= 0.01f)
            {
                priceRangeMin = 100f;
                priceRangeMax = 200f;
            }
            else
            {
                priceRangeMin = 200f;
                priceRangeMax = 300f;
            }

            // Calculer le prix avec la variation aléatoire (influencée par le prix max du segment)
            float minPrice = priceRangeMin * (1 + variation);
            float maxPrice = priceRangeMax * (1 + variation);
            marketPrice = UnityEngine.Random.Range(minPrice, maxPrice);

            // Mettre à jour la dernière modification de prix
            lastPriceUpdate = DateTime.Now;
        }


    }
}
