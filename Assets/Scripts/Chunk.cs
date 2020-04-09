using Noise;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    [System.Serializable]
    public class Chunk 
    {
        public static readonly Vector3Int Size = new Vector3Int(16, 64, 16);
        private static List<Vector3> vertices;
        private static List<Vector2> uv;
        private static List<int> triangles; 
        private static Material blocksMaterial;
        private static Material objectsMaterial;
        private static BlockObject testObject;

        [SerializeField] private int x, y;
        [HideInInspector] public int[] data;
        [NonSerialized] private Mesh mesh; 
        [NonSerialized] private bool dirty; 

        public Chunk(int x, int y)
        {
            this.x = x;
            this.y = y;
            data = new int[Size.x * Size.y * Size.z];
            dirty = true;
            GenerateTerrain();
        }

        public Chunk(int x, int y, int[] data, int index)
        {
            this.x = x;
            this.y = y;
            this.data = new int[Size.x * Size.y * Size.z];
            data.CopyTo(this.data, index);
            dirty = true;
        }

        public int X => x;
        public int Y => y;
        public Mesh Mesh => mesh;
        public bool IsDirty => dirty;
        public void SetDirty() => dirty = true;
        public int this[int x, int y, int z]
        {
            get => data[x + Size.x * (y + Size.y * z)];
            set
            {
                if (dirty || data[x + Size.x * (y + Size.y * z)] != value)
                {
                    data[x + Size.x * (y + Size.y * z)] = value;
                    dirty = true;
                }
            }
        }

        public void SetData(int[] data, int index)
        {
            data.CopyTo(this.data, index);
            dirty = true;
        }

        public int[] GetData(int index, int length)
        {
            if (index + length > this.data.Length)
                length -= index + length - this.data.Length;

            int[] data = new int[length];
            for (int i = 0; i < length; i++)
                data[i] = this.data[index + i];
            return data;
        }

        public static int SampleHeight(int x, int y)
        {
            return Mathf.FloorToInt(Perlin.Noise(x / 100f, y / 100f, World.Instance.settings) * Size.y);
        }

        public void GenerateTerrain()
        {
            for (int x = 0; x < Size.x; x++)
                for (int z = 0; z < Size.z; z++)
                {
                    int height = SampleHeight(X * Size.x + x, Y * Size.z + z);
                    for (int y = 0; y < height; y++)
                        this[x, y, z] = x + z * 16;
                }
        }

        public void RegenerateMesh()
        {
            #region Declare Arrays

            vertices = new List<Vector3>();
            uv = new List<Vector2>();
            triangles = new List<int>();

            #endregion

            #region Add Quad

            void AddQuad(Vector3 c00, Vector3 c10, Vector3 c11, Vector3 c01, int index)
            {
                if (index <= 0)
                    return;

                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 3);

                vertices.Add(c00);
                vertices.Add(c10);
                vertices.Add(c11);
                vertices.Add(c01);

                uv.Add(new Vector2((index % 16 + 0) / 16f, (index / 16 + 0) / 16f));
                uv.Add(new Vector2((index % 16 + 1) / 16f, (index / 16 + 0) / 16f));
                uv.Add(new Vector2((index % 16 + 1) / 16f, (index / 16 + 1) / 16f));
                uv.Add(new Vector2((index % 16 + 0) / 16f, (index / 16 + 1) / 16f));
            }

            #endregion

            //World.Instance.TryGetChunk(X, Y + 1, out Chunk n);
            //World.Instance.TryGetChunk(X + 1, Y, out Chunk e);
            //World.Instance.TryGetChunk(X, Y - 1, out Chunk s);
            //World.Instance.TryGetChunk(X - 1, Y, out Chunk w);
            
            for (int x = 0; x < Size.x; x++)
                for (int y = 0; y < Size.y; y++)
                    for (int z = 0; z < Size.z; z++)
                    {
                        if (this[x, y, z] == 0)
                            continue;

                        #region East Faces
                        if (x == Size.x - 1 || this[x + 1, y, z] == 0)
                            AddQuad
                            (
                                new Vector3(x + 1, y + 0, z + 0),
                                new Vector3(x + 1, y + 0, z + 1),
                                new Vector3(x + 1, y + 1, z + 1),
                                new Vector3(x + 1, y + 1, z + 0),
                                this[x, y, z]
                            );
                        #endregion

                        #region West Faces
                        if (x == 0 || this[x - 1, y, z] == 0)
                            AddQuad
                            (
                                new Vector3(x + 0, y + 0, z + 1),
                                new Vector3(x + 0, y + 0, z + 0),
                                new Vector3(x + 0, y + 1, z + 0),
                                new Vector3(x + 0, y + 1, z + 1),
                                this[x, y, z]
                            );
                        #endregion

                        #region Up Faces
                        if (y == Size.y - 1 || this[x, y + 1, z] == 0)
                            AddQuad
                            (
                                new Vector3(x + 0, y + 1, z + 0),
                                new Vector3(x + 1, y + 1, z + 0),
                                new Vector3(x + 1, y + 1, z + 1),
                                new Vector3(x + 0, y + 1, z + 1),
                                this[x, y, z]
                            );
                        #endregion

                        #region Down Faces
                        if (y == 0 || this[x, y - 1, z] == 0)
                            AddQuad
                            (
                                new Vector3(x + 1, y + 0, z + 0),
                                new Vector3(x + 0, y + 0, z + 0),
                                new Vector3(x + 0, y + 0, z + 1),
                                new Vector3(x + 1, y + 0, z + 1),
                                this[x, y, z]
                            );
                        #endregion

                        #region North Faces
                        if (z == Size.z - 1 || this[x, y, z + 1] == 0)
                            AddQuad
                            (
                                new Vector3(x + 1, y + 0, z + 1),
                                new Vector3(x + 0, y + 0, z + 1),
                                new Vector3(x + 0, y + 1, z + 1),
                                new Vector3(x + 1, y + 1, z + 1),
                                this[x, y, z]
                            );
                        #endregion

                        #region South Faces
                        if (z == 0 || this[x, y, z - 1] == 0)
                            AddQuad
                            (
                                new Vector3(x + 0, y + 0, z + 0),
                                new Vector3(x + 1, y + 0, z + 0),
                                new Vector3(x + 1, y + 1, z + 0),
                                new Vector3(x + 0, y + 1, z + 0),
                                this[x, y, z]
                            );
                        #endregion
                    }

            #region Build Mesh
            mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            #endregion

            dirty = false;
        }

        public void DrawMesh()
        {
            if (blocksMaterial == null)
                blocksMaterial = Resources.Load<Material>("Blocks");

            if (dirty)
                RegenerateMesh();

            Graphics.DrawMesh(mesh, new Vector3(x * Size.x, 0, y * Size.z), Quaternion.identity, blocksMaterial, LayerMask.NameToLayer("Blocks"));
        }

        public void DrawObjects()
        {
            if (objectsMaterial == null)
                objectsMaterial = Resources.Load<Material>("PiksolStandard");

            if (testObject == null)
            {
                testObject = new BlockObject(8, 8, 8);
                testObject[0, 0, 0] = 1;
                testObject[1, 0, 0] = 1;
                testObject[2, 0, 0] = 1;
                testObject[0, 1, 0] = 2;
                testObject[0, 1, 1] = 2;
                testObject[0, 1, 2] = 2;
                testObject.RegenerateMesh();
            }

            for (int i = 0; i < data.Length; i++)
                if (data[i] == -1)
                {
                    int x = i % Size.x;
                    int y = (i / Size.x) % Size.y;
                    int z = i / (Size.y * Size.x);

                    Graphics.DrawMesh(testObject.Mesh, new Vector3(X * Size.x + x, y, Y * Size.z + z), Quaternion.identity, objectsMaterial, LayerMask.NameToLayer("Blocks"));
                }


        }
    }
}
