using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.inventory;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.patch
{
    public class CardOpening
    {
        public static int totalExpGained = 0;
        public static List<int> LegendaryBoosters = new List<int>();
        public static void UpdatePreFix(ref List<CardData> ___m_RolledCardDataList, CardOpeningSequence __instance)
        {
            if (__instance.m_StateIndex == 11 && totalExpGained > 0)
            {
                ___m_RolledCardDataList.Clear();
                CEventManager.QueueEvent(new CEventPlayer_AddShopExp(totalExpGained));
                totalExpGained = 0;
            }
        }
        public static void OpenBooster(List<CardData> ___m_RolledCardDataList, List<float> ___m_CardValueList, ECollectionPackType ___m_CollectionPackType, Item ___m_CurrentItem)
        {
            if (SavesManager.DebuggingSave)
            {
                return;
            }

            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            ___m_CardValueList.Clear();
            totalExpGained = 0;

            int boosterSize = ___m_RolledCardDataList.Count;
            List<WankulCardData> alreadySelectedCards = new List<WankulCardData>();

            for (int i = 0; i < boosterSize; i++)
            {
                bool isTerrain = i == 0;
                bool isMinRare = i == boosterSize - 1;
                bool isMinLegendary = false;
                int hash = ___m_CurrentItem.GetHashCode();

                if (isMinRare)
                {
                    foreach (int boosterHash in LegendaryBoosters)
                    {
                        if (boosterHash == hash)
                        {
                            LegendaryBoosters.Remove(boosterHash);
                            isMinLegendary = true;
                            isMinRare = false;
                            break;
                        }
                    }
                }
                WankulCardData wankulCard = WankulInventory.DropCard(___m_CollectionPackType, alreadySelectedCards, isTerrain, isMinRare, isMinLegendary);
                CardData inGameCard = ___m_RolledCardDataList[i];
                CardData associatedCard = wankulCardsData.GetCardDataFromWankulCardData(wankulCard);

                if (associatedCard == null)
                {
                    inGameCard = wankulCardsData.GetUnassciatedCardData();
                    ___m_RolledCardDataList[i] = inGameCard;

                    inGameCard.isFoil = false;
                    inGameCard.isChampionCard = false;

                    // Vérification de la rareté pour décider si la carte est foil
                    if (wankulCard is EffigyCardData)
                    {
                        EffigyCardData effigyCard = (EffigyCardData)wankulCard;

                        // Si la carte a une rareté de UR1 ou plus, elle devient foil
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

                // Calcul de l'XP gagnée
                totalExpGained += WankulCardsData.GetExperienceFromWankulCard(wankulCard);
                // Ajout de la valeur de la carte dans la liste des prix
                ___m_CardValueList.Add(wankulCard.MarketPrice);
            }
            Plugin.Logger.LogInfo($"OpenBooster totalExpGained {totalExpGained}");
        }

        public class EvaluateOpenCardPack__State
        {
            public bool CanOpenCardBox;
        }

        public static void EvaluateOpenCardPackPreFix(out EvaluateOpenCardPack__State __state, InteractionPlayerController __instance)
        {
            __state = new EvaluateOpenCardPack__State();
            if (__instance.CanOpenCardBox())
            {
                __state.CanOpenCardBox = true;
            }
            else
            {
                __state.CanOpenCardBox = false;
            }
        }

        public static void EvaluateOpenCardPackPostFix(EvaluateOpenCardPack__State __state, InteractionPlayerController __instance)
        {
            if (__state.CanOpenCardBox)
            {
                List<Item> m_HoldItemList = (List<Item>)AccessTools.Field(__instance.GetType(), "m_HoldItemList").GetValue(__instance);
                int randomIndex = Random.RandomRangeInt(0, m_HoldItemList.Count);
                Item item = m_HoldItemList[randomIndex];
                int hash = item.GetHashCode();
                LegendaryBoosters.Add(hash);
            }
        }
    }
}
