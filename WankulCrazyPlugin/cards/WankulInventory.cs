using System.Collections.Generic;
using UnityEngine;

namespace WankulCrazyPlugin.cards
{
    public class WankulInventory : MonoBehaviour
    {
        private static WankulInventory _instance;
        private static readonly object _lock = new object();
        public static WankulInventory Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Chercher une instance existante dans la scène
                        _instance = Object.FindObjectOfType<WankulInventory>();

                        // Si aucune instance n'est trouvée, en créer une nouvelle
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<WankulInventory>();
                            singletonObject.name = typeof(WankulInventory).ToString() + " (Singleton)";
                            DontDestroyOnLoad(singletonObject);
                            Debug.Log("CardsData singleton instance created");
                        }
                        else
                        {
                            Debug.Log("CardsData singleton instance found in scene");
                        }
                    }
                    else
                    {
                        Debug.Log("CardsData singleton instance already exists");
                    }
                    return _instance;
                }
            }
        }

        // Constructeur privé pour empêcher la création d'instances supplémentaires
        private WankulInventory() { }

        List<WankulCardData> cards = [];

        public static WankulCardData DropCard()
        {
            List<WankulCardData> cards = WankulCardsData.Instance.cards;

            float totalDropChance = 0f;
            foreach (var card in cards)
            {
                totalDropChance += card.Drop;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalDropChance);
            float cumulativeDropChance = 0f;

            foreach (var card in cards)
            {
                cumulativeDropChance += card.Drop;
                if (randomValue <= cumulativeDropChance)
                {
                    Plugin.Logger.LogInfo($"Dropped card: {card.Title}");
                    return card;
                }
            }

            // En cas d'erreur, retourner null
            Plugin.Logger.LogError("Failed to drop a card");
            return null;
        }

        public static List<WankulCardData> DropCards(int count)
        {
            List<WankulCardData> droppedCards = new List<WankulCardData>();
            for (int i = 0; i < count; i++)
            {
                WankulCardData card = DropCard();
                if (card != null)
                {
                    droppedCards.Add(card);
                }
            }
            return droppedCards;
        }

        public static void AddCard(WankulCardData card)
        {
            _instance.cards.Add(card);
        }

        public static void CardOpening(List<CardData> ___m_RolledCardDataList)
        {
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                Plugin.Logger.LogInfo("CardOpening: " + ___m_RolledCardDataList[i].monsterType);
            }
            DropCards(7);
            Plugin.Logger.LogInfo("CardOpening");
        }
    }
}
