using Mirror;
using Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public class World : MonoBehaviour
    {
        public static World Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<World>();
                return instance;
            }
        }

        private static World instance;

        public NoiseSettings settings;

        private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

        private void Update()
        {
            foreach (Chunk chunk in chunks.Values)
            {
                chunk.DrawMesh();
                chunk.DrawObjects();
            }
        }

        private void OnValidate()
        {
            foreach (Chunk chunk in chunks.Values)
                chunk.GenerateTerrain();
        }
        public int ChunksCount => chunks.Count;
        public static Vector3Int World2Block(Vector3 world) => Vector3Int.FloorToInt(world);
        public static Vector3Int Block2Chunk(Vector3 block) => Vector3Int.FloorToInt(block / 16f);

        public int GetBlock(Vector3Int block)
        {
            Vector3Int chunk = Block2Chunk(block);

            if (!TryGetChunk(chunk.x, chunk.z, out Chunk c))
                return -1;

            block.x -= chunk.x * 16;
            block.z -= chunk.z * 16;

            if (block.x < 0 || block.x >= Chunk.Size.x)
                return -1;
            if (block.y < 0 || block.y >= Chunk.Size.y)
                return -1;
            if (block.z < 0 || block.z >= Chunk.Size.x)
                return -1;

            return c[block.x, block.y, block.z];
        }

        public static bool IsTransparent(int blockID)
        {
            return blockID == 0;
        }

        public bool SetBlock(Vector3Int block, int id)
        {
            Vector3Int chunk = Block2Chunk(block);

            if (!TryGetChunk(chunk.x, chunk.z, out Chunk c))
                return false;

            block.x -= chunk.x * 16;
            block.z -= chunk.z * 16;

            c[block.x, block.y, block.z] = id;

            if (IsTransparent(id))
            {
                if (block.x == 0)
                {
                    if (TryGetChunk(chunk.x - 1, chunk.z, out Chunk w))
                    {
                        int adjacent = w[Chunk.Size.x - 1, block.y, block.z];
                        if (!IsTransparent(adjacent))
                            w.SetDirty();
                    }
                }
                else if (block.x == Chunk.Size.x - 1)
                {
                    if (TryGetChunk(chunk.x + 1, chunk.z, out Chunk e))
                    {
                        int adjacent = e[0, block.y, block.z];
                        if (!IsTransparent(adjacent))
                            e.SetDirty();
                    }
                }
                if (block.z == 0)
                {
                    if (TryGetChunk(chunk.x, chunk.z - 1, out Chunk s))
                    {
                        int adjacent = s[block.x, block.y, Chunk.Size.z - 1];
                        if (!IsTransparent(adjacent))
                            s.SetDirty();
                    }
                }
                else if (block.z == Chunk.Size.z - 1)
                {
                    if (TryGetChunk(chunk.x, chunk.z + 1, out Chunk n))
                    {
                        int adjacent = n[block.x, block.y, 0];
                        if (!IsTransparent(adjacent))
                            n.SetDirty();
                    }
                }
            }
            return true;
        }

        public bool TryGetChunk(int x, int y, out Chunk chunk)
        {
            return chunks.TryGetValue(new Vector2Int(x, y), out chunk);
        }

        public Chunk GetOrCreateChunk(int x, int y)
        {
            if (chunks.TryGetValue(new Vector2Int(x, y), out Chunk chunk))
                return chunk;
            else
            {
                chunk = new Chunk(x, y);
                chunks.Add(new Vector2Int(x, y), chunk);
                return chunk;
            }
        }

        public void WriteToChunk(int x, int y, int[] blockData, int index)
        {
            if (chunks.TryGetValue(new Vector2Int(x, y), out Chunk chunk))
                chunk.SetData(blockData, index);

            else
            {
                chunk = new Chunk(x, y, blockData, index);
                chunks.Add(new Vector2Int(x, y), chunk);
            }
        }

        public int[] RequestChunkData(int x, int y, int index, int length)
        {
            Chunk chunk = GetOrCreateChunk(x, y);

            return chunk.GetData(index, length);
        }
    }
}
