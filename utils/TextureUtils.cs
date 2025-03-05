using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WankulCrazyPlugin.utils
{
    public class TextureUtils
    {
        public static Texture2D MakeTextureReadable(Texture2D texture)
        {
            RenderTexture temporaryRenderTex = RenderTexture.GetTemporary(
                texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, temporaryRenderTex);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temporaryRenderTex;

            Texture2D readableTexture = new Texture2D(texture.width, texture.height);
            readableTexture.ReadPixels(new Rect(0, 0, temporaryRenderTex.width, temporaryRenderTex.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporaryRenderTex);

            return readableTexture;
        }

        public static Texture2D LoadTexture(string path)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            return texture;
        }
    }
}
