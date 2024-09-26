using System.Collections.Generic;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.patch
{
    public class CardOpening
    {
        public static void OpenBooster(List<CardData> ___m_RolledCardDataList, List<float> ___m_CardValueList, ECollectionPackType ___m_CollectionPackType)
        {
            if (SavesManager.DebuggingSave)
            {
                return;
            }
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            ___m_CardValueList.Clear();
            int totalExpGained = 0;
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                bool isTerrain = i == 0;
                bool isMinRare = i == ___m_RolledCardDataList.Count - 1;
                CardData inGameCard = ___m_RolledCardDataList[i];
                WankulCardData wankulCard = WankulInventory.DropCard(___m_CollectionPackType, isTerrain, isMinRare);
                CardData associatedCard = wankulCardsData.GetCardDataFromWankulCardData(wankulCard);

                if (associatedCard == null)
                {
                    inGameCard = wankulCardsData.GetUnassciatedCardData();
                    ___m_RolledCardDataList[i] = inGameCard;

                    inGameCard.isFoil = false;
                    inGameCard.isChampionCard = false;
                    if (wankulCard is EffigyCardData)
                    {
                        EffigyCardData effigyCard = (EffigyCardData)wankulCard;

                        if (effigyCard.Rarity >= Rarity.UR1)
                        {
                            inGameCard.isFoil = true;
                        }
                    }

                    wankulCardsData.SetFromMonster(inGameCard, wankulCard);
                }
                else
                {
                    inGameCard = associatedCard;
                    ___m_RolledCardDataList[i] = inGameCard;
                }

                totalExpGained = totalExpGained + WankulCardsData.GetExperienceFromWankulCard(wankulCard);
                ___m_CardValueList.Add(wankulCard.MarketPrice);
            }

            CEventManager.QueueEvent(new CEventPlayer_AddShopExp(totalExpGained));
        }
    }
}
