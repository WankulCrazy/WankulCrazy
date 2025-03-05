using CMF;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using WankulCrazyPlugin.utils;


namespace WankulCrazyPlugin.patch
{
    public static class InteractionPlayerControllerPatch
    {
        const int MaxPacks = 24;
        const int PacksPerColumn = 12;

        [HarmonyPatch(typeof(InteractionPlayerController), "Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(InteractionPlayerController __instance)
        {

            if (__instance.m_HoldCardPackPosList == null)
                return;

            while (__instance.m_HoldCardPackPosList.Count < MaxPacks)
            {
                Transform lastTransform = __instance.m_HoldCardPackPosList.Last();
                Vector3 newPosition = lastTransform.localPosition;

                if (__instance.m_HoldCardPackPosList.Count >= PacksPerColumn)
                {
                    // Seconde Colonne
                    newPosition.x = -0.07f;
                    newPosition.z = __instance.m_HoldCardPackPosList[__instance.m_HoldCardPackPosList.Count % PacksPerColumn].localPosition.z;
                }
                else
                {
                    newPosition.z -= 0.02f;
                }

                GameObject newPackPos = new GameObject($"CardPackPos_{__instance.m_HoldCardPackPosList.Count}");
                newPackPos.transform.SetParent(lastTransform.parent);
                newPackPos.transform.localPosition = newPosition;
                newPackPos.transform.localRotation = lastTransform.localRotation;
                newPackPos.transform.localScale = Vector3.one; // Echelle des packs (1, 1, 1)

                __instance.m_HoldCardPackPosList.Add(newPackPos.transform);
            }

            // Position final des packs
            for (int i = 0; i < __instance.m_HoldCardPackPosList.Count; i++)
            {
                Transform packTransform = __instance.m_HoldCardPackPosList[i];
            }
        }

        public static bool CanOpenPack(ref bool __result, InteractionPlayerController __instance)
        {
            bool result = false;
            EItemType boosterStellar = EnumExtensions.SafeParseEItemType("BoosterStellar");
            EItemType boosterStellarTaux = EnumExtensions.SafeParseEItemType("BoosterStellarTaux");

            List<Item> m_HoldItemList = (List<Item>)AccessTools.Field(typeof(InteractionPlayerController), "m_HoldItemList").GetValue(__instance);
            if (m_HoldItemList.Count > 0)
            {
                Item item = m_HoldItemList[0];
                result = item.GetItemType() == EItemType.BasicCardPack || item.GetItemType() == EItemType.RareCardPack || item.GetItemType() == EItemType.EpicCardPack || item.GetItemType() == EItemType.LegendaryCardPack || item.GetItemType() == EItemType.DestinyBasicCardPack || item.GetItemType() == EItemType.DestinyRareCardPack || item.GetItemType() == EItemType.DestinyEpicCardPack || item.GetItemType() == EItemType.DestinyLegendaryCardPack || item.GetItemType() == EItemType.GhostPack || item.GetItemType() == EItemType.MegabotPack || item.GetItemType() == EItemType.FantasyRPGPack || item.GetItemType() == EItemType.CatJobPack || item.GetItemType() == boosterStellar || item.GetItemType() == boosterStellarTaux;
            }
            __result = result;
            return false;
        }

        public static bool CanOpenCardBox(ref bool __result, InteractionPlayerController __instance)
        {
            bool result = false;
            EItemType displayStellar = EnumExtensions.SafeParseEItemType("DisplayStellar");
            EItemType displayStellarTaux = EnumExtensions.SafeParseEItemType("DisplayStellarTaux");

            List<Item> m_HoldItemList = (List<Item>)AccessTools.Field(typeof(InteractionPlayerController), "m_HoldItemList").GetValue(__instance);
            if (m_HoldItemList.Count > 0)
            {
                Item item = m_HoldItemList[0];
                result = item.GetItemType() == EItemType.BasicCardBox || item.GetItemType() == EItemType.RareCardBox || item.GetItemType() == EItemType.EpicCardBox || item.GetItemType() == EItemType.LegendaryCardBox || item.GetItemType() == EItemType.DestinyBasicCardBox || item.GetItemType() == EItemType.DestinyRareCardBox || item.GetItemType() == EItemType.DestinyEpicCardBox || item.GetItemType() == EItemType.DestinyLegendaryCardBox || item.GetItemType() == displayStellar || item.GetItemType() == displayStellarTaux;
            }
            __result = result;
            return false;
        }

        public static bool CardBoxToCardPack(EItemType cardBoxItemType, ref EItemType __result)
        {
            // Récupère les valeurs dynamiques
            EItemType displayStellar = EnumExtensions.SafeParseEItemType("DisplayStellar");
            EItemType displayStellarTaux = EnumExtensions.SafeParseEItemType("DisplayStellarTaux");
            EItemType boosterStellar = EnumExtensions.SafeParseEItemType("BoosterStellar");
            EItemType boosterStellarTaux = EnumExtensions.SafeParseEItemType("BoosterStellarTaux");

            if (cardBoxItemType == EItemType.BasicCardBox)
                __result = EItemType.BasicCardPack;
            else if (cardBoxItemType == EItemType.RareCardBox)
                __result = EItemType.RareCardPack;
            else if (cardBoxItemType == EItemType.EpicCardBox)
                __result = EItemType.EpicCardPack;
            else if (cardBoxItemType == EItemType.LegendaryCardBox)
                __result = EItemType.LegendaryCardPack;
            else if (cardBoxItemType == EItemType.DestinyBasicCardBox)
                __result = EItemType.DestinyBasicCardPack;
            else if (cardBoxItemType == EItemType.DestinyRareCardBox)
                __result = EItemType.DestinyRareCardPack;
            else if (cardBoxItemType == EItemType.DestinyEpicCardBox)
                __result = EItemType.DestinyEpicCardPack;
            else if (cardBoxItemType == EItemType.DestinyLegendaryCardBox)
                __result = EItemType.DestinyLegendaryCardPack;
            else if (cardBoxItemType == displayStellar)
                __result = boosterStellar;
            else if (cardBoxItemType == displayStellarTaux)
                __result = boosterStellarTaux;
            else
                __result = EItemType.None;

            return false;
        }

        // Nouvelle méthode RemoveToolTip
        public static void RemoveToolTip(EGameAction action)
        {
            var inputTooltipListDisplayField = AccessTools.Field(typeof(InteractionPlayerController), "m_InputTooltipListDisplay");
            var inputTooltipListDisplay = inputTooltipListDisplayField.GetValue(CSingleton<InteractionPlayerController>.Instance) as InputTooltipListDisplay;

            inputTooltipListDisplay.RemoveTooltip(action);
        }

        // Nouvelle méthode EvaluateOpenCardPack
        [HarmonyPatch(typeof(InteractionPlayerController), "EvaluateOpenCardPack")]
        public static bool EvaluateOpenCardPack(InteractionPlayerController __instance)
        {
            List<Item> m_HoldItemList = (List<Item>)AccessTools.Field(__instance.GetType(), "m_HoldItemList").GetValue(__instance);
            var canOpenPackMethod = AccessTools.Method(__instance.GetType(), "CanOpenPack");
            bool canOpenPack = (bool)canOpenPackMethod.Invoke(__instance, null);

            if (canOpenPack)
            {
                Item holdItem = m_HoldItemList[0];
                var removeHoldItemMethod = AccessTools.Method(__instance.GetType(), "RemoveHoldItem");
                removeHoldItemMethod.Invoke(__instance, new object[] { holdItem });

                CSingleton<CardOpeningSequence>.Instance.ReadyingCardPack(holdItem);

                AccessTools.Field(__instance.GetType(), "m_IsHoldingMouseDown").SetValue(__instance, false);
                AccessTools.Field(__instance.GetType(), "m_IsHoldingRightMouseDown").SetValue(__instance, false);
            }
            else
            {
                var canOpenCardBoxMethod = AccessTools.Method(__instance.GetType(), "CanOpenCardBox");
                bool canOpenCardBox = (bool)canOpenCardBoxMethod.Invoke(__instance, null);
                if (!canOpenCardBox)
                {
                    return false;
                }

                AccessTools.Field(__instance.GetType(), "m_IsOpeningCardBox").SetValue(__instance, true);
                Item holdItem = m_HoldItemList[0];
                EItemType cardPack = (EItemType)AccessTools.Method(__instance.GetType(), "CardBoxToCardPack").Invoke(__instance, new object[] { holdItem.GetItemType() });

                ItemMeshData itemMeshData1 = InventoryBase.GetItemMeshData(holdItem.GetItemType());

                var openCardBoxMeshFilterField = AccessTools.Field(__instance.GetType(), "m_OpenCardBoxMeshFilter");
                var openCardBoxMeshField = AccessTools.Field(__instance.GetType(), "m_OpenCardBoxMesh");

                var openCardBoxMeshFilter = openCardBoxMeshFilterField.GetValue(__instance) as MeshFilter;
                var openCardBoxMesh = openCardBoxMeshField.GetValue(__instance) as MeshRenderer;

                openCardBoxMeshFilter.mesh = itemMeshData1.mesh;
                openCardBoxMesh.material = itemMeshData1.material;
                holdItem.gameObject.SetActive(false);
                m_HoldItemList.Clear();
                CPlayerData.m_HoldItemTypeList.Clear();
                AccessTools.Method(typeof(InteractionPlayerController), "RemoveToolTip").Invoke(null, new object[] { EGameAction.OpenCardBox });
                SoundManager.PlayAudio("SFX_OpenCardBox", 0.6f);

                var openCardBoxInnerMeshField = AccessTools.Field(__instance.GetType(), "m_OpenCardBoxInnerMesh");
                var openCardBoxInnerMesh = openCardBoxInnerMeshField.GetValue(__instance) as Animation;
                openCardBoxInnerMesh.gameObject.SetActive(true);

                var openCardBoxSpawnCardPackPosListField = AccessTools.Field(__instance.GetType(), "m_OpenCardBoxSpawnCardPackPosList");
                var openCardBoxSpawnCardPackPosList = openCardBoxSpawnCardPackPosListField.GetValue(__instance) as List<Transform>;

                var holdCardPackPosListField = AccessTools.Field(__instance.GetType(), "m_HoldCardPackPosList");
                var holdCardPackPosList = holdCardPackPosListField.GetValue(__instance) as List<Transform>;

                if (m_HoldItemList.Count < 24)
                {
                    while (m_HoldItemList.Count < 24)
                    {
                        Item fakeItem = ItemSpawnManager.GetItem(null);
                        m_HoldItemList.Add(fakeItem);
                    }
                }


                // Déclarer un parent par défaut si nécessaire (remplacer "SomeParentObject" par un objet réel de votre scène)
                Transform someDefaultParentTransform = GameObject.Find("Level_Environment_Grp")?.transform; // Remplacer "SomeParentObject" par le nom réel de l'objet qui servira de parent par défaut

                if (someDefaultParentTransform == null)
                {
                    Plugin.Logger.LogError("Le parent par défaut 'SomeParentObject' n'a pas été trouvé dans la scène !");
                }

                if (openCardBoxSpawnCardPackPosList.Count < 24)
                {
                    // Récupérer le dernier élément valide pour baser les éléments fictifs
                    Transform lastValidTransform = openCardBoxSpawnCardPackPosList[openCardBoxSpawnCardPackPosList.Count - 1];

                    // Vérifier si le dernier élément valide a un parent, sinon définisser un parent par défaut
                    Transform defaultParent = lastValidTransform.parent != null ? lastValidTransform.parent : someDefaultParentTransform;

                    while (openCardBoxSpawnCardPackPosList.Count < 24)
                    {
                        Transform fakeTransform = new GameObject("FakeTransform").transform;
                        fakeTransform.localPosition = new Vector3(0.02f, 0, 0);
                        fakeTransform.localRotation = lastValidTransform.localRotation * Quaternion.Euler(0, 0.5f, 0);
                        fakeTransform.localScale = lastValidTransform.localScale;
                        fakeTransform.name = $"SpawnCardPackPosLoc ({openCardBoxSpawnCardPackPosList.Count})";
                        fakeTransform.parent = defaultParent;
                        openCardBoxSpawnCardPackPosList.Add(fakeTransform);
                        lastValidTransform = fakeTransform;
                    }
                }

                // Pour rattacher les éléments sans parent
                for (int i = 0; i < openCardBoxSpawnCardPackPosList.Count; i++)
                {
                    if (openCardBoxSpawnCardPackPosList[i].parent == null)
                    {
                        openCardBoxSpawnCardPackPosList[i].parent = someDefaultParentTransform;  // Assigner le parent par défaut
                    }
                }

                for (int i = 0; i < openCardBoxSpawnCardPackPosList.Count; i++)
                {
                    Transform item = openCardBoxSpawnCardPackPosList[i];
                }

                if (__instance.m_HoldCardPackPosList.Count < 24)
                {
                    // Récupérer le dernier élément valide pour baser les éléments fictifs
                    Transform lastValidTransform = __instance.m_HoldCardPackPosList.Count > 0 ? __instance.m_HoldCardPackPosList[__instance.m_HoldCardPackPosList.Count - 1] : null;

                    // Déclarer un parent par défaut si nécessaire (remplacer "SomeParentObject" par un objet réel de votre scène)
                    Transform someDefaultParentTransformV2 = GameObject.Find("SomeParentObject")?.transform;

                    // Vérifier si lastValidTransform a un parent, sinon définisser un parent par défaut
                    Transform defaultParent = lastValidTransform?.parent != null ? lastValidTransform.parent : someDefaultParentTransformV2;

                    while (__instance.m_HoldCardPackPosList.Count < 24)
                    {
                        Transform fakeTransform = new GameObject("FakeTransform").transform;
                        fakeTransform.localPosition = lastValidTransform != null ? lastValidTransform.localPosition + new Vector3(0.02f, 0, 0) : Vector3.zero;
                        fakeTransform.localRotation = lastValidTransform != null ? lastValidTransform.localRotation * Quaternion.Euler(0, 0.5f, 0) : Quaternion.identity;
                        fakeTransform.localScale = lastValidTransform != null ? lastValidTransform.localScale : Vector3.one;
                        fakeTransform.name = $"HoldCardPackPosLoc ({__instance.m_HoldCardPackPosList.Count})";
                        fakeTransform.parent = defaultParent;
                        __instance.m_HoldCardPackPosList.Add(fakeTransform);
                        lastValidTransform = fakeTransform;
                    }
                }

                for (int index = 0; index < openCardBoxSpawnCardPackPosList.Count; ++index)
                {
                    if (index < m_HoldItemList.Count)
                    {
                        ItemMeshData itemMeshData2 = InventoryBase.GetItemMeshData(cardPack);
                        Item obj = ItemSpawnManager.GetItem(openCardBoxSpawnCardPackPosList[index]);
                        if (obj != null)
                        {
                            Transform cardBoxInnerMesh = __instance.m_HoldCardPackPosList[index].Find("CardBoxInnerMesh");
                            if (cardBoxInnerMesh != null)
                            {
                                Vector3[] packPositions = new Vector3[]
                                {
                                    new Vector3(-0.05f, 0.1f - 0.08f, 0),
                                    new Vector3(0.05f, 0.1f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.096f - 0.08f, 0),
                                    new Vector3(0.05f, 0.096f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.092f - 0.08f, 0),
                                    new Vector3(0.05f, 0.092f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.088f - 0.08f, 0),
                                    new Vector3(0.05f, 0.088f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.084f - 0.08f, 0),
                                    new Vector3(0.05f, 0.084f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.08f - 0.08f, 0),
                                    new Vector3(0.05f, 0.08f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.076f - 0.08f, 0),
                                    new Vector3(0.05f, 0.076f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.072f - 0.08f, 0),
                                    new Vector3(0.05f, 0.072f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.068f - 0.08f, 0),
                                    new Vector3(0.05f, 0.068f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.064f - 0.08f, 0),
                                    new Vector3(0.05f, 0.064f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.060f - 0.08f, 0),
                                    new Vector3(0.05f, 0.060f - 0.08f, 0),
                                    new Vector3(-0.05f, 0.056f - 0.08f, 0),
                                    new Vector3(0.05f, 0.056f - 0.08f, 0),
                                };
                                int packIndex = 0;
                                foreach (Transform child in cardBoxInnerMesh)
                                {
                                    if (child.name.StartsWith("SpawnCardPackPosLoc"))
                                    {
                                        if (packIndex < packPositions.Length)
                                        {
                                            child.localPosition = packPositions[packIndex];
                                            child.localRotation = Quaternion.Euler(270, 0, 0);
                                            child.localScale = new Vector3(1f, 1f, 1f);
                                            packIndex++;
                                        }
                                    }
                                    else if (child.name.StartsWith("CardBoxStaticMesh"))
                                    {
                                        child.localPosition = new Vector3(-0.001f, 0.1107f - 0.08f, 0);
                                        child.localRotation = Quaternion.Euler(0, 180, 0);
                                        child.localScale = new Vector3(0.15f, 0.15f, 0.0001f);
                                        Dictionary<EItemType, string> texturePaths = new Dictionary<EItemType, string>
                                        {
                                            { EItemType.BasicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1.png") },
                                            { EItemType.RareCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2.png") },
                                            { EItemType.EpicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3.png") },
                                            { EnumExtensions.SafeParseEItemType("BoosterStellar"), Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S4.png") },
                                            { EItemType.LegendaryCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS.png") },
                                            { EItemType.DestinyBasicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1_TauxDrop.png") },
                                            { EItemType.DestinyRareCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2_TauxDrop.png") },
                                            { EItemType.DestinyEpicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3_TauxDrop.png") },
                                            { EnumExtensions.SafeParseEItemType("BoosterStellarTaux"), Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S4_TauxDrop.png") },
                                            { EItemType.DestinyLegendaryCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS_TauxDrop.png") },
                                        };

                                        // Vérifie si le cardPack a une texture associée dans le dictionnaire
                                        if (texturePaths.TryGetValue(cardPack, out string texturePath))
                                        {
                                            //Plugin.Logger.LogError($"{cardPack} détecté, application d'une nouvelle texture.");
                                            ApplyTextureToChild(child, texturePath);
                                        }

                                        // Coordonnées UV pour les coins du rectangle
                                        Vector2 uvTopLeft = new Vector2(0.029f, 0.509f);       // Haut gauche
                                        Vector2 uvBottomRight = new Vector2(0.490f, 0.099f);   // Bas droite
                                        ChangeTextureColorInUVRectangle(child.gameObject, uvTopLeft, uvBottomRight, Color.white);
                                        StartChildAnimation(child);
                                    }
                                    else if (child.name.StartsWith("StaticMesh"))
                                    {
                                        child.localPosition = new Vector3(-0.001f, 0.08f - 0.08f, 0);
                                        child.localRotation = Quaternion.Euler(90, 180, 0);
                                        child.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                                        Dictionary<EItemType, string> texturePaths = new Dictionary<EItemType, string>
                                        {
                                            { EItemType.BasicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1.png") },
                                            { EItemType.RareCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2.png") },
                                            { EItemType.EpicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3.png") },
                                            { EnumExtensions.SafeParseEItemType("BoosterStellar"), Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S4.png") },
                                            { EItemType.LegendaryCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS.png") },
                                            { EItemType.DestinyBasicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1_TauxDrop.png") },
                                            { EItemType.DestinyRareCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2_TauxDrop.png") },
                                            { EItemType.DestinyEpicCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3_TauxDrop.png") },
                                            { EnumExtensions.SafeParseEItemType("BoosterStellarTaux"), Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S4_TauxDrop.png") },
                                            { EItemType.DestinyLegendaryCardPack, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS_TauxDrop.png") },
                                        };

                                        // Vérifie si le cardPack a une texture associée dans le dictionnaire
                                        if (texturePaths.TryGetValue(cardPack, out string texturePath))
                                        {
                                            //Plugin.Logger.LogError($"{cardPack} détecté, application d'une nouvelle texture.");
                                            ApplyTextureToChild(child, texturePath);
                                        }
                                    }
                                }
                            }

                            obj.SetMesh(itemMeshData2.mesh, itemMeshData2.material, cardPack, itemMeshData2.meshSecondary, itemMeshData2.materialSecondary);
                            obj.transform.position = openCardBoxSpawnCardPackPosList[index].position;
                            obj.transform.rotation = openCardBoxSpawnCardPackPosList[index].rotation;
                            obj.transform.parent = openCardBoxSpawnCardPackPosList[index];
                            obj.transform.localScale = openCardBoxSpawnCardPackPosList[index].localScale;
                            obj.gameObject.SetActive(true);

                            m_HoldItemList[index] = obj; // Remplacer l'élément fictif par le véritable objet
                            CPlayerData.m_HoldItemTypeList.Add(obj.GetItemType());

                            var delayLerpMethod = AccessTools.Method(__instance.GetType(), "DelayLerpSpawnedCardPackToHand");
                            var enumerator = (IEnumerator)delayLerpMethod.Invoke(__instance, new object[] { index, (float)(1.25 + 0.05 * index), obj, holdCardPackPosList[index], holdItem });
                            DelayLerpSpawnedCardPackToHandPostfix(__instance, index, (1.25f + 0.05f * (float)index), obj, holdCardPackPosList[index], holdItem);
                        }
                    }
                }
            }

            return false;
        }

        public static void DelayLerpSpawnedCardPackToHandPostfix(InteractionPlayerController __instance, int index, float waitTime, Item item, Transform targetTransform, Item cardBoxItem)
        {
            __instance.StartCoroutine(HandleDelayLerpSpawnedCardPackToHand(__instance, index, waitTime, item, targetTransform, cardBoxItem));
        }

        private static IEnumerator HandleDelayLerpSpawnedCardPackToHand(InteractionPlayerController __instance, int index, float waitTime, Item item, Transform targetTransform, Item cardBoxItem)
        {
            var innerMeshField = AccessTools.Field(__instance.GetType(), "m_OpenCardBoxInnerMesh");
            var m_OpenCardBoxInnerMesh = innerMeshField.GetValue(__instance) as Animation;
            var isOpeningField = AccessTools.Field(__instance.GetType(), "m_IsOpeningCardBox");

            yield return new WaitForSeconds(waitTime);
            item.SmoothLerpToTransform(targetTransform, targetTransform);

            if (index == 23 && cardBoxItem != null)
            {
                yield return new WaitForSeconds(0.5f);
                cardBoxItem.DisableItem();
                if (m_OpenCardBoxInnerMesh != null)
                {
                    m_OpenCardBoxInnerMesh.gameObject.SetActive(false);
                }
                isOpeningField.SetValue(__instance, false);
            }
        }

        private static AnimationOpeningDisplay animationDisplay;
        public static void InitializeAnimationDisplay()
        {
            GameObject animationDisplayObject = new GameObject("AnimationOpeningDisplay");
            animationDisplay = animationDisplayObject.AddComponent<AnimationOpeningDisplay>();
        }

        public static void StartChildAnimation(Transform child)
        {
            if (animationDisplay == null)
            {
                InitializeAnimationDisplay();
            }
            animationDisplay.StartAnimation(child);
        }
        static void ChangeTextureColorInUVRectangle(GameObject targetObject, Vector2 uvTopLeft, Vector2 uvBottomRight, Color newColor)
        {
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                Texture2D texture = material.mainTexture as Texture2D;

                if (texture != null)
                {
                    // Rendre la texture modifiable
                    Texture2D readableTexture = TextureUtils.MakeTextureReadable(texture);
                    if (readableTexture != null)
                    {
                        // Convertir les coordonnées UV en coordonnées de pixels
                        int pixelXMin = Mathf.FloorToInt(uvTopLeft.x * readableTexture.width);
                        int pixelYMin = Mathf.FloorToInt(uvBottomRight.y * readableTexture.height);
                        int pixelXMax = Mathf.FloorToInt(uvBottomRight.x * readableTexture.width);
                        int pixelYMax = Mathf.FloorToInt(uvTopLeft.y * readableTexture.height);

                        // Boucle sur les pixels compris dans le rectangle
                        for (int y = pixelYMin; y <= pixelYMax; y++)
                        {
                            for (int x = pixelXMin; x <= pixelXMax; x++)
                            {
                                // Modifier la couleur de chaque pixel dans la zone
                                readableTexture.SetPixel(x, y, newColor);
                            }
                        }

                        // Appliquer les changements à la texture
                        readableTexture.Apply();

                        // Réaffecter la texture modifiée au matériau
                        material.mainTexture = readableTexture;

                        //Debug.Log($"Texture modifiée pour la zone UV ({uvTopLeft}, {uvBottomRight}) sur l'objet {targetObject.name}");
                    }
                    else
                    {
                        Debug.LogError("Impossible de rendre la texture lisible.");
                    }
                }
                else
                {
                    Debug.LogError("Le GameObject n'a pas de Texture2D assignée.");
                }
            }
            else
            {
                Debug.LogError("Le GameObject n'a pas de Renderer.");
            }
        }
        public static Texture2D LoadTexture(string path)
        {
            // Exemple de chargement d'une texture à partir d'un fichier (ajuste selon ton projet)
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
                return tex;
            return null;
        }
        public static void ApplyTextureToChild(Transform child, string texturePath)
        {
            Texture2D newTexture = LoadTexture(texturePath);

            if (newTexture != null)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Réutilise un matériau existant si possible
                    Material material = new Material(Shader.Find("Standard"));
                    material.mainTexture = newTexture;
                    renderer.material = material;
                }
                else
                {
                    Plugin.Logger.LogError("Impossible de récupérer le Renderer.");
                }
            }
            else
            {
                Plugin.Logger.LogError("Échec du chargement de la texture.");
            }
        }
    }
}