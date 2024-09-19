using System.Collections.Generic;
using UnityEngine;

namespace WankulCrazyPlugin
{
    public class CardsData : MonoBehaviour
    {
        // Instance unique du singleton
        private static CardsData _instance;
        private static readonly object _lock = new object();

        // Propriété pour accéder à l'instance unique
        public static CardsData Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Chercher une instance existante dans la scène
                        _instance = FindObjectOfType<CardsData>();

                        // Si aucune instance n'est trouvée, en créer une nouvelle
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<CardsData>();
                            singletonObject.name = typeof(CardsData).ToString() + " (Singleton)";
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
        private CardsData() { }

        // Liste des cartes
        public List<CardData> cards = new List<CardData>();

        // Méthode appelée à la destruction de l'objet
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Debug.Log("CardsData singleton instance destroyed");
            }
        }
    }
}
