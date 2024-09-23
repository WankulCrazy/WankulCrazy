using System.Collections.Generic;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;

namespace WankulCrazyPlugin.patch
{
    public class CardOpening
    {
        public static void OpenBooster(List<CardData> ___m_RolledCardDataList, List<float> ___m_CardValueList, ECollectionPackType ___m_CollectionPackType)
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            ___m_CardValueList.Clear();
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                bool isTerrain = i == 0;
                bool isMinRare = i == ___m_RolledCardDataList.Count - 1;
                CardData inGameCard = ___m_RolledCardDataList[i];
                WankulCardData wankulCard = WankulInventory.DropCard(___m_CollectionPackType, isTerrain, isMinRare);
                CardData associatedCard = wankulCardsData.GetCardDataFromWankulCardData(wankulCard);
                Plugin.Logger.LogInfo($"Dropped Card added: {wankulCard.Title}, Market Price: {wankulCard.MarketPrice}, CardIngame: {inGameCard.monsterType}_{inGameCard.expansionType}_{inGameCard.borderType}");

                if (associatedCard != null)
                {
                    inGameCard = wankulCardsData.GetUnassciatedCardData();
                    ___m_RolledCardDataList[i] = inGameCard;
                }
                Plugin.Logger.LogInfo($"Set unassociated Card : {wankulCard.Title}, Market Price: {wankulCard.MarketPrice}, CardIngame: {inGameCard.monsterType}_{inGameCard.expansionType}_{inGameCard.borderType}");

                if (wankulCard != null)
                {
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
                    Plugin.Logger.LogError("Failed to drop a card");
                }

                if (wankulCard != null)
                {
                    WankulInventory.AddCard(wankulCard, inGameCard);

                    ___m_CardValueList.Add(wankulCard.MarketPrice);
                    Plugin.Logger.LogInfo($"Card added: {wankulCard.Title}, Market Price: {wankulCard.MarketPrice}");
                }
                else
                {
                    Plugin.Logger.LogError("Failed to drop a card");
                }
            }
        }
    }
}
