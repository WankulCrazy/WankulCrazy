using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WankulCrazyPlugin.cards;
using UnityEngine;
using WankulCrazyPlugin.utils;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using I2.Loc;
using static UnityEngine.UIElements.UIR.GradientSettingsAtlas;
using WankulCrazyPlugin.utils.obj;

namespace WankulCrazyPlugin.importer
{
    public class CustomItemsImporter
    {
        public static List<ItemMeshData> ItemMeshDataList;
        public static List<ItemData> ItemDataList;
        public static List<RestockData> RestockDataList;
        private static bool isImported = false;

        public static void ImportCustomItems()
        {
            if (isImported)
            {
                Plugin.Logger.LogInfo("Custom items already imported");
                return;
            }

            ItemDataList = DeserializeItemDataListJson();
            RestockDataList = DeserializeRestockDataListJson();
            ItemMeshDataList = DeserializeItemMeshDataList();

            InventoryBase.Instance.m_StockItemData_SO.m_ItemDataList.AddRange(ItemDataList);
            InventoryBase.Instance.m_StockItemData_SO.m_RestockDataList.AddRange(RestockDataList);
            InventoryBase.Instance.m_StockItemData_SO.m_ItemMeshDataList.AddRange(ItemMeshDataList);

            //foreach (var item in EnumExtensions.customEnumValues[typeof(EItemType)])
            //{
            //    InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType(item.Value));
            //}

            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("BoosterStellar"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("DisplayStellar"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("BoosterStellarTaux"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("DisplayStellarTaux"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("StarterApocalypse"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownItemType.Add(EnumExtensions.SafeParseEItemType("StarterShowtime"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownFigurineItemType.Add(EnumExtensions.SafeParseEItemType("CaleconStellar"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownAccessoryItemType.Add(EnumExtensions.SafeParseEItemType("TapisS41"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownAccessoryItemType.Add(EnumExtensions.SafeParseEItemType("TapisS42"));
            InventoryBase.Instance.m_StockItemData_SO.m_ShownAccessoryItemType.Add(EnumExtensions.SafeParseEItemType("ClasseurS4"));

            isImported = true;
        }

        public static List<ItemData> DeserializeItemDataListJson()
        {
            List<ItemData> itemDataList = new List<ItemData>();
            string pluginPath = Plugin.GetPluginPath();
            string customItemsPath = Path.Combine(pluginPath, "data/customitems");
            string itemDataListPath = Path.Combine(pluginPath, customItemsPath, "ItemDataList.json");
            string jsonContent = File.ReadAllText(itemDataListPath);

            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);

                foreach (JObject itemDataJson in jsonArray)
                {
                    ItemData itemData = new ItemData();
                    EItemType itemType = EnumExtensions.SafeParseEItemType(itemDataJson.GetValue("itemType").Value<string>());
                    itemData.name = itemDataJson.GetValue("name").Value<string>();
                    itemData.category = (EItemCategory)Enum.Parse(typeof(EItemCategory), itemDataJson.GetValue("category").Value<string>());
                    itemData.iconScale = itemDataJson.GetValue("iconScale").Value<float>();
                    itemData.baseCost = itemDataJson.GetValue("baseCost").Value<float>();
                    itemData.marketPriceMinPercent = itemDataJson.GetValue("marketPriceMinPercent").Value<float>();
                    itemData.marketPriceMaxPercent = itemDataJson.GetValue("marketPriceMaxPercent").Value<float>();
                    itemData.boxFollowItemPrice = EnumExtensions.SafeParseEItemType(itemDataJson.GetValue("boxFollowItemPrice").Value<string>());
                    itemData.isNotBoosterPack = itemDataJson.GetValue("isNotBoosterPack").Value<bool>();
                    itemData.isTallItem = itemDataJson.GetValue("isTallItem").Value<bool>();
                    itemData.isHideItemUntilUnlocked = itemDataJson.GetValue("isHideItemUntilUnlocked").Value<bool>();
                    itemData.posYOffsetInBox = itemDataJson.GetValue("posYOffsetInBox").Value<float>();
                    itemData.scaleOffsetInBox = itemDataJson.GetValue("scaleOffsetInBox").Value<float>();

                    // Parsing affectedPriceChangeType as List<EPriceChangeType>
                    JArray affectedPriceChangeTypeArray = itemDataJson.GetValue("affectedPriceChangeType").Value<JArray>();
                    List<EPriceChangeType> affectedPriceChangeTypeList = new List<EPriceChangeType>();
                    foreach (var type in affectedPriceChangeTypeArray)
                    {
                        EPriceChangeType priceChangeType = (EPriceChangeType)Enum.Parse(typeof(EPriceChangeType), type.Value<string>());
                        affectedPriceChangeTypeList.Add(priceChangeType);
                    }
                    itemData.affectedPriceChangeType = affectedPriceChangeTypeList;

                    JObject itemDimensionJson = itemDataJson.GetValue("itemDimension").Value<JObject>();
                    itemData.itemDimension = new Vector3(
                        itemDimensionJson.GetValue("x").Value<float>(),
                        itemDimensionJson.GetValue("y").Value<float>(),
                        itemDimensionJson.GetValue("z").Value<float>()
                    );
                    JObject colliderPosOffsetJson = itemDataJson.GetValue("colliderPosOffset").Value<JObject>();
                    itemData.colliderPosOffset = new Vector3(
                        colliderPosOffsetJson.GetValue("x").Value<float>(),
                        colliderPosOffsetJson.GetValue("y").Value<float>(),
                        colliderPosOffsetJson.GetValue("z").Value<float>()
                    );
                    JObject colliderScaleJson = itemDataJson.GetValue("colliderScale").Value<JObject>();
                    itemData.colliderScale = new Vector3(
                        colliderScaleJson.GetValue("x").Value<float>(),
                        colliderScaleJson.GetValue("y").Value<float>(),
                        colliderScaleJson.GetValue("z").Value<float>()
                    );

                    string iconRelativePath = itemDataJson.GetValue("icon").Value<string>();
                    string iconPath = Path.Combine(customItemsPath, "icons" , iconRelativePath);
                    if (File.Exists(iconPath))
                    {
                        Texture2D iconTexture = TextureUtils.LoadTexture(iconPath);
                        Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.zero);
                        iconSprite.name = itemData.name + "_icon";
                        itemData.icon = iconSprite;
                    }
                    else
                    {
                        Plugin.Logger.LogError("Icon file not found: " + iconPath);
                    }
                    //InventoryBase.Instance.m_StockItemData_SO.m_ItemDataList.Insert((int)itemType, itemData);
                    itemDataList.Add(itemData);
                }

            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError("Failed to deserialize JSON ItemData: " + ex.Message);
            }

            return itemDataList;
        }

        public static List<RestockData> DeserializeRestockDataListJson()
        {
            List<RestockData> restockDataList = new List<RestockData>();
            string pluginPath = Plugin.GetPluginPath();
            string customItemsPath = Path.Combine(pluginPath, "data/customitems");
            string restockDataListPath = Path.Combine(pluginPath, customItemsPath, "restockDataList.json");
            string jsonContent = File.ReadAllText(restockDataListPath);

            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);

                foreach (JObject restockDataJson in jsonArray)
                {
                    RestockData restockData = new RestockData();
                    restockData.index = restockDataJson.GetValue("index").Value<int>();
                    restockData.name = restockDataJson.GetValue("name").Value<string>();
                    restockData.isBigBox = restockDataJson.GetValue("isBigBox").Value<bool>();
                    restockData.ignoreDoubleImage = restockDataJson.GetValue("ignoreDoubleImage").Value<bool>();
                    restockData.amount = restockDataJson.GetValue("amount").Value<int>();
                    restockData.licenseShopLevelRequired = restockDataJson.GetValue("licenseShopLevelRequired").Value<int>();
                    restockData.licensePrice = restockDataJson.GetValue("licensePrice").Value<float>();
                    restockData.itemType = EnumExtensions.SafeParseEItemType(restockDataJson.GetValue("itemType").Value<string>());
                    restockData.prologueShow = restockDataJson.GetValue("prologueShow").Value<bool>();
                    restockData.isHideItemUntilUnlocked = restockDataJson.GetValue("isHideItemUntilUnlocked").Value<bool>();

                    restockDataList.Add(restockData);
                }

            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError("Failed to deserialize JSON RestockData: " + ex.Message);
            }

            return restockDataList;
        }

        public static List<ItemMeshData> DeserializeItemMeshDataList()
        {
            List<ItemMeshData> itemMeshDataList = new List<ItemMeshData>();
            string pluginPath = Plugin.GetPluginPath();
            string customItemsPath = Path.Combine(pluginPath, "data/customitems");
            string itemMeshDataListPath = Path.Combine(pluginPath, customItemsPath, "itemMeshDataList.json");
            string jsonContent = File.ReadAllText(itemMeshDataListPath);
            string copyItemTypestring = "rien";

            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);

                foreach (JObject itemMeshDataJson in jsonArray)
                {
                    ItemMeshData itemMeshData = new ItemMeshData();
                    EItemType itemType = EnumExtensions.SafeParseEItemType(itemMeshDataJson.GetValue("itemType").Value<string>());

                    string importType = itemMeshDataJson.GetValue("importType").Value<string>();
                    switch (importType)
                    {
                        case "CopyItem":
                            {
                                itemMeshData.name = itemMeshDataJson.GetValue("name").Value<string>();
                                EItemType copyItemType = EnumExtensions.SafeParseEItemType(itemMeshDataJson.GetValue("copyItemType").Value<string>());
                                copyItemTypestring = itemMeshDataJson.GetValue("copyItemType").Value<string>();
                                ItemMeshData sourceItem = InventoryBase.GetItemMeshData(copyItemType);

                                itemMeshData.mesh = sourceItem.mesh;

                                string textureRelativePath = itemMeshDataJson.GetValue("texture").Value<string>();
                                string texturePath = Path.Combine(customItemsPath, "textures", textureRelativePath);
                                if (File.Exists(texturePath))
                                {
                                    Texture2D texture = TextureUtils.LoadTexture(texturePath);
                                    Material newMaterial = new Material(Shader.Find("Standard"));
                                    newMaterial.mainTexture = texture;
                                    itemMeshData.material = newMaterial;
                                }
                                else
                                {
                                    Plugin.Logger.LogError("Texture file not found: " + texturePath);
                                }

                                itemMeshData.meshSecondary = sourceItem.meshSecondary;
                                itemMeshData.materialSecondary = sourceItem.materialSecondary;
                            }
                            break;
                        case "ImportObj":
                            {
                                itemMeshData.name = itemMeshDataJson.GetValue("name").Value<string>();
                                float scale = itemMeshDataJson.GetValue("scale").Value<float>();

                                string objRelativePath = itemMeshDataJson.GetValue("obj").Value<string>();
                                string objPath = Path.Combine(customItemsPath, "meshes", objRelativePath);
                                OBJImporter.tempmesh = new OBJLoader().Load(objPath);
                                OBJImporter.tempmesh.name = itemMeshData.name;
                                if ((UnityEngine.Object)OBJImporter.tempmesh != (UnityEngine.Object)null)
                                {
                                    try
                                    {
                                        List<Mesh> meshList = new List<Mesh>();
                                        foreach (Component component in OBJImporter.tempmesh.transform)
                                        {
                                            Mesh mesh = component.gameObject.GetComponent<MeshFilter>().mesh;
                                            Vector3[] vertices = mesh.vertices;
                                            for (int index = 0; index < vertices.Length; ++index)
                                                vertices[index].z = -vertices[index].z;
                                            int[] triangles = mesh.triangles;
                                            for (int index = 0; index < triangles.Length; index += 3)
                                            {
                                                int num = triangles[index];
                                                triangles[index] = triangles[index + 2];
                                                triangles[index + 2] = num;
                                            }
                                            mesh.vertices = vertices;
                                            mesh.triangles = triangles;
                                            meshList.Add(mesh);
                                        }
                                        CombineInstance[] combine = new CombineInstance[meshList.Count];
                                        for (int index = 0; index < meshList.Count; ++index)
                                        {
                                            combine[index].mesh = meshList[index];
                                            combine[index].transform = Matrix4x4.identity;
                                        }
                                        Mesh mesh1 = new Mesh();
                                        mesh1.CombineMeshes(combine, false);
                                        mesh1.name = itemMeshData.name;
                                        itemMeshData.mesh = mesh1;
                                    }
                                    catch
                                    {
                                    }
                                }
                                OBJImporter.tempmesh.gameObject.SetActive(false);

                                string textureRelativePath = itemMeshDataJson.GetValue("texture").Value<string>();
                                string texturePath = Path.Combine(customItemsPath, "textures", textureRelativePath);
                                if (File.Exists(texturePath))
                                {
                                    Texture2D texture = TextureUtils.LoadTexture(texturePath);
                                    Material newMaterial = new Material(Shader.Find("Standard"));
                                    newMaterial.mainTexture = texture;
                                    itemMeshData.material = newMaterial;
                                }
                                else
                                {
                                    Plugin.Logger.LogError("Texture file not found: " + texturePath);
                                }
                            }
                            break;
                        default:
                            Plugin.Logger.LogError("Unknown importType: " + importType);
                            break;
                    }

                    //InventoryBase.Instance.m_StockItemData_SO.m_ItemMeshDataList.Insert((int)itemType, itemMeshData);
                    itemMeshDataList.Add(itemMeshData);
                }

            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError("Failed to deserialize JSON ItemMeshData: " + ex.Message);
                Plugin.Logger.LogError("Failed to deserialize JSON ItemMeshData: " + copyItemTypestring);
            }

            return itemMeshDataList;
        }

        public static bool ItemTypeToCollectionPackType(EItemType itemType, ref ECollectionPackType __result)
        {
            // Récupère les valeurs dynamiques
            EItemType boosterStellar = EnumExtensions.SafeParseEItemType("BoosterStellar");
            EItemType displayStellar = EnumExtensions.SafeParseEItemType("DisplayStellar");
            EItemType boosterStellarTaux = EnumExtensions.SafeParseEItemType("BoosterStellarTaux");
            EItemType displayStellarTaux = EnumExtensions.SafeParseEItemType("DisplayStellarTaux");
            EItemType boosterGoldBattle = EnumExtensions.SafeParseEItemType("BoosterGoldBattle");
            EItemType boosterGoldStellar = EnumExtensions.SafeParseEItemType("BoosterGoldStellar");
            ECollectionPackType stellarPack = EnumExtensions.SafeParseECollectionPackType("Stellar");
            ECollectionPackType stellarPackTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");

            if (itemType == EItemType.BasicCardPack || itemType == EItemType.BasicCardBox)
                __result = ECollectionPackType.BasicCardPack;
            else if (itemType == EItemType.RareCardPack || itemType == EItemType.RareCardBox)
                __result = ECollectionPackType.RareCardPack;
            else if (itemType == EItemType.EpicCardPack || itemType == EItemType.EpicCardBox || itemType == boosterGoldBattle)
                __result = ECollectionPackType.EpicCardPack;
            else if (itemType == EItemType.LegendaryCardPack || itemType == EItemType.LegendaryCardBox)
                __result = ECollectionPackType.LegendaryCardPack;
            else if (itemType == EItemType.DestinyBasicCardPack || itemType == EItemType.DestinyBasicCardBox)
                __result = ECollectionPackType.DestinyBasicCardPack;
            else if (itemType == EItemType.DestinyRareCardPack || itemType == EItemType.DestinyRareCardBox)
                __result = ECollectionPackType.DestinyRareCardPack;
            else if (itemType == EItemType.DestinyEpicCardPack || itemType == EItemType.DestinyEpicCardBox || itemType == boosterGoldBattle)
                __result = ECollectionPackType.DestinyEpicCardPack;
            else if (itemType == EItemType.DestinyLegendaryCardPack || itemType == EItemType.DestinyLegendaryCardBox)
                __result = ECollectionPackType.DestinyLegendaryCardPack;
            else if (itemType == EItemType.GhostPack)
                __result = ECollectionPackType.GhostPack;
            else if (itemType == EItemType.MegabotPack)
                __result = ECollectionPackType.MegabotPack;
            else if (itemType == EItemType.FantasyRPGPack)
                __result = ECollectionPackType.FantasyRPGPack;
            else if (itemType == EItemType.CatJobPack)
                __result = ECollectionPackType.CatJobPack;
            else if (itemType == boosterStellar || itemType == displayStellar || itemType == boosterGoldStellar)
                __result = stellarPack;
            else if (itemType == boosterStellarTaux || itemType == displayStellarTaux || itemType == boosterGoldStellar)
                __result = stellarPackTaux;
            else
                __result = ECollectionPackType.None;

            return false;
        }

        public static bool GetCardExpansionType(ECollectionPackType collectionPackType, ref ECardExpansionType __result)
        {
            // Récupère la valeur dynamique pour Stellar
            ECollectionPackType stellarPack = EnumExtensions.SafeParseECollectionPackType("Stellar");
            ECollectionPackType stellarPackTaux = EnumExtensions.SafeParseECollectionPackType("StellarTaux");

            if (collectionPackType == ECollectionPackType.BasicCardPack ||
                collectionPackType == ECollectionPackType.RareCardPack ||
                collectionPackType == ECollectionPackType.EpicCardPack ||
                collectionPackType == ECollectionPackType.LegendaryCardPack ||
                collectionPackType == stellarPackTaux ||
                collectionPackType == stellarPack)
            {
                __result = ECardExpansionType.Tetramon;
            }
            else if (collectionPackType == ECollectionPackType.DestinyBasicCardPack ||
                     collectionPackType == ECollectionPackType.DestinyRareCardPack ||
                     collectionPackType == ECollectionPackType.DestinyEpicCardPack ||
                     collectionPackType == ECollectionPackType.DestinyLegendaryCardPack)
            {
                __result = ECardExpansionType.Destiny;
            }
            else if (collectionPackType == ECollectionPackType.GhostPack)
                __result = ECardExpansionType.Ghost;
            else if (collectionPackType == ECollectionPackType.MegabotPack)
                __result = ECardExpansionType.Megabot;
            else if (collectionPackType == ECollectionPackType.FantasyRPGPack)
                __result = ECardExpansionType.FantasyRPG;
            else if (collectionPackType == ECollectionPackType.CatJobPack)
                __result = ECardExpansionType.CatJob;
            else
                __result = ECardExpansionType.None;

            return false;
        }
    }
}
