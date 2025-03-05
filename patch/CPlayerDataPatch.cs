using System;
using System.Collections.Generic;
using System.Text;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch
{
    public class CPlayerDataPatch
    {
        public static bool GetCardAmount(CardData cardData, ref int __result)
        {
            WankulCardData wankulCardData = WankulCardsData.Instance.GetFromMonster(cardData, true);
            if (wankulCardData == null)
            {
                return true;
            }
            else {
                int amount = WankulInventory.Instance.wankulCards.ContainsKey(wankulCardData.Index) ? WankulInventory.Instance.wankulCards[wankulCardData.Index].amount : 0;
                __result = amount;
                return false;
            }
        }
    }
}
