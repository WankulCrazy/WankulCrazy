using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WankulCrazyPlugin.utils.obj;
using WankulCrazyPlugin;
using UnityEngine.UI;
using BepInEx;
using System.Globalization;

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


        private static void checkFolders()
        {
            if (!Directory.Exists(OBJImporter.path_mes))
                Directory.CreateDirectory(OBJImporter.path_mes);
            if(!Directory.Exists(OBJImporter.path_spr))
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
