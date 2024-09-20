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

        public static void AddCard(WankulCardData card)
        {
            Instance.cards.Add(card);
        }

        public static void CardOpening(List<CardData> ___m_RolledCardDataList)
        {
            WankulCardsData wankulCardsData = WankulCardsData.Instance;
            Plugin.Logger.LogInfo("CardOpening");
            for (int i = 0; i < ___m_RolledCardDataList.Count; i++)
            {
                Plugin.Logger.LogInfo("CardOpening: " + ___m_RolledCardDataList[i].monsterType);

                WankulCardData card = DropCard();
                if (card != null)
                {
                    Plugin.Logger.LogInfo("CardOpening: " + card.Title);
                    AddCard(card);
                    wankulCardsData.SetFromMonster(___m_RolledCardDataList[i].monsterType, card);
                }
            }
        }
    }
}
