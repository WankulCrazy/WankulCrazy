using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using HarmonyLib;

namespace WankulCrazyPlugin.importer
{
    internal class PatchTexturesImporter
    {
        public static void ReplaceGameTextures(string textureLevel)
        {
            string texturesPath = Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", textureLevel);
            List<string> filenames = GetFileNames(texturesPath);

            foreach (string filename in filenames)
            {
                Texture targetTexture = Resources.FindObjectsOfTypeAll<Texture>().FirstOrDefault(t => t.name == Path.GetFileNameWithoutExtension(filename));
                if (targetTexture != null)
                {
                    string texturePath = Path.Combine(texturesPath, filename);
                    if (targetTexture is Texture2D targetTexture2D)
                    {
                        Texture2D newTexture = LoadTexture2D(texturePath);
                        ReplaceTexture(targetTexture2D, newTexture);
                    }
                }
                else
                {
                    Plugin.Logger.LogWarning($"Texture {Path.GetFileNameWithoutExtension(filename)} not found");
                }
            }
        }

        private static void ReplaceTexture(Texture2D original, Texture2D replacement)
        {
            if (original.width != replacement.width || original.height != replacement.height)
            {
                Plugin.Logger.LogError("Les dimensions des textures ne correspondent pas.");
                return;
            }

            if (original.format != replacement.format)
            {
                replacement = ConvertTextureFormat(replacement, original.format, original.mipmapCount);
            }

            if (original.mipmapCount != replacement.mipmapCount)
            {
                replacement = AdjustMipMapLevels(replacement, original.mipmapCount);
            }

            try
            {
                Graphics.CopyTexture(replacement, original);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Erreur lors du remplacement de la texture : {ex.Message}");
            }
        }

        private static Texture2D ConvertTextureFormat(Texture2D texture, TextureFormat format, int mipCount)
        {
            // Crée une nouvelle Texture2D avec le format souhaité et le nombre de mipmaps
            Texture2D convertedTexture = new Texture2D(texture.width, texture.height, format, mipCount > 1);

            // Copie les pixels de la texture d'origine vers la nouvelle texture
            if (format != TextureFormat.DXT5 && format != TextureFormat.DXT1)
            {
                Color[] pixels = texture.GetPixels();
                convertedTexture.SetPixels(pixels);
                convertedTexture.Apply(true);
            }
            else
            {
                if (TextureFormat.DXT1 == format)
                {
                    texture = ConvertTextureToNoAlpha(texture);
                    Color[] pixels = texture.GetPixels();
                    convertedTexture = new Texture2D(texture.width, texture.height, texture.format, mipCount > 1);
                    convertedTexture.SetPixels(pixels);
                    convertedTexture.Apply(true);
                    convertedTexture.Compress(false);

                }
                else
                {
                    Color[] pixels = texture.GetPixels();
                    convertedTexture = new Texture2D(texture.width, texture.height, texture.format, mipCount > 1);
                    convertedTexture.SetPixels(pixels);
                    convertedTexture.Apply(true);
                    convertedTexture.Compress(false);
                }
            }

            return convertedTexture;
        }

        private static Texture2D ConvertTextureToNoAlpha(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;

            // Crée une nouvelle texture sans canal alpha
            Texture2D textureNoAlpha = new Texture2D(width, height, TextureFormat.RGB24, texture.mipmapCount > 1);

            // Obtenez les pixels de la texture d'origine
            Color[] pixels = texture.GetPixels();

            // Créez un tableau de couleurs sans alpha
            Color[] pixelsNoAlpha = new Color[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                // Copiez les valeurs RGB et ignorez l'alpha
                pixelsNoAlpha[i] = new Color(pixels[i].r, pixels[i].g, pixels[i].b);
            }

            // Appliquez les pixels à la nouvelle texture
            textureNoAlpha.SetPixels(pixelsNoAlpha);
            textureNoAlpha.Apply(true);

            return textureNoAlpha;
        }



        private static Texture2D AdjustMipMapLevels(Texture2D texture, int mipCount)
        {
            Texture2D adjustedTexture = new Texture2D(texture.width, texture.height, texture.format, mipCount > 1);
            adjustedTexture.SetPixels(texture.GetPixels());
            adjustedTexture.Apply(true);
            return adjustedTexture;
        }

        private static Texture2D LoadTexture2D(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            texture.LoadImage(bytes);
            return texture;
        }

        public static List<string> GetFileNames(string directoryPath)
        {
            List<string> fileNames = new List<string>();
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                fileNames.Add(fileName);
            }

            return fileNames;
        }

        static void ItemPostfix(Item __instance, Mesh mesh, Material material, EItemType itemType, Mesh meshSecondary, Material materialSecondary)
        {
            // Dictionnaire associant chaque type de CardBox à sa texture
            Dictionary<EItemType, string> texturePaths = new Dictionary<EItemType, string>
            {
                { EItemType.BasicCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1.png") },
                { EItemType.RareCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2.png") },
                { EItemType.EpicCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3.png") },
                { EnumExtensions.SafeParseEItemType("DisplayStellar"), Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S4.png") },
                { EItemType.LegendaryCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS.png") },
                { EItemType.DestinyBasicCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S1_TauxDrop.png") },
                { EItemType.DestinyRareCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S2_TauxDrop.png") },
                { EItemType.DestinyEpicCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_S3_TauxDrop.png") },
                { EItemType.DestinyLegendaryCardBox, Path.Combine(Plugin.GetPluginPath(), "data", "patchtextures", "shared1", "Texture_Display_HS_TauxDrop.png") },
            };

            if (texturePaths.TryGetValue(itemType, out string texturePath))
            {
                //Debug.Log($"{itemType} {__instance.name} {mesh.name} {material.name} détecté ! Application d'une nouvelle texture.");
                ApplyTextureToItem(__instance, texturePath);
            }
        }

        private static Texture2D LoadTexture(string path)
        {
            // Exemple de chargement d'une texture à partir d'un fichier (ajuste selon ton projet)
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
                return tex;
            return null;
        }
        private static void ApplyTextureToItem(Item item, string texturePath)
        {
            Texture2D newTexture = LoadTexture(texturePath);

            if (newTexture != null)
            {
                if (item.m_Mesh != null)
                {
                    Renderer renderer = item.m_Mesh.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (renderer.material != null && renderer.material.mainTexture == newTexture)
                        {
                            Debug.Log("Texture déjà appliquée, aucune modification nécessaire.");
                            return;
                        }

                        // Réutilise un matériau existant si possible
                        Material material = new Material(Shader.Find("Standard"));
                        material.mainTexture = newTexture;
                        renderer.material = material;

                        //Debug.Log("Texture appliquée avec succès !");
                    }
                    else
                    {
                        Debug.LogError("Impossible de récupérer le Renderer.");
                    }
                }
                else
                {
                    Debug.LogError("m_Mesh est null !");
                }
            }
            else
            {
                Debug.LogError("Échec du chargement de la texture !");
            }
        }
    }
}
