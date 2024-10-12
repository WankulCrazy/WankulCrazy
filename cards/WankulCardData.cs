using System;
using System.Collections.Generic;
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

        public Texture2D TextureMask;

        public Sprite Sprite;

        public Sprite SpriteMask;

        public float Drop;

        public List<float> PastPercent = new List<float>();
        public float Percentage = 100;

        public float generatedMarketPrice;

        public float MarketPrice
        {
            get
            {
                if (generatedMarketPrice == 0)
                {
                    generatedMarketPrice = CardPrice.generateMarketPrice(this);
                }

                return generatedMarketPrice * (Percentage / 100);
            }
            set
            {
                generatedMarketPrice = value;
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
