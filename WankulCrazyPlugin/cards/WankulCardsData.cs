using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.cards
{
    public class WankulCardsData : Singleton<WankulCardsData>
    {
        public List<WankulCardData> cards = [];
        public Dictionary<string, WankulCardData> association = [];

        public WankulCardData GetFromMonster(CardData monster)
        {
            string key = monster.monsterType.ToString() + "_" + monster.borderType.ToString();
            if (association.TryGetValue(key, out WankulCardData card))
            {
                return card;
            }
            else
            {
                return null;
            }
        }

        public void SetFromMonster(CardData monster, WankulCardData card)
        {
            string key = monster.monsterType.ToString() + "_" + monster.borderType.ToString();
            association[key] = card;
        }
    }
}
