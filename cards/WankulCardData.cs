using System;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.patch;

namespace WankulCrazyPlugin.cards
{
    public class WankulCardData
    {
        public static float timeToUpdatePrice = 20f;

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

        public List<float> PastPrices = new List<float>();

        private DateTime lastPriceUpdate;

        private float marketPrice;

        public float MarketPrice
        {
            get
            {
                // Met à jour le prix si plus de 20 minutes se sont écoulées
                if ((DateTime.Now - lastPriceUpdate).TotalMinutes > WankulCardData.timeToUpdatePrice)
                {
                    CardPrice.UpdateMarketPrice(this);
                }
                return marketPrice;
            }
            set
            {
                // Permet également de définir manuellement le prix du marché
                PastPrices.Add(marketPrice);
                if (PastPrices.Count > 30)
                {
                    PastPrices.RemoveAt(0);
                }

                marketPrice = value;
                lastPriceUpdate = DateTime.Now;
            }
        }

        public bool IsNumberInt(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        public int NumberInt
        {
            get
            {
                if (!IsNumberInt(Number))
                {
                    return -1;
                }
                return int.Parse(Number);
            }
        }
    }
}
