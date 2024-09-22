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
                priceRangeMin = 0.01f;
                priceRangeMax = 0.05f;
            }
            else if (Drop >= 0.8f)
            {
                priceRangeMin = 0.05f;
                priceRangeMax = 0.02f;
            }
            else if (Drop >= 0.7f)
            {
                priceRangeMin = 0.02f;
                priceRangeMax = 0.05f;
            }
            else if (Drop >= 0.6f)
            {
                priceRangeMin = 0.05f;
                priceRangeMax = 0.1f;
            }
            else if (Drop >= 0.5f)
            {
                priceRangeMin = 0.1f;
                priceRangeMax = 0.15f;
            }
            else if (Drop >= 0.4f)
            {
                priceRangeMin = 0.15f;
                priceRangeMax = 0.2f;
            }
            else if (Drop >= 0.3f)
            {
                priceRangeMin = 0.2f;
                priceRangeMax = 0.25f;
            }
            else if (Drop >= 0.2f)
            {
                priceRangeMin = 0.25f;
                priceRangeMax = 0.3f;
            }
            else if (Drop >= 0.1f)
            {
                priceRangeMin = 0.3f;
                priceRangeMax = 0.5f;
            }
            else if (Drop >= 0.08f)
            {
                priceRangeMin = 0.5f;
                priceRangeMax = 0.8f;
            }
            else if (Drop >= 0.06f)
            {
                priceRangeMin = 0.8f;
                priceRangeMax = 1f;
            }
            else if (Drop >= 0.05f)
            {
                priceRangeMin = 1f;
                priceRangeMax = 2f;
            }
            else if (Drop >= 0.04f)
            {
                priceRangeMin = 2f;
                priceRangeMax = 2.5f;
            }
            else if (Drop >= 0.03f)
            {
                priceRangeMin = 4.5f;
                priceRangeMax = 8.0f;
            }
            else if (Drop >= 0.02f)
            {
                priceRangeMin = 10.0f;
                priceRangeMax = 25.0f;
            }
            else if (Drop >= 0.01f)
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
            marketPrice = UnityEngine.Random.Range(minPrice, maxPrice);

            // Mettre à jour la dernière modification de prix
            lastPriceUpdate = DateTime.Now;
        }


    }
}
