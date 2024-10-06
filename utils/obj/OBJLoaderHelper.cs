using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace WankulCrazyPlugin.utils.obj
{
    public static class OBJLoaderHelper
    {
        public static void EnableMaterialTransparency(Material mtl)
        {
            mtl.SetFloat("_Mode", 3f);
            mtl.SetInt("_SrcBlend", 5);
            mtl.SetInt("_DstBlend", 10);
            mtl.SetInt("_ZWrite", 0);
            mtl.DisableKeyword("_ALPHATEST_ON");
            mtl.EnableKeyword("_ALPHABLEND_ON");
            mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtl.renderQueue = 3000;
        }

        public static float FastFloatParse(string input)
        {
            if (input.Contains("e") || input.Contains("E"))
                return float.Parse(input, CultureInfo.InvariantCulture);
            float num1 = 0.0f;
            int num2 = 0;
            int length = input.Length;
            if (length == 0)
                return float.NaN;
            char ch1 = input[0];
            float num3 = 1f;
            if (ch1 == '-')
            {
                num3 = -1f;
                ++num2;
                if (num2 >= length)
                    return float.NaN;
            }
            char ch2;
            while (true)
            {
                if (num2 < length)
                {
                    ch2 = input[num2++];
                    if (ch2 >= '0' && ch2 <= '9')
                        num1 = num1 * 10f + (ch2 - 48);
                    else
                        goto label_11;
                }
                else
                    break;
            }
            return num3 * num1;
        label_11:
            if (ch2 != '.' && ch2 != ',')
                return float.NaN;
            float num4 = 0.1f;
            while (num2 < length)
            {
                char ch3 = input[num2++];
                if (ch3 < '0' || ch3 > '9')
                    return float.NaN;
                num1 += (ch3 - 48) * num4;
                num4 *= 0.1f;
            }
            return num3 * num1;
        }

        public static int FastIntParse(string input)
        {
            int num = 0;
            bool flag = input[0] == '-';
            for (int index = flag ? 1 : 0; index < input.Length; ++index)
                num = num * 10 + (input[index] - 48);
            return flag ? -num : num;
        }

        public static Material CreateNullMaterial() => new Material(Shader.Find("Standard"));

        public static Vector3 VectorFromStrArray(string[] cmps)
        {
            float x = FastFloatParse(cmps[1]);
            float y = FastFloatParse(cmps[2]);
            if (cmps.Length != 4)
                return (Vector3)new Vector2(x, y);
            float z = FastFloatParse(cmps[3]);
            return new Vector3(x, y, z);
        }

        public static Color ColorFromStrArray(string[] cmps, float scalar = 1f)
        {
            return new Color(FastFloatParse(cmps[1]) * scalar, FastFloatParse(cmps[2]) * scalar, FastFloatParse(cmps[3]) * scalar);
        }
    }
}