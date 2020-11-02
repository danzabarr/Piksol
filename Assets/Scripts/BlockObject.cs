using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Piksol
{
    [Serializable]
    public class BlockObject : ScriptableObject
    {
        public static readonly float VoxelSize = 1f / 8f;

        private static int[,,] bubbles;
        private static List<Vector3> vertices;
        private static List<Vector2> metal;
        private static List<Vector2> emissionRG;
        private static List<Vector2> emissionBA;
        private static List<int> triangles;
        private static List<Color> colors;
        private static List<BoundsInt> vols;
        private static bool[,,] vol;
        private static int[,] faces;

        private static readonly int BeingChecked = -2;
        private static readonly int NotInsideBubble = -1;
        private static readonly int Unknown = 0;
        private static readonly int InsideBubble = 1;
        private static readonly int Solid = 2;

        private static readonly int[] NX = new int[] { 1, -1, 0, 0, 0, 0 };
        private static readonly int[] NY = new int[] { 0, 0, 1, -1, 0, 0 };
        private static readonly int[] NZ = new int[] { 0, 0, 0, 0, 1, -1 };

        [SerializeField] private readonly int[] data;
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3Int size;
        [SerializeField] private Vector3Int lowerInset;
        [SerializeField] private Vector3Int upperInset;
        [SerializeField] private string paletteName;

        [NonSerialized] private Mesh mesh;
        [NonSerialized] private Palette palette;
        [NonSerialized] private BoundsInt[] volumes;
        [NonSerialized] private bool dirty;

        public bool IsDirty => dirty;

        public Vector3 Origin
        {
            get => origin;
            set
            {
                if (origin != value)
                {
                    origin = value;
                    dirty = true;
                }
            }
        }

        public Vector3Int Size => size;
        public Vector3Int InsetSize => size - lowerInset - upperInset;
        public Vector3 Center => (Vector3)size * .5f * VoxelSize;//(lowerInset + (Vector3)InsetSize * .5f) * VoxelSize;
        public Vector3Int LowerInset
        {
            get => lowerInset;
            set
            {
                if (size.x - upperInset.x - value.x < 1)
                    value.x = size.x - upperInset.x - 1;

                if (size.y - upperInset.y - value.y < 1)
                    value.y = size.y - upperInset.y - 1;

                if (size.z - upperInset.z - value.z < 1)
                    value.z = size.z - upperInset.z - 1;

                value = Vector3Int.Max(value, Vector3Int.zero);

                if (lowerInset != value)
                {
                    lowerInset = value;
                    dirty = true;
                }
            }
        }
        public Vector3Int UpperInset
        {
            get => upperInset;
            set
            {
                if (size.x - lowerInset.x - value.x < 1)
                    value.x = size.x - lowerInset.x - 1;

                if (size.y - lowerInset.y - value.y < 1)
                    value.y = size.y - lowerInset.y - 1;

                if (size.z - lowerInset.z - value.z < 1)
                    value.z = size.z - lowerInset.z - 1;

                value = Vector3Int.Max(value, Vector3Int.zero);

                if (upperInset != value)
                {
                    upperInset = value;
                    dirty = true;
                }
            }
        }

        public Mesh Mesh
        {
            get
            {
                if (mesh == null)
                    RegenerateMesh();
                return mesh;
            }
        }

        public BoundsInt[] Volumes => volumes;

        public string PaletteName
        {
            get => paletteName;
            set
            {
                if (Palette.Load(value, out Palette palette))
                    paletteName = palette.Name;
                else
                    paletteName = null;

                if (this.palette != palette)
                {
                    this.palette = palette;
                    dirty = true;
                }
            }
        }

        public Palette Palette
        {
            get {
                if (palette == null)
                {
                    if (Palette.Load(paletteName, out Palette palette))
                        paletteName = palette.Name;
                    else
                        paletteName = null;

                    if (this.palette != palette)
                    {
                        this.palette = palette;
                        dirty = true;
                    }
                }
                return palette;
            }
            set => PaletteName = value?.Name;
        }

       

        public BlockObject(int xSize, int ySize, int zSize, string paletteName = null)
        {
            size = new Vector3Int(xSize, ySize, zSize);
            this.paletteName = paletteName;
            data = new int[size.x * size.y * size.z];
            dirty = true;
        }

        public BlockObject(BlockObject original) 
        {
            size = original.size;
            data = new int[original.data.Length];
            for (int i = 0; i < data.Length; i++)
                data[i] = original.data[i];
            paletteName = original.PaletteName;
            lowerInset = original.lowerInset;
            upperInset = original.upperInset;
            palette = original.palette;
            mesh = original.mesh;
            dirty = original.dirty;
        }

        public int this[int x, int y, int z]
        {
            get => data[x + size.x * (y + size.y * z)];
            set
            {
                if (dirty || data[x + size.x * (y + size.y * z)] != value)
                {
                    data[x + size.x * (y + size.y * z)] = value;
                    dirty = true;
                }
            }
        }

        public void RegenerateMesh()
        {
            #region Declare Arrays

            vertices = new List<Vector3>();
            metal = new List<Vector2>();
            emissionRG = new List<Vector2>();
            emissionBA = new List<Vector2>();
            triangles = new List<int>();
            colors = new List<Color>();
            bubbles = new int[size.x, size.y, size.z];
            vols = new List<BoundsInt>();
            vol = new bool[size.x, size.y, size.z];

            #endregion

            #region Identify Bubbles
            bool BreadthFirstFill(int x, int y, int z, out List<Vector3Int> bubble)
            {
                bubble = null;

                //If bubble state is not unknown
                if (bubbles[x, y, z] != Unknown)
                    return false;

                //If pixel is not air
                if (this[x, y, z] != 0)
                {
                    bubbles[x, y, z] = Solid;
                    return false;
                }

                //If not at edge
                if (x <= lowerInset.x || y <= lowerInset.y || z <= lowerInset.z || x >= size.x - 1 - upperInset.x || y >= size.y - 1 - upperInset.y || z >= size.z - 1 - upperInset.z)
                {
                    bubbles[x, y, z] = NotInsideBubble;
                    return false;
                }

                bubble = new List<Vector3Int>();
                bool valid = true;
                Queue<Vector3Int> frontier = new Queue<Vector3Int>();
                bubbles[x, y, z] = BeingChecked;
                frontier.Enqueue(new Vector3Int(x, y, z));

                while (frontier.Count > 0)
                {
                    Vector3Int b = frontier.Dequeue();
                    bubble.Add(b);

                    for (int i = 0; i < 6; i++)
                    {
                        int nX = b.x + NX[i];
                        int nY = b.y + NY[i];
                        int nZ = b.z + NZ[i];

                        if (this[nX, nY, nZ] != 0)
                        {
                            bubbles[nX, nY, nZ] = Solid;
                            continue;
                        }

                        if (nX == 0 || nY == 0 || nZ == 0 || nX == size.x - 1 || nY == size.y - 1 || nZ == size.z - 1)
                        {
                            valid = false;
                            bubbles[nX, nY, nZ] = NotInsideBubble;
                            continue;
                        }

                        if (bubbles[nX, nY, nZ] == NotInsideBubble)
                        {
                            valid = false;
                            continue;
                        }

                        if (bubbles[nX, nY, nZ] != Unknown)
                            continue;

                        bubbles[nX, nY, nZ] = BeingChecked;
                        frontier.Enqueue(new Vector3Int(nX, nY, nZ));
                    }
                }
                foreach (Vector3Int b in bubble)
                {
                    if (valid)
                        bubbles[b.x, b.y, b.z] = InsideBubble;
                    else
                        bubbles[b.x, b.y, b.z] = NotInsideBubble;
                }
                return valid;
            }

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                        BreadthFirstFill(x, y, z, out _);
            #endregion

            #region Identify Volumes

            for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                        vol[x, y, z] = this[x, y, z] != 0 || bubbles[x, y, z] == InsideBubble;

            for (int x = lowerInset.y; x < size.x - upperInset.x; x++)
                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                    {
                        if (!vol[x, y, z])
                            continue;

                        int x1 = size.x - upperInset.x;
                        int y1 = size.y - upperInset.y;
                        int z1 = size.z - upperInset.z;

                        for (int x0 = x; x0 < x1; x0++)
                            for (int y0 = y; y0 < y1; y0++)
                                for (int z0 = z; z0 < z1; z0++)
                                    if (!vol[x0, y0, z0])
                                    {
                                        if (x0 > x)
                                            x1 = x0;
                                        else if (y0 > y)
                                            y1 = y0;
                                        else
                                            z1 = z0;

                                        break;
                                    }

                        for (int x2 = x; x2 < x1; x2++)
                            for (int y2 = y; y2 < y1; y2++)
                                for (int z2 = z; z2 < z1; z2++)
                                    vol[x2, y2, z2] = false;

                        vols.Add(new BoundsInt(x, y, z, x1 - x, y1 - y, z1 - z));
                    }

            volumes = vols.ToArray();

            #endregion

            #region Add Quad
            void AddQuad(Vector3[] corners, int index)
            {
                index--;

                if (index < 0 || index >= Palette.Length)
                    return;

                Voxel voxel = Palette[index];

                if (voxel == null)
                    return;

                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 3);

                vertices.Add(corners[0] * VoxelSize - origin);
                vertices.Add(corners[1] * VoxelSize - origin);
                vertices.Add(corners[2] * VoxelSize - origin);
                vertices.Add(corners[3] * VoxelSize - origin);

                Vector2 m = new Vector2(voxel.metallic, voxel.smoothness);
                Vector2 eRG = new Vector2(voxel.emission.r, voxel.emission.g);
                Vector2 eBA = new Vector2(voxel.emission.b, voxel.emission.a);

                colors.Add(voxel.color);
                colors.Add(voxel.color);
                colors.Add(voxel.color);
                colors.Add(voxel.color);

                metal.Add(m);
                metal.Add(m);
                metal.Add(m);
                metal.Add(m);

                emissionRG.Add(eRG);
                emissionRG.Add(eRG);
                emissionRG.Add(eRG);
                emissionRG.Add(eRG);

                emissionBA.Add(eBA);
                emissionBA.Add(eBA);
                emissionBA.Add(eBA);
                emissionBA.Add(eBA);
            }
            #endregion

            #region East Faces
            for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
            {
                faces = new int[size.y, size.z];

                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                        if (this[x, y, z] != 0 && (x == size.x - 1 - upperInset.x || (this[x + 1, y, z] == 0 && bubbles[x + 1, y, z] != InsideBubble)))
                            faces[y, z] = this[x, y, z];

                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                    {
                        if (faces[y, z] == 0)
                            continue;

                        int y1 = size.y - upperInset.y;
                        int z1 = size.z - upperInset.z;

                        for (int y0 = y; y0 < y1; y0++)
                            for (int z0 = z; z0 < z1; z0++)
                                if (faces[y0, z0] != faces[y, z])
                                {
                                    if (y0 > y)
                                        y1 = y0;
                                    else
                                        z1 = z0;

                                    break;
                                }

                        for (int y2 = y; y2 < y1; y2++)
                            for (int z2 = z; z2 < z1; z2++)
                                faces[y2, z2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x + 1, y, z),
                            new Vector3(x + 1, y, z1),
                            new Vector3(x + 1, y1, z1),
                            new Vector3(x + 1, y1, z)
                        }, this[x, y, z]);
                    }
            }
            #endregion

            #region West Faces
            for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
            {
                faces = new int[size.y, size.z];

                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                        if (this[x, y, z] != 0 && (x == 0 + lowerInset.x || (this[x - 1, y, z] == 0 && bubbles[x - 1, y, z] != InsideBubble)))
                            faces[y, z] = this[x, y, z];

                for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                    {
                        if (faces[y, z] == 0)
                            continue;

                        int y1 = size.y - upperInset.y;
                        int z1 = size.z - upperInset.z;

                        for (int y0 = y; y0 < y1; y0++)
                            for (int z0 = z; z0 < z1; z0++)
                                if (faces[y0, z0] != faces[y, z])
                                {
                                    if (y0 > y)
                                        y1 = y0;
                                    else
                                        z1 = z0;

                                    break;
                                }

                        for (int y2 = y; y2 < y1; y2++)
                            for (int z2 = z; z2 < z1; z2++)
                                faces[y2, z2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x + 0, y, z),
                            new Vector3(x + 0, y1, z),
                            new Vector3(x + 0, y1, z1),
                            new Vector3(x + 0, y, z1)
                        }, this[x, y, z]);
                    }
                }
            #endregion

            #region North Faces
            for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
            {
                faces = new int[size.x, size.y];
                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                        if (this[x, y, z] != 0 && (z == size.z - 1 - upperInset.z || (this[x, y, z + 1] == 0 && bubbles[x, y, z + 1] != InsideBubble)))
                            faces[x, y] = this[x, y, z];

                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    {
                        if (faces[x, y] == 0)
                            continue;

                        int x1 = size.x - upperInset.x;
                        int y1 = size.y - upperInset.y;

                        for (int x0 = x; x0 < x1; x0++)
                            for (int y0 = y; y0 < y1; y0++)
                                if (faces[x0, y0] != faces[x, y])
                                {
                                    if (x0 > x)
                                        x1 = x0;
                                    else
                                        y1 = y0;

                                    break;
                                }

                        for (int x2 = x; x2 < x1; x2++)
                            for (int y2 = y; y2 < y1; y2++)
                                faces[x2, y2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x, y, z + 1),
                            new Vector3(x, y1, z + 1),
                            new Vector3(x1, y1, z + 1),
                            new Vector3(x1, y, z + 1),
                        }, this[x, y, z]);
                    }
            }
            #endregion

            #region South Faces
            for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
            {
                faces = new int[size.x, size.y];
                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                        if (this[x, y, z] != 0 && (z == lowerInset.z || (this[x, y, z - 1] == 0 && bubbles[x, y, z - 1] != InsideBubble)))
                            faces[x, y] = this[x, y, z];

                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
                    {
                        if (faces[x, y] == 0)
                            continue;

                        int x1 = size.x;
                        int y1 = size.y;

                        for (int x0 = x; x0 < x1; x0++)
                            for (int y0 = y; y0 < y1; y0++)
                                if (faces[x0, y0] != faces[x, y])
                                {
                                    if (x0 > x)
                                        x1 = x0;
                                    else
                                        y1 = y0;

                                    break;
                                }

                        for (int x2 = x; x2 < x1; x2++)
                            for (int y2 = y; y2 < y1; y2++)
                                faces[x2, y2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x, y, z),
                            new Vector3(x1, y, z),
                            new Vector3(x1, y1, z),
                            new Vector3(x, y1, z),
                        }, this[x, y, z]);
                    }
            }
            #endregion

            #region Top Faces
            for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
            {
                faces = new int[size.x, size.z];
                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                        if (this[x, y, z] != 0 && (y == size.y - 1 - upperInset.y || (this[x, y + 1, z] == 0 && bubbles[x, y + 1, z] != InsideBubble)))
                            faces[x, z] = this[x, y, z];

                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                    {
                        if (faces[x, z] == 0)
                            continue;

                        int x1 = size.x - upperInset.x;
                        int z1 = size.z - upperInset.z;

                        for (int x0 = x; x0 < x1; x0++)
                            for (int z0 = z; z0 < z1; z0++)
                                if (faces[x0, z0] != faces[x, z])
                                {
                                    if (x0 > x)
                                        x1 = x0;
                                    else
                                        z1 = z0;
                                    break;
                                }

                        for (int x2 = x; x2 < x1; x2++)
                            for (int z2 = z; z2 < z1; z2++)
                                faces[x2, z2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x, y + 1, z),
                            new Vector3(x1, y + 1, z),
                            new Vector3(x1, y + 1, z1),
                            new Vector3(x, y + 1, z1),
                        }, this[x, y, z]);
                    }
            }
            #endregion

            #region Bottom Faces
            for (int y = lowerInset.y; y < size.y - upperInset.y; y++)
            {
                faces = new int[size.x, size.z];
                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                        if (this[x, y, z] != 0 && (y == lowerInset.y || (this[x, y - 1, z] == 0 && bubbles[x, y - 1, z] != InsideBubble)))
                            faces[x, z] = this[x, y, z];

                for (int x = lowerInset.x; x < size.x - upperInset.x; x++)
                    for (int z = lowerInset.z; z < size.z - upperInset.z; z++)
                    {
                        if (faces[x, z] == 0)
                            continue;

                        int x1 = size.x - upperInset.x;
                        int z1 = size.z - upperInset.z;

                        for (int x0 = x; x0 < x1; x0++)
                            for (int z0 = z; z0 < z1; z0++)
                                if (faces[x0, z0] != faces[x, z])
                                {
                                    if (x0 > x)
                                        x1 = x0;
                                    else
                                        z1 = z0;
                                    break;
                                }

                        for (int x2 = x; x2 < x1; x2++)
                            for (int z2 = z; z2 < z1; z2++)
                                faces[x2, z2] = 0;

                        AddQuad(new Vector3[] {
                            new Vector3(x, y, z),
                            new Vector3(x, y, z1),
                            new Vector3(x1, y, z1),
                            new Vector3(x1, y, z),
                        }, this[x, y, z]);
                    }
            }
            #endregion

            #region Build Mesh
            mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = metal.ToArray(),
                uv2 = emissionRG.ToArray(),
                uv3 = emissionBA.ToArray(),
                triangles = triangles.ToArray(),
                colors = colors.ToArray()
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            #endregion

            dirty = false;
        }
    }
}
