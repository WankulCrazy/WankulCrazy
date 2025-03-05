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
                new Vector3(-0.4545f, 0.7963f, 0.5f),
                new Vector3(1.7419f, 0.7963f, 0.5f),
                new Vector3(3.8383f, 0.7963f, 0.5f),
                new Vector3(5.8347f, 0.7963f, 0.5f),
                new Vector3(7.86f, 0.7963f, 0.5f),
                new Vector3(9.85f, 0.7963f, 0.5f)
            };

            string[] texturePaths = new string[]
            {
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Commun.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Rare.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Shiny.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Legendaire.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Gold.png"),
                Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "Affiche_Pack_Gamer.png")
            };

            for (int i = 0; i < positions.Length; i++)
            {
                string posterName = "Poster" + (i + 1);
                GameObject poster = new GameObject(posterName);

                SpriteRenderer spriteRenderer = poster.AddComponent<SpriteRenderer>();

                string texturePath = texturePaths[i];
                Texture2D texture = LoadPNG(texturePath);

                if (texture != null)
                {
                    spriteRenderer.sprite = TextureToSprite(texture);
                    spriteRenderer.material.SetInt("_ZWrite", 1);
                    Plugin.Logger.LogInfo(posterName + " : Sprite créé avec succès");
                }
                else
                {
                    Plugin.Logger.LogInfo(posterName + " : Échec du chargement de la texture");
                }

                poster.transform.SetParent(Windows_Transform, false);
                poster.transform.localPosition = positions[i];
                poster.transform.localScale = new Vector2(0.048f, 0.048f);
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