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

            GameObject poster = new GameObject("Poster");

            SpriteRenderer spriteRenderer = poster.AddComponent<SpriteRenderer>();

            string texturePath = Path.Combine(Plugin.GetPluginPath(), "data", "sprites", "CardBox.png");
            Texture2D texture = LoadPNG(texturePath);

            if (texture != null)
            {
                spriteRenderer.sprite = TextureToSprite(texture);

                Plugin.Logger.LogInfo("Sprite créé avec succès");
            }
            else
            {
                Plugin.Logger.LogInfo("Échec du chargement de la texture");
            }

            poster.transform.SetParent(Windows_Transform, false);

            poster.transform.localPosition = new Vector3(0, 1.2618f, 0.5f);
            poster.transform.localScale = new Vector2(0.2f, 0.2f);
            poster.transform.localRotation = Quaternion.Euler(0, 180, 0);
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
