using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using WankulCrazyPlugin.patch;
using UnityEngine;
using WankulCrazyPlugin.importer;
using BepInEx.Configuration;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.SceneManagement;
using WankulCrazyPlugin.objmod;

namespace WankulCrazyPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static Plugin Instance;
    private static string path_mes = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_meshes/");
    private static string path_nam = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_data/");
    private static MeshFilter[] mesh_list = new MeshFilter[0];
    private static Material[] mat_list = new Material[0];
    private static Image[] image_list = new Image[0];
    private static bool init_inTitle = false;
    private static bool init_inGame = false;
    private static bool init_filePaths = false;
    public static Dictionary<string, string> filePaths_obj = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
    public static GameObject tempmesh = new GameObject((string)null);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        MethodInfo original_OnLevelFinishedLoading = AccessTools.Method(typeof(CGameManager), "OnLevelFinishedLoading");
        MethodInfo patch_OnLevelFinishedLoading = AccessTools.Method(typeof(GameStarting), "OnLevelFinishedLoading");
        harmony.Patch(original_OnLevelFinishedLoading, postfix: new HarmonyMethod(patch_OnLevelFinishedLoading));

        MethodInfo original_setcardui = AccessTools.Method(typeof(CardUI), "SetCardUI");
        MethodInfo patch_setcarduiprefix = AccessTools.Method(typeof(ReplacingCards), "SetCardUIPrefix");
        MethodInfo patch_setcarduipostfix = AccessTools.Method(typeof(ReplacingCards), "SetCardUIPostFix");
        harmony.Patch(original_setcardui, prefix: new HarmonyMethod(patch_setcarduiprefix), postfix: new HarmonyMethod(patch_setcarduipostfix));

        MethodInfo original_closeup = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "EnterViewUpCloseState");
        MethodInfo patch_closeup_prefix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePrefix");
        MethodInfo patch_closeup_postfix = AccessTools.Method(typeof(ReplacingCards), "EnterViewUpCloseStatePostfix");
        harmony.Patch(original_closeup, prefix: new HarmonyMethod(patch_closeup_prefix), postfix: new HarmonyMethod(patch_closeup_postfix));

        MethodInfo original_CardOpening = AccessTools.Method(typeof(CardOpeningSequence), "GetPackContent");
        MethodInfo patch_CardOpening = AccessTools.Method(typeof(CardOpening), "OpenBooster");
        harmony.Patch(original_CardOpening, postfix: new HarmonyMethod(patch_CardOpening));

        MethodInfo original_Update = AccessTools.Method(typeof(CardOpeningSequence), "Update");
        MethodInfo patch_Update = AccessTools.Method(typeof(CardOpening), "UpdatePostFix");
        harmony.Patch(original_Update, postfix: new HarmonyMethod(patch_Update));

        MethodInfo original_binderSetCard = AccessTools.Method(typeof(CollectionBinderFlipAnimCtrl), "UpdateBinderAllCardUI");
        MethodInfo patch_binderSetCard = AccessTools.Method(typeof(SortUI), "UpdateBinderAllCardUI");
        harmony.Patch(original_binderSetCard, postfix: new HarmonyMethod(patch_binderSetCard));

        MethodInfo original_save = AccessTools.Method(typeof(CSaveLoad), "Save");
        MethodInfo patch_save = AccessTools.Method(typeof(Saves), "Save");
        harmony.Patch(original_save, postfix: new HarmonyMethod(patch_save));

        MethodInfo original_load = AccessTools.Method(typeof(CSaveLoad), "Load");
        MethodInfo patch_load = AccessTools.Method(typeof(Saves), "Load");
        harmony.Patch(original_load, postfix: new HarmonyMethod(patch_load));

        // Récupère la méthode originale à patcher en spécifiant les paramètres (ici sans paramètres)
        MethodInfo originalMethod1 = AccessTools.Method(typeof(CPlayerData), "GetCardMarketPrice", new[] { typeof(CardData) });
        MethodInfo patchMethod1 = AccessTools.Method(typeof(CardPrice), nameof(CardPrice.Postfix_GetCardMarketPrice_CardData));
        harmony.Patch(originalMethod1, postfix: new HarmonyMethod(patchMethod1));

        MethodInfo originalMethod2 = AccessTools.Method(typeof(CPlayerData), "GetCardMarketPrice", new[] { typeof(int), typeof(ECardExpansionType), typeof(bool) });
        MethodInfo patchMethod2 = AccessTools.Method(typeof(CardPrice), nameof(CardPrice.Postfix_GetCardMarketPrice_ThreeParams));
        harmony.Patch(originalMethod2, postfix: new HarmonyMethod(patchMethod2));

        MethodInfo original_InitCardPhone = AccessTools.Method(typeof(CheckPricePanelUI), "InitCard");
        MethodInfo patch_InitCardPhone = AccessTools.Method(typeof(CheckPriceUI), "CheckPricePanelInitCard");
        harmony.Patch(original_InitCardPhone, prefix: new HarmonyMethod(patch_InitCardPhone));

        MethodInfo original_EvaluateItemPanelUI = AccessTools.Method(typeof(CheckPriceScreen), "EvaluateCardPanelUI");
        MethodInfo patch_EvaluateItemPanelUI = AccessTools.Method(typeof(CheckPriceUI), "EvaluateCardPanelUI");
        harmony.Patch(original_EvaluateItemPanelUI, prefix: new HarmonyMethod(patch_EvaluateItemPanelUI));

        MethodInfo original_OnPressOpenCardPriceGraph = AccessTools.Method(typeof(CheckPriceScreen), "OnPressOpenCardPriceGraph");
        MethodInfo patch_OnPressOpenCardPriceGraph = AccessTools.Method(typeof(CheckPriceUI), "OnPressOpenCardPriceGraph");
        harmony.Patch(original_OnPressOpenCardPriceGraph, prefix: new HarmonyMethod(patch_OnPressOpenCardPriceGraph));

        MethodInfo original_ShowCardPriceChart = AccessTools.Method(typeof(ItemPriceGraphScreen), "ShowCardPriceChart");
        MethodInfo patch_ShowCardPriceChart = AccessTools.Method(typeof(CheckPriceUI), "ShowCardPriceChart");
        harmony.Patch(original_ShowCardPriceChart, prefix: new HarmonyMethod(patch_ShowCardPriceChart));

        MethodInfo original_AddCard = AccessTools.Method(typeof(CPlayerData), "AddCard");
        MethodInfo patch_AddCard = AccessTools.Method(typeof(Inventory), "AddCard");
        harmony.Patch(original_AddCard, postfix: new HarmonyMethod(patch_AddCard));

        MethodInfo original_RemoveCard = AccessTools.Method(typeof(CPlayerData), "ReduceCard");
        MethodInfo patch_RemoveCard = AccessTools.Method(typeof(Inventory), "RemoveCard");
        harmony.Patch(original_RemoveCard, postfix: new HarmonyMethod(patch_RemoveCard));

        MethodInfo original_SetTotalValue = AccessTools.Method(typeof(CollectionBinderUI), "SetTotalValue");
        MethodInfo patch_SetCardPriceTotalValue = AccessTools.Method(typeof(Inventory), "SetCardPriceTotalValue");
        harmony.Patch(original_SetTotalValue, postfix: new HarmonyMethod(patch_SetCardPriceTotalValue));

        MethodInfo original_GetIcon = AccessTools.Method(typeof(MonsterData), "GetIcon");
        MethodInfo patch_GetIcon = AccessTools.Method(typeof(ReplacingCards), "GetIcon");
        harmony.Patch(original_GetIcon, prefix: new HarmonyMethod(patch_GetIcon));


        MethodInfo original_OpenSortAlbumScreen = AccessTools.Method(typeof(CollectionBinderUI), "OpenSortAlbumScreen");
        MethodInfo patch_OpenSortAlbumScreenPrefix = AccessTools.Method(typeof(SortUI), "OpenSortAlbumScreenPrefix");
        MethodInfo patch_OpenSortAlbumScreen = AccessTools.Method(typeof(SortUI), "OpenSortAlbumScreen");
        harmony.Patch(original_OpenSortAlbumScreen, postfix: new HarmonyMethod(patch_OpenSortAlbumScreen), prefix: new HarmonyMethod(patch_OpenSortAlbumScreenPrefix));





        MethodInfo original_ExpansionOpenScreen = AccessTools.Method(typeof(CardExpansionSelectScreen), "OpenScreen");
        MethodInfo patch_ExpansionOpenScreen = AccessTools.Method(typeof(ExpansionScreen), "OpenExpansionScreen");
        harmony.Patch(original_ExpansionOpenScreen, postfix: new HarmonyMethod(patch_ExpansionOpenScreen));

        MethodInfo original_ExpansionPressButton = AccessTools.Method(typeof(CardExpansionSelectScreen), "OnPressButton");
        MethodInfo patch_ExpansionPressButton = AccessTools.Method(typeof(ExpansionScreen), "OnExpansionPressButton");
        harmony.Patch(original_ExpansionPressButton, prefix: new HarmonyMethod(patch_ExpansionPressButton));

        MethodInfo original_CardExpansionUpdated = AccessTools.Method(typeof(WorkbenchUIScreen), "OnCardExpansionUpdated");
        MethodInfo patch_CardExpansionUpdated = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnCardExpansionUpdated");
        harmony.Patch(original_CardExpansionUpdated, postfix: new HarmonyMethod(patch_CardExpansionUpdated));

        MethodInfo original_RarityOpenScreen = AccessTools.Method(typeof(CardRaritySelectScreen), "OpenScreen");
        MethodInfo patch_RarityOpenScreen = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OpenRarityScreen");
        harmony.Patch(original_RarityOpenScreen, postfix: new HarmonyMethod(patch_RarityOpenScreen));

        MethodInfo original_RarityPressButton = AccessTools.Method(typeof(CardRaritySelectScreen), "OnPressButton");
        MethodInfo patch_RarityPressButton = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnRarityPressButton");
        harmony.Patch(original_RarityPressButton, prefix: new HarmonyMethod(patch_RarityPressButton));

        MethodInfo original_CardRarityUpdated = AccessTools.Method(typeof(WorkbenchUIScreen), "OnRarityLimitUpdated");
        MethodInfo patch_CardRarityUpdated = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OnRarityLimitUpdated");
        harmony.Patch(original_CardRarityUpdated, postfix: new HarmonyMethod(patch_CardRarityUpdated));

        MethodInfo original_WorkbenchOpenScreen = AccessTools.Method(typeof(WorkbenchUIScreen), "OpenScreen");
        MethodInfo patch_WorkbenchOpenScreen = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "OpenWorkBenchScreen");
        harmony.Patch(original_WorkbenchOpenScreen, postfix: new HarmonyMethod(patch_WorkbenchOpenScreen));

        MethodInfo original_RunBundleCardBulkFunction = AccessTools.Method(typeof(WorkbenchUIScreen), "RunBundleCardBulkFunction");
        MethodInfo patch_RunBundleCardBulkFunction = AccessTools.Method(typeof(patch.workbench.WorkbenchPatch), "RunBundleCardBulkFunction");
        harmony.Patch(original_RunBundleCardBulkFunction, prefix: new HarmonyMethod(patch_RunBundleCardBulkFunction));


        Plugin.Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);
    }

    public static string GetPluginPath()
    {
        return Path.Combine(Application.dataPath, "../BepInEx/plugins", PluginInfo.PLUGIN_NAME);
    }


    private void checkFolders()
    {
        if (!Directory.Exists(Plugin.path_mes))
            Directory.CreateDirectory(Plugin.path_mes);
        if (Directory.Exists(Plugin.path_nam))
            return;
        Directory.CreateDirectory(Plugin.path_nam);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Title" && !Plugin.init_filePaths)
        {
            this.checkFolders();
            try
            {
                string[] strArray = new string[1] { "*.obj" };
                foreach (string searchPattern in strArray)
                {
                    foreach (string file in Directory.GetFiles(Plugin.path_mes, searchPattern, SearchOption.AllDirectories))
                    {
                        string fileName = Path.GetFileName(file);
                        string directoryName = Path.GetDirectoryName(file);
                        if (!Plugin.filePaths_obj.ContainsKey(fileName))
                            Plugin.filePaths_obj.Add(fileName, directoryName + "/");
                    }
                }
            }
            catch
            {
            }
            Plugin.init_filePaths = true;
            this.CacheMeshesAtStart();
        }
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "Title" && !Plugin.init_inTitle)
        {
            Plugin.init_inTitle = true;
            Plugin.init_inGame = false;
            this.DoReplace();
        }
        activeScene = SceneManager.GetActiveScene();
        if (!(activeScene.name == "Start") || Plugin.init_inGame)
            return;
        Plugin.init_inGame = true;
        Plugin.init_inTitle = false;
        this.DoReplace();
    }


    private void CacheMeshesAtStart()
    {
        foreach (KeyValuePair<string, string> keyValuePair in Plugin.filePaths_obj)
        {
            string key = keyValuePair.Key.Replace(".obj", "");
            Plugin.tempmesh = new OBJLoader().Load(keyValuePair.Value + key + ".obj");
            Plugin.tempmesh.name = key;
            if ((UnityEngine.Object)Plugin.tempmesh != (UnityEngine.Object)null)
            {
                try
                {
                    List<Mesh> meshList = new List<Mesh>();
                    foreach (Component component in Plugin.tempmesh.transform)
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
                    mesh1.name = key;
                    Plugin.cachedMeshes[key] = mesh1;
                    Plugin.cachedMeshes[key].UploadMeshData(true);
                }
                catch
                {
                }
            }
        }
        Debug.Log((object)"Meshes cached at start");
    }

    private Mesh GetCachedMesh(string name)
    {
        Mesh mesh;
        return Plugin.cachedMeshes.TryGetValue(name, out mesh) ? mesh : (Mesh)null;
    }

    private void DoReplace()
    {
        Plugin.mesh_list = UnityEngine.Resources.FindObjectsOfTypeAll<MeshFilter>();
        if (Plugin.mesh_list.Length != 0)
        {
            foreach (MeshFilter mesh in Plugin.mesh_list)
            {
                if ((UnityEngine.Object)mesh != (UnityEngine.Object)null && (UnityEngine.Object)mesh.sharedMesh != (UnityEngine.Object)null)
                {
                    Mesh cachedMesh = this.GetCachedMesh(mesh.sharedMesh.name.Replace("'s", ""));
                    if ((UnityEngine.Object)cachedMesh != (UnityEngine.Object)null)
                    {
                        mesh.sharedMesh = cachedMesh;
                        mesh.sharedMesh.UploadMeshData(true);
                    }
                }
            }
            Debug.Log((object)"Custom 3D Objects loaded!");
        }
        this.ReplaceSpriteLists();
        Debug.Log((object)"Custom Textures loaded!");
    }

    private void ReplaceSpriteLists()
    {
        if (!(SceneManager.GetActiveScene().name == "Start"))
            return;
        foreach (List<ItemData> spriteList in new List<List<ItemData>>()
      {
        (List<ItemData>) CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemDataList
      })
            this.ReplaceItemDataInList(spriteList);
        foreach (List<ItemMeshData> spriteList in new List<List<ItemMeshData>>()
      {
        (List<ItemMeshData>) CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemMeshDataList
      })
            this.ReplaceItemDataMeshInList(spriteList);
    }

    private void ReplaceItemDataInList(List<ItemData> spriteList)
    {
        for (int index = 0; index < spriteList.Count; ++index)
        {
            Sprite icon = spriteList[index].icon;
            if ((UnityEngine.Object)icon != (UnityEngine.Object)null)
            {
                if (spriteList[index].name != "")
                {
                    //Debug.LogWarning("Nom : "+(spriteList[index].name));
                    if (File.Exists(Plugin.path_nam + "figurines/" + spriteList[index].name + "_NAME.txt"))
                    {
                        try
                        {
                            string[] strArray = File.ReadAllLines(Plugin.path_nam + "figurines/" + spriteList[index].name + "_NAME.txt");
                            spriteList[index].name = strArray[0];
                        }
                        catch
                        {
                        }
                    }
                    else if (File.Exists(Plugin.path_nam + "accessories/" + spriteList[index].name + "_NAME.txt"))
                    {
                        try
                        {
                            string[] strArray = File.ReadAllLines(Plugin.path_nam + "accessories/" + spriteList[index].name + "_NAME.txt");
                            spriteList[index].name = strArray[0];
                        }
                        catch
                        {
                        }
                    }
                    else if (File.Exists(Plugin.path_nam + "booster packs/" + spriteList[index].name + "_NAME.txt"))
                    {
                        try
                        {
                            string[] strArray = File.ReadAllLines(Plugin.path_nam + "booster packs/" + spriteList[index].name + "_NAME.txt");
                            spriteList[index].name = strArray[0];
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    private void ReplaceItemDataMeshInList(List<ItemMeshData> spriteList)
    {
        for (int index = 0; index < spriteList.Count; ++index)
        {
            Mesh mesh = spriteList[index].mesh;
            if ((UnityEngine.Object)mesh != (UnityEngine.Object)null)
            {
                Mesh cachedMesh = this.GetCachedMesh(mesh.name);
                if ((UnityEngine.Object)cachedMesh != (UnityEngine.Object)null)
                {
                    spriteList[index].mesh = cachedMesh;
                    spriteList[index].mesh.UploadMeshData(true);
                }
            }
        }
    }

}
