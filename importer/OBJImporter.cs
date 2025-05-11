using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WankulCrazyPlugin.utils.obj;
using WankulCrazyPlugin.patch;
using WankulCrazyPlugin;
using UnityEngine.UI;
using BepInEx;
using System.Linq;


namespace WankulCrazyPlugin.importer
{
    internal class OBJImporter
    {
        private static string path_mes = Path.Combine(Plugin.GetPluginPath(), "data", "meshes/");
        private static string path_nam = Path.Combine(Plugin.GetPluginPath(), "data", "names/");
        private static string path_spr = Path.Combine(Plugin.GetPluginPath(), "data", "sprites/");
        private static MeshFilter[] mesh_list = new MeshFilter[0];
        private static Material[] mat_list = new Material[0];
        private static Image[] image_list = new Image[0];
        public static Dictionary<string, string> filePaths_obj = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, string> filePaths_tex = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
        public static GameObject tempmesh = new GameObject((string)null);

        private static Dictionary<string, Mesh> vanilla_podium_meshes = new Dictionary<string, Mesh>();
        private static Dictionary<string, Texture> vanilla_podium_textures = new Dictionary<string, Texture>();
        private static readonly string[] podium_meshes = new string[29]
    {
      "BatD_Statue_Mesh",
      "Bear_AtkPose",
      "Beetle_AtkPose",
      "Beetle_AtkPose2",
      "BugA_AtkPose",
      "BugB_IdlePose",
      "BugC_IdlePose",
      "BugD_AtkPose",
      "EarthDragon_IdlePose",
      "Figurine_PigB_Mesh",
      "FireDragon_AtkPose",
      "FireWolfA_IdlePose",
      "FireWolfB_IdlePose",
      "FireWolfC_IdlePose",
      "FireWolfD_IdlePose",
      "FoxD_IdlePose",
      "GolemA_Mesh",
      "PiggyA_IdlePose",
      "PiggyC_IdlePose",
      "PiggyD_AtkPose",
      "PiggyD_IdlePose",
      "ShellfishD_IdlePose",
      "StarfishA_Mesh",
      "StarfishD_Mesh",
      "ThunderDragon_IdlePose",
      "TreeD_IdlePose",
      "WaterDragon_AtkPPose",
      "Wisp_AtkPose",
      "Wisp_IdlePose"
    };
        private static readonly string[] podium_textures = new string[27]
        {
      "T_BatD",
      "T_Bear",
      "T_Bear",
      "T_Beetle",
      "T_BugA",
      "T_BugB",
      "T_BugC",
      "T_BugD",
      "T_DragonEarth",
      "T_PiggyB",
      "T_DragonFire",
      "T_FireWolfA",
      "T_FireWolfB",
      "T_FireWolfC",
      "T_FireWolfD",
      "T_FoxD",
      "T_GolemA",
      "T_PiggyA",
      "T_PiggyC",
      "T_PiggyD",
      "T_ShellyD",
      "T_StarfishA",
      "T_StarfishD",
      "T_DragonThunder",
      "T_TreeD",
      "T_DragonWater",
      "T_Wisp"
        };


        private static void checkFolders()
        {
            if (!Directory.Exists(OBJImporter.path_mes))
                Directory.CreateDirectory(OBJImporter.path_mes);
            if (!Directory.Exists(OBJImporter.path_spr))
                Directory.CreateDirectory(OBJImporter.path_spr);
            if (Directory.Exists(OBJImporter.path_nam))
                return;
            Directory.CreateDirectory(OBJImporter.path_nam);
        }

        public static void InitFiles()
        {
            Plugin.Logger.LogInfo("Loading 3D Objects...");
            checkFolders();
            Plugin.Logger.LogInfo("Checking for .obj files...");
            try
            {
                string[] strArray = new string[2]
                {
            "*.png",
            "*.txt"
                };
                foreach (string searchPattern in strArray)
                {
                    foreach (string file in Directory.GetFiles(OBJImporter.path_spr, searchPattern, SearchOption.AllDirectories))
                    {
                        string fileName = Path.GetFileName(file);
                        string directoryName = Path.GetDirectoryName(file);
                        if (!OBJImporter.filePaths_tex.ContainsKey(fileName))
                            OBJImporter.filePaths_tex.Add(fileName, directoryName + "/");
                    }
                }
            }
            catch
            {
            }
            try
            {
                string[] strArray = new string[1] { "*.obj" };
                foreach (string searchPattern in strArray)
                {
                    foreach (string file in Directory.GetFiles(OBJImporter.path_mes, searchPattern, SearchOption.AllDirectories))
                    {
                        string fileName = Path.GetFileName(file);
                        string directoryName = Path.GetDirectoryName(file);
                        if (!OBJImporter.filePaths_obj.ContainsKey(fileName))
                            OBJImporter.filePaths_obj.Add(fileName, directoryName + "/");
                    }
                }
            }
            catch
            {
                Plugin.Logger.LogError("Error while loading .obj files");
            }
            CacheMeshesAtStart();
            CacheTexturesAtStart();
        }

        private static void CacheTexturesAtStart()
        {
            foreach (KeyValuePair<string, string> keyValuePair in OBJImporter.filePaths_tex)
            {
                string key = keyValuePair.Key.Replace(".png", "");
                Texture2D texture2D = !key.ToLower().EndsWith("_n") && !key.ToLower().EndsWith("_normal") && !key.ToLower().EndsWith(" n") ? OBJImporter.LoadPNG(keyValuePair.Value + key + ".png") : OBJImporter.LoadPNG_Bump(keyValuePair.Value + key + ".png");
                if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
                {
                    texture2D.Apply(true, true);
                    OBJImporter.cachedTextures[key] = texture2D;
                }
            }
            Debug.Log((object)"Textures cached at start");
        }

        private static Texture2D GetCachedTexture(string name)
        {
            Texture2D texture2D;
            return OBJImporter.cachedTextures.TryGetValue(name, out texture2D) ? texture2D : (Texture2D)null;
        }

        private static void CacheMeshesAtStart()
        {
            foreach (KeyValuePair<string, string> keyValuePair in OBJImporter.filePaths_obj)
            {
                string key = keyValuePair.Key.Replace(".obj", "");
                OBJImporter.tempmesh = new OBJLoader().Load(keyValuePair.Value + key + ".obj");
                OBJImporter.tempmesh.name = key;
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
                        mesh1.name = key;
                        OBJImporter.cachedMeshes[key] = mesh1;
                        OBJImporter.cachedMeshes[key].UploadMeshData(true);
                    }
                    catch
                    {
                    }
                }
            }
            Plugin.Logger.LogInfo("Meshes cached at start");
        }

        private static Mesh GetCachedMesh(string name)
        {
            Mesh mesh;
            return OBJImporter.cachedMeshes.TryGetValue(name, out mesh) ? mesh : (Mesh)null;
        }

        public static void DoReplace()
        {
            OBJImporter.mesh_list = UnityEngine.Resources.FindObjectsOfTypeAll<MeshFilter>();
            if (OBJImporter.mesh_list.Length != 0)
            {
                foreach (MeshFilter mesh in OBJImporter.mesh_list)
                {
                    if ((UnityEngine.Object)mesh != (UnityEngine.Object)null && (UnityEngine.Object)mesh.sharedMesh != (UnityEngine.Object)null)
                    {
                        Mesh cachedMesh = GetCachedMesh(mesh.sharedMesh.name.Replace("'s", ""));
                        if ((UnityEngine.Object)cachedMesh != (UnityEngine.Object)null)
                        {
                            mesh.sharedMesh = cachedMesh;
                            mesh.sharedMesh.UploadMeshData(true);
                        }
                    }
                }
                Plugin.Logger.LogInfo("Custom 3D Objects loaded!");
            }
            // Remplacement des textures podium
            var renderers = UnityEngine.Resources.FindObjectsOfTypeAll<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    var sharedMaterial = renderer.sharedMaterial;
                    var mainTex = sharedMaterial.mainTexture as Texture2D;
                    if (mainTex != null)
                    {
                        string texName = mainTex.name;

                        // Sauvegarde de la texture podium originale
                        if (OBJImporter.podium_textures.Contains(texName) && !OBJImporter.vanilla_podium_textures.ContainsKey(texName))
                        {
                            OBJImporter.vanilla_podium_textures.Add(texName, mainTex);
                        }

                        // Recherche de la texture custom
                        Texture2D cachedTexture = GetCachedTexture(texName);
                        if (cachedTexture == null && OBJImporter.podium_textures.Contains(texName))
                        {
                            string suffix = texName.Replace("T_", "");
                            string customTexPath = Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", $"MonsterStatue_{suffix}.png");

                            if (File.Exists(customTexPath))
                            {
                                cachedTexture = PatchTexturesImporter.LoadTexture2D(customTexPath);
                                if (cachedTexture != null)
                                {
                                    cachedTexture.name = texName;
                                    OBJImporter.cachedTextures[texName] = cachedTexture;
                                }
                                else
                                {
                                    Plugin.Logger.LogWarning($"Échec du chargement de la texture depuis : {customTexPath}");
                                }
                            }
                            else
                            {
                                Plugin.Logger.LogWarning($"Fichier introuvable : {customTexPath}");
                            }
                        }

                        // Application si la texture custom est chargée
                        if (cachedTexture != null)
                        {
                            // Sauvegarde si non encore faite
                            if (!OBJImporter.vanilla_podium_textures.ContainsKey(texName) && OBJImporter.podium_textures.Contains(texName))
                            {
                                OBJImporter.vanilla_podium_textures[texName] = mainTex;
                            }

                            // Création d'une texture clonée et remplacement de son contenu
                            Texture2D clonedTex = UnityEngine.Object.Instantiate(mainTex);
                            clonedTex.name = texName + "_Custom";

                            // Remplacement du contenu
                            clonedTex = PatchTexturesImporter.ReplaceTexture(clonedTex, cachedTexture);

                            // Duplication du matériau existant pour éviter de modifier les autres renderers
                            Material podiumMaterial = UnityEngine.Object.Instantiate(sharedMaterial);
                            podiumMaterial.name = sharedMaterial.name + "_PodiumCustom";

                            // Application de la nouvelle texture dans le matériau du renderer
                            podiumMaterial.mainTexture = clonedTex;
                            renderer.material = podiumMaterial;
                        }
                    }
                }
            }

            OBJImporter.image_list = UnityEngine.Resources.FindObjectsOfTypeAll<Image>();
            if (OBJImporter.image_list.Length != 0)
            {
                foreach (Image image in OBJImporter.image_list)
                {
                    if ((UnityEngine.Object)image != (UnityEngine.Object)null && (UnityEngine.Object)image.sprite != (UnityEngine.Object)null)
                    {
                        Texture2D cachedTexture = GetCachedTexture(image.sprite.name);
                        if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
                        {
                            Sprite sprite = OBJImporter.TextureToSprite(cachedTexture);
                            image.sprite = sprite;
                        }
                    }
                }
            }
            FixNewStatues();
            ReplaceSpriteLists();
            Plugin.Logger.LogInfo("Custom Textures loaded!");
        }

        private static void ReplaceSpriteLists()
        {
            if (!(SceneManager.GetActiveScene().name == "Start"))
                return;
            foreach (List<ItemData> spriteList in new List<List<ItemData>>()
      {
        (List<ItemData>) CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemDataList
      })
                ReplaceItemDataInList(spriteList);
            foreach (List<ItemMeshData> spriteList in new List<List<ItemMeshData>>()
      {
        (List<ItemMeshData>) CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemMeshDataList
      })
                ReplaceItemDataMeshInList(spriteList);
            foreach (List<EDecoObject> dataList in new List<List<EDecoObject>>()
      {
        CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_OtherDecoList
      })
                ReplaceDecoDataInList(dataList);
            foreach (List<EDecoObject> dataList in new List<List<EDecoObject>>()
      {
        CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_PosterDecoList
      })
                ReplaceDecoDataInList(dataList);
        }

        private static void ReplaceItemDataInList(List<ItemData> spriteList)
        {
            for (int index = 0; index < spriteList.Count; ++index)
            {
                Sprite icon = spriteList[index].icon;
                if ((UnityEngine.Object)icon != (UnityEngine.Object)null)
                {
                    if (spriteList[index].name != "")
                    {
                        //Debug.LogWarning("Nom : "+(spriteList[index].name));
                        if (File.Exists(OBJImporter.path_nam + "figurines/" + spriteList[index].name + "_NAME.txt"))
                        {
                            try
                            {
                                string[] strArray = File.ReadAllLines(OBJImporter.path_nam + "figurines/" + spriteList[index].name + "_NAME.txt");
                                spriteList[index].name = strArray[0];
                            }
                            catch
                            {
                            }
                        }
                        else if (File.Exists(OBJImporter.path_nam + "accessories/" + spriteList[index].name + "_NAME.txt"))
                        {
                            try
                            {
                                string[] strArray = File.ReadAllLines(OBJImporter.path_nam + "accessories/" + spriteList[index].name + "_NAME.txt");
                                spriteList[index].name = strArray[0];
                            }
                            catch
                            {
                            }
                        }
                        else if (File.Exists(OBJImporter.path_nam + "booster packs/" + spriteList[index].name + "_NAME.txt"))
                        {
                            try
                            {
                                string[] strArray = File.ReadAllLines(OBJImporter.path_nam + "booster packs/" + spriteList[index].name + "_NAME.txt");
                                spriteList[index].name = strArray[0];
                            }
                            catch
                            {
                            }
                        }
                        else if (File.Exists(OBJImporter.path_nam + "posters/" + spriteList[index].name + "_NAME.txt"))
                        {
                            try
                            {
                                string[] strArray = File.ReadAllLines(OBJImporter.path_nam + "posters/" + spriteList[index].name + "_NAME.txt");
                                spriteList[index].name = strArray[0];
                            }
                            catch
                            {
                            }
                        }
                    }
                    Texture2D cachedTexture = GetCachedTexture(icon.name);
                    if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
                    {
                        Sprite sprite = OBJImporter.TextureToSprite(cachedTexture);
                        spriteList[index].icon = sprite;
                    }
                }
            }
        }

        private static void ReplaceItemDataMeshInList(List<ItemMeshData> spriteList)
        {
            for (int index = 0; index < spriteList.Count; ++index)
            {
                Mesh mesh = spriteList[index].mesh;
                if ((UnityEngine.Object)mesh != (UnityEngine.Object)null)
                {
                    Mesh cachedMesh = GetCachedMesh(mesh.name);
                    if ((UnityEngine.Object)cachedMesh != (UnityEngine.Object)null)
                    {
                        spriteList[index].mesh = cachedMesh;
                        spriteList[index].mesh.UploadMeshData(true);
                    }
                }
            }
        }

        private static void ReplaceDecoDataInList(List<EDecoObject> dataList)
        {
            Dictionary<string, int> categoryCounter = new Dictionary<string, int>()
            {
                { "Statue", 1 },
                { "Plant", 1 },
                { "Sign", 1 },
                { "Poster", 1 }
            };

            for (int i = 0; i < dataList.Count; ++i)
            {
                DecoPurchaseData decoData = InventoryBase.GetItemDecoPurchaseData(dataList[i]);
                if (decoData != null)
                {
                    string category = "Other";

                    if (decoData.name.Contains("Statue"))
                        category = "Statue";
                    else if (decoData.name.Contains("Plant"))
                        category = "Plant";
                    else if (decoData.name.Contains("Sign"))
                        category = "Sign";
                    else if (decoData.name.Contains("Poster"))
                        category = "Poster";

                    string nameFile = $"{(categoryCounter.ContainsKey(category) ? categoryCounter[category] : 1)}_{decoData.name}_NAME.txt";
                    string nameFilePath = Path.Combine(path_nam, "posters", nameFile);

                    if (File.Exists(nameFilePath))
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(nameFilePath);
                            decoData.mainNameText = lines[0];
                        }
                        catch
                        {
                            Plugin.Logger.LogWarning($"Failed to load name from: {nameFilePath}");
                        }
                    }

                    Texture2D cachedTexture = GetCachedTexture(decoData.icon.name);
                    if (cachedTexture != null)
                    {
                        Sprite sprite = TextureToSprite(cachedTexture);
                        sprite.name = decoData.icon.name;
                        decoData.icon = sprite;
                    }

                    if (categoryCounter.ContainsKey(category))
                        categoryCounter[category]++;
                }
            }
        }

        private static void FixNewStatues()
        {
            InteractableObject[] objectsOfTypeAll = UnityEngine.Resources.FindObjectsOfTypeAll<InteractableObject>();
            if (objectsOfTypeAll.Length == 0)
                return;

            for (int i = 0; i < objectsOfTypeAll.Length; ++i)
            {
                InteractableObject interactableObject = objectsOfTypeAll[i];
                if (interactableObject == null)
                    continue;

                GameObject go = interactableObject.gameObject;
                if (go == null || !go.name.StartsWith("Interactable_MonsterStatue"))
                    continue;

                string meshName = go.name.Replace("Interactable_", "") + "_Mesh";
                string texName = go.name.Replace("Interactable_", "");

                MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
                if (meshFilters == null || meshFilters.Length == 0)
                    continue;

                foreach (var meshFilter in meshFilters)
                {
                    if (meshFilter == null)
                        continue;

                    Renderer renderer = meshFilter.GetComponent<Renderer>();
                    Material mat = renderer != null ? renderer.material : null;

                    if (meshFilter.name.Contains("IdlePose") || meshFilter.name.Contains("AtkPose"))
                    {
                        Mesh cachedMesh = GetCachedMesh(meshName);
                        if (cachedMesh != null)
                        {
                            cachedMesh.name = meshFilter.mesh.name;
                            meshFilter.mesh = cachedMesh;
                            meshFilter.mesh.UploadMeshData(true);
                        }
                        else if (vanilla_podium_meshes.ContainsKey(meshFilter.mesh.name))
                        {
                            meshFilter.mesh = vanilla_podium_meshes[meshFilter.mesh.name];
                            meshFilter.mesh.UploadMeshData(true);
                        }

                        if (mat != null)
                        {
                            Texture2D cachedTex = GetCachedTexture(texName);
                            if (cachedTex != null)
                            {
                                cachedTex.name = texName;
                                mat.SetTexture("_MainTex", cachedTex);
                                mat.SetOverrideTag("RenderType", "Transparent");
                                mat.SetInt("_SrcBlend", 5);
                                mat.SetInt("_DstBlend", 10);
                                mat.SetInt("_ZWrite", 1);
                                mat.DisableKeyword("_ALPHATEST_ON");
                                mat.EnableKeyword("_ALPHABLEND_ON");
                                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                mat.renderQueue = 2450;
                            }
                            else if (vanilla_podium_textures.TryGetValue(mat.GetTexture("_MainTex").name, out Texture tex))
                            {
                                mat.SetTexture("_MainTex", tex);
                            }
                        }
                    }
                    else if (meshFilter.name == "StatuePodium")
                    {
                        string podiumMeshName = texName + "_Podium_Mesh";
                        string podiumTexName = texName + "_Podium";

                        Mesh cachedMesh = GetCachedMesh(podiumMeshName);
                        if (cachedMesh != null)
                        {
                            cachedMesh.name = meshFilter.mesh.name;
                            meshFilter.mesh = cachedMesh;
                            meshFilter.mesh.UploadMeshData(true);
                        }

                        if (mat != null)
                        {
                            Texture2D cachedTex = GetCachedTexture(podiumTexName);
                            if (cachedTex != null)
                            {
                                cachedTex.name = podiumTexName;
                                mat.SetTexture("_MainTex", cachedTex);
                            }
                            else if (vanilla_podium_textures.TryGetValue(mat.GetTexture("_MainTex").name, out Texture tex))
                            {
                                mat.SetTexture("_MainTex", tex);
                            }
                        }
                    }
                    else if (meshFilter.name.Contains("StatuePodium ("))
                    {
                        string podiumMeshName2 = texName + "_Podium_Mesh2";
                        string podiumTexName2 = texName + "_Podium2";

                        Mesh cachedMesh = GetCachedMesh(podiumMeshName2);
                        if (cachedMesh != null)
                        {
                            cachedMesh.name = meshFilter.mesh.name;
                            meshFilter.mesh = cachedMesh;
                            meshFilter.mesh.UploadMeshData(true);
                        }

                        if (mat != null)
                        {
                            Texture2D cachedTex = GetCachedTexture(podiumTexName2);
                            if (cachedTex != null)
                            {
                                cachedTex.name = podiumTexName2;
                                mat.SetTexture("_MainTex", cachedTex);
                            }
                            else if (vanilla_podium_textures.TryGetValue(mat.GetTexture("_MainTex").name, out Texture tex))
                            {
                                mat.SetTexture("_MainTex", tex);
                            }
                        }
                    }
                }
            }
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = (Texture2D)null;
            if (File.Exists(filePath))
            {
                byte[] data = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(data);
            }
            return tex;
        }

        public static Texture2D LoadPNG_Bump(string filePath)
        {
            Texture2D tex = (Texture2D)null;
            if (File.Exists(filePath))
            {
                byte[] data = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, UnityEngine.TextureFormat.R8, true, true);
                tex.LoadImage(data);
            }
            return tex;
        }

        public static Sprite TextureToSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, (float)texture.width, (float)texture.height), new Vector2(0.5f, 0.5f), 50f, 0U, SpriteMeshType.FullRect);
        }
    }
}
