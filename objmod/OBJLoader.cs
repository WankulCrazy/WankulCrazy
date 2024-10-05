using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WankulCrazyPlugin.objmod
{
    public class OBJLoader
    {
        public SplitMode SplitMode = SplitMode.Object;
        internal List<Vector3> Vertices = new List<Vector3>();
        internal List<Vector3> Normals = new List<Vector3>();
        internal List<Vector2> UVs = new List<Vector2>();
        internal Dictionary<string, Material> Materials;
        private FileInfo _objInfo;


        public GameObject Load(Stream input)
        {
            StreamReader reader = new StreamReader(input);
            Dictionary<string, OBJObjectBuilder> builderDict = new Dictionary<string, OBJObjectBuilder>();
            OBJObjectBuilder currentBuilder = null;
            string material = "default";
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();
            Action<string> action = objectName =>
            {
                if (builderDict.TryGetValue(objectName, out currentBuilder))
                    return;
                currentBuilder = new OBJObjectBuilder(objectName, this);
                builderDict[objectName] = currentBuilder;
            };
            action("default");
            CharWordReader charWordReader = new CharWordReader(reader, 4096);
            while (true)
            {
                string str1;
                do
                {
                    charWordReader.SkipWhitespaces();
                    if (!charWordReader.endReached)
                    {
                        charWordReader.ReadUntilWhiteSpace();
                        if (!charWordReader.Is("#"))
                        {
                            if (Materials != null || !charWordReader.Is("mtllib"))
                            {
                                if (!charWordReader.Is("v"))
                                {
                                    if (!charWordReader.Is("vn"))
                                    {
                                        if (!charWordReader.Is("vt"))
                                        {
                                            if (charWordReader.Is("usemtl"))
                                            {
                                                charWordReader.SkipWhitespaces();
                                                charWordReader.ReadUntilNewLine();
                                                str1 = charWordReader.GetString();
                                                material = str1;
                                            }
                                            else
                                                goto label_14;
                                        }
                                        else
                                            goto label_10;
                                    }
                                    else
                                        goto label_8;
                                }
                                else
                                    goto label_6;
                            }
                            else
                                goto label_4;
                        }
                        else
                            goto label_2;
                    }
                    else
                        goto label_40;
                }
                while (SplitMode != SplitMode.Material);
                goto label_13;
            label_2:
                charWordReader.SkipUntilNewLine();
                continue;
            label_4:
                charWordReader.SkipWhitespaces();
                charWordReader.ReadUntilNewLine();
                continue;
            label_6:
                Vertices.Add(charWordReader.ReadVector());
                continue;
            label_8:
                Normals.Add(charWordReader.ReadVector());
                continue;
            label_10:
                UVs.Add((Vector2)charWordReader.ReadVector());
                continue;
            label_13:
                action(str1);
                continue;
            label_14:
                if ((charWordReader.Is("o") || charWordReader.Is("g")) && SplitMode == SplitMode.Object)
                {
                    charWordReader.ReadUntilNewLine();
                    string str2 = charWordReader.GetString(1);
                    action(str2);
                }
                else if (charWordReader.Is("f"))
                {
                    while (true)
                    {
                        bool newLinePassed;
                        charWordReader.SkipWhitespaces(out newLinePassed);
                        if (!newLinePassed)
                        {
                            int num1 = int.MinValue;
                            int num2 = int.MinValue;
                            int num3 = charWordReader.ReadInt();
                            if (charWordReader.currentChar == '/')
                            {
                                charWordReader.MoveNext();
                                if (charWordReader.currentChar != '/')
                                    num2 = charWordReader.ReadInt();
                                if (charWordReader.currentChar == '/')
                                {
                                    charWordReader.MoveNext();
                                    num1 = charWordReader.ReadInt();
                                }
                            }
                            if (num3 > int.MinValue)
                            {
                                if (num3 < 0)
                                    num3 = Vertices.Count - num3;
                                --num3;
                            }
                            if (num1 > int.MinValue)
                            {
                                if (num1 < 0)
                                    num1 = Normals.Count - num1;
                                --num1;
                            }
                            if (num2 > int.MinValue)
                            {
                                if (num2 < 0)
                                    num2 = UVs.Count - num2;
                                --num2;
                            }
                            vertexIndices.Add(num3);
                            normalIndices.Add(num1);
                            uvIndices.Add(num2);
                        }
                        else
                            break;
                    }
                    currentBuilder.PushFace(material, vertexIndices, normalIndices, uvIndices);
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();
                }
                else
                    charWordReader.SkipUntilNewLine();
            }
        label_40:
            GameObject gameObject = new GameObject(_objInfo != null ? Path.GetFileNameWithoutExtension(_objInfo.Name) : "WavefrontObject");
            gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
            foreach (KeyValuePair<string, OBJObjectBuilder> keyValuePair in builderDict)
            {
                if (keyValuePair.Value.PushedFaceCount != 0)
                    keyValuePair.Value.Build().transform.SetParent(gameObject.transform, false);
            }
            return gameObject;
        }


        public GameObject Load(string path, string mtlPath)
        {
            _objInfo = new FileInfo(path);
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                using (FileStream input = new FileStream(path, FileMode.Open))
                    return Load(input);
            }
            else
            {
                using (FileStream input = new FileStream(path, FileMode.Open))
                    return Load(input);
            }
        }

        public GameObject Load(string path) => Load(path, null);
    }
}

