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

namespace WankulCrazyPlugin.importer
{
    internal class OBJImporter
    {
        private static string path_mes = Path.Combine(Plugin.GetPluginPath(), "data", "meshes/");
        private static string path_nam = Path.Combine(Plugin.GetPluginPath(), "data", "names/");
        private static MeshFilter[] mesh_list = new MeshFilter[0];
        private static Material[] mat_list = new Material[0];
        private static Image[] image_list = new Image[0];
        public static Dictionary<string, string> filePaths_obj = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
        public static GameObject tempmesh = new GameObject((string)null);


        private static void checkFolders()
        {
            if (!Directory.Exists(OBJImporter.path_mes))
                Directory.CreateDirectory(OBJImporter.path_mes);
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
    }
}
