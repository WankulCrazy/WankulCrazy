using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine;

namespace WankulCrazyPlugin.utils.obj
{
    public class OBJObjectBuilder
    {
        private OBJLoader _loader;
        private string _name;
        private Dictionary<ObjLoopHash, int> _globalIndexRemap = new Dictionary<ObjLoopHash, int>();
        private Dictionary<string, List<int>> _materialIndices = new Dictionary<string, List<int>>();
        private List<int> _currentIndexList;
        private string _lastMaterial = null;
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<Vector2> _uvs = new List<Vector2>();
        private bool recalculateNormals = false;

        public int PushedFaceCount { get; private set; } = 0;

        public GameObject Build()
        {
            GameObject gameObject = new GameObject(_name);
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            int index = 0;
            Material[] materialArray = new Material[_materialIndices.Count];
            foreach (KeyValuePair<string, List<int>> materialIndex in _materialIndices)
            {
                Material material = null;
                if (_loader.Materials == null)
                {
                    material = OBJLoaderHelper.CreateNullMaterial();
                    material.name = materialIndex.Key;
                }
                else if (!_loader.Materials.TryGetValue(materialIndex.Key, out material))
                {
                    material = OBJLoaderHelper.CreateNullMaterial();
                    material.name = materialIndex.Key;
                    _loader.Materials[materialIndex.Key] = material;
                }
                materialArray[index] = material;
                ++index;
            }
            meshRenderer.sharedMaterials = materialArray;
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            int submesh = 0;
            Mesh mesh1 = new Mesh();
            mesh1.name = _name;
            mesh1.indexFormat = _vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh1.subMeshCount = _materialIndices.Count;
            Mesh mesh2 = mesh1;
            mesh2.SetVertices(_vertices);
            mesh2.SetNormals(_normals);
            mesh2.SetUVs(0, _uvs);
            foreach (KeyValuePair<string, List<int>> materialIndex in _materialIndices)
            {
                mesh2.SetTriangles(materialIndex.Value, submesh);
                ++submesh;
            }
            if (recalculateNormals)
                mesh2.RecalculateNormals();
            mesh2.RecalculateTangents();
            mesh2.RecalculateBounds();
            meshFilter.sharedMesh = mesh2;
            return gameObject;
        }

        public void SetMaterial(string name)
        {
            if (_materialIndices.TryGetValue(name, out _currentIndexList))
                return;
            _currentIndexList = new List<int>();
            _materialIndices[name] = _currentIndexList;
        }

        public void PushFace(
          string material,
          List<int> vertexIndices,
          List<int> normalIndices,
          List<int> uvIndices)
        {
            if (vertexIndices.Count < 3)
                return;
            if (material != _lastMaterial)
            {
                SetMaterial(material);
                _lastMaterial = material;
            }
            int[] numArray = new int[vertexIndices.Count];
            for (int index = 0; index < vertexIndices.Count; ++index)
            {
                int vertexIndex = vertexIndices[index];
                int normalIndex = normalIndices[index];
                int uvIndex = uvIndices[index];
                ObjLoopHash key = new ObjLoopHash()
                {
                    vertexIndex = vertexIndex,
                    normalIndex = normalIndex,
                    uvIndex = uvIndex
                };
                int num = -1;
                if (!_globalIndexRemap.TryGetValue(key, out num))
                {
                    _globalIndexRemap.Add(key, _vertices.Count);
                    num = _vertices.Count;
                    _vertices.Add(vertexIndex < 0 || vertexIndex >= _loader.Vertices.Count ? Vector3.zero : _loader.Vertices[vertexIndex]);
                    _normals.Add(normalIndex < 0 || normalIndex >= _loader.Normals.Count ? Vector3.zero : _loader.Normals[normalIndex]);
                    _uvs.Add(uvIndex < 0 || uvIndex >= _loader.UVs.Count ? Vector2.zero : _loader.UVs[uvIndex]);
                    if (normalIndex < 0)
                        recalculateNormals = true;
                }
                numArray[index] = num;
            }
            if (numArray.Length == 3)
                _currentIndexList.AddRange(new int[3]
                {
          numArray[0],
          numArray[1],
          numArray[2]
                });
            else if (numArray.Length == 4)
            {
                _currentIndexList.AddRange(new int[3]
                {
          numArray[0],
          numArray[1],
          numArray[2]
                });
                _currentIndexList.AddRange(new int[3]
                {
          numArray[2],
          numArray[3],
          numArray[0]
                });
            }
            else if (numArray.Length > 4)
            {
                for (int index = numArray.Length - 1; index >= 2; --index)
                    _currentIndexList.AddRange(new int[3]
                    {
            numArray[0],
            numArray[index - 1],
            numArray[index]
                    });
            }
            ++PushedFaceCount;
        }

        public OBJObjectBuilder(string name, OBJLoader loader)
        {
            _name = name;
            _loader = loader;
        }

        private class ObjLoopHash
        {
            public int vertexIndex;
            public int normalIndex;
            public int uvIndex;

            public override bool Equals(object obj)
            {
                if (!(obj is ObjLoopHash))
                    return false;
                ObjLoopHash objLoopHash = obj as ObjLoopHash;
                return objLoopHash.vertexIndex == vertexIndex && objLoopHash.uvIndex == uvIndex && objLoopHash.normalIndex == normalIndex;
            }

            public override int GetHashCode()
            {
                return ((3 * 314159 + vertexIndex) * 314159 + normalIndex) * 314159 + uvIndex;
            }
        }
    }
}
