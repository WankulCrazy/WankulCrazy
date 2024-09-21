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
            // Augmenter la plage de variation aléatoire entre -30% et +30%
            float variation = UnityEngine.Random.Range(-0.3f, 0.3f);

            // Calculer le prix maximum en fonction du segment du Drop
            float priceRangeMax;

            if (Drop >= 0.9f)
                priceRangeMax = 0.5f;
            else if (Drop >= 0.8f)
                priceRangeMax = 0.2f;
            else if (Drop >= 0.7f)
                priceRangeMax = 0.5f;
            else if (Drop >= 0.6f)
                priceRangeMax = 1f;
            else if (Drop >= 0.5f)
                priceRangeMax = 1.5f;
            else if (Drop >= 0.4f)
                priceRangeMax = 2f;
            else if (Drop >= 0.3f)
                priceRangeMax = 2.5f;
            else if (Drop >= 0.2f)
                priceRangeMax = 3f;
            else if (Drop >= 0.1f)
                priceRangeMax = 5f;
            else if (Drop >= 0.08f)
                priceRangeMax = 8f;
            else if (Drop >= 0.06f)
                priceRangeMax = 10f;
            else if (Drop >= 0.05f)
                priceRangeMax = 20f;
            else if (Drop >= 0.04f)
                priceRangeMax = 25f;
            else if (Drop >= 0.03f)
                priceRangeMax = 50f;
            else if (Drop >= 0.02f)
                priceRangeMax = 100f;
            else if (Drop >= 0.01f)
                priceRangeMax = 200f;
            else
                priceRangeMax = 300f;

            // Calculer le prix avec la variation aléatoire (influencée par le prix max du segment)
            marketPrice = priceRangeMax * (1 + variation);

            // Mettre à jour la dernière modification de prix
            lastPriceUpdate = DateTime.Now;
        }


    }
}
