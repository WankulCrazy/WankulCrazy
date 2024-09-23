using System;
using UnityEngine;
using WankulCrazyPlugin.patch;

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
                // Met à jour le prix si plus de 20 minutes se sont écoulées
                if ((DateTime.Now - lastPriceUpdate).TotalMinutes > 20)
                {
                    CardPrice.UpdateMarketPrice(this);
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
    }
}
