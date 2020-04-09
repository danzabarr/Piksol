using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public class Bone : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private BlockObject block;
        public MeshFilter MeshFilter
        {
            get
            {
                if (meshFilter == null)
                    meshFilter = GetComponent<MeshFilter>();
                return meshFilter;
            }
        }
        public BlockObject Block
        {
            get => block;
            set
            {
                block = value;
                MeshFilter.sharedMesh = block.Mesh;
            }
        }

        public void DrawCube()
        {
            BlockEditor.DrawCube(transform, -block.Origin + (Vector3)block.LowerInset * BlockObject.VoxelSize, (Vector3)block.InsetSize * BlockObject.VoxelSize);
        }
    }
}
