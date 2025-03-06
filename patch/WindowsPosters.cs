using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

namespace WankulCrazyPlugin.patch
{

    public class WindowsPosters
    {
        public static void Init()
        {
            Transform Windows_Transform = Plugin.GetByPathIn("Level_Environment_Grp", "StoreModel_Group/Windows door");

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-0.454f, 0.7963f, 0.5f),
                new Vector3(1.7423f, 0.7963f, 0.5f),
                new Vector3(3.8385f, 0.7963f, 0.5f),
                new Vector3(5.8347f, 0.7963f, 0.5f),
                new Vector3(7.86f, 0.7963f, 0.5f),
                new Vector3(9.85f, 0.7963f, 0.5f)
            };

            string[] texturePaths = new string[]
            {
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Gold.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Legendaire.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Gamer.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Shiny.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Rare.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Commun.png"),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                string posterName = "Poster" + (i + 1);
                GameObject poster = GameObject.CreatePrimitive(PrimitiveType.Quad); // Utiliser un Quad
                poster.name = posterName;

                MeshRenderer meshRenderer = poster.GetComponent<MeshRenderer>();
                Material material = new Material(Shader.Find("Standard")); // Utiliser un shader existant
                meshRenderer.material = material;

                string texturePath = texturePaths[i];
                Texture2D texture = LoadPNG(texturePath);

                if (texture != null)
                {
                    material.mainTexture = texture;
                    meshRenderer.material = material;
                    // 🔹 Ajuster la taille du Quad pour correspondre au ratio de l’image
                    float aspectRatio = (float)texture.width / texture.height;
                    poster.transform.localScale = new Vector3(aspectRatio, 1f, 1f);
                }
                else
                {
                    Plugin.Logger.LogInfo(posterName + " : Échec du chargement de la texture");
                }

                poster.transform.SetParent(Windows_Transform, false);
                poster.transform.localPosition = positions[i];
                poster.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }

        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            if (File.Exists(filePath))
            {
                byte[] data = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(data);
            }
            return tex;
        }

        public static Sprite TextureToSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}