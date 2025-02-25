using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WankulCrazyPlugin.patch
{
    public class RuntimeMakeTextureReadable
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
    }
}
