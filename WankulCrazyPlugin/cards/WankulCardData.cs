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

        private const float MIN_PRICE = 0f;
        private const float MAX_PRICE = 3000f;

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
            // Variation aléatoire entre -30% et +30%
            float variation = UnityEngine.Random.Range(-0.3f, 0.3f);

            // Calcul du nouveau prix basé sur le taux de drop et la variation
            marketPrice = Drop * (1 + variation) * MAX_PRICE;

            // Empêche le prix d'être inférieur à 0 ou supérieur à MAX_PRICE (3000 euros)
            marketPrice = Mathf.Clamp(marketPrice, MIN_PRICE, MAX_PRICE);

            // Mise à jour de la dernière modification de prix
            lastPriceUpdate = DateTime.Now;

            Debug.Log($"Le prix du marché de la carte '{Title}' a été mis à jour : {marketPrice} euros");
        }
    }
}
