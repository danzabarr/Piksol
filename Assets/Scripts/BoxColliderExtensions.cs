using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{

    public static class BoxColliderExtensions
    {
        public static void Set(this BoxCollider collider, Vector3 center, Vector3 size)
        {
            collider.center = center;
            collider.size = size;
        }
        public static void Set(this BoxCollider collider, BlockObject block)
        {
            collider.center = (block.LowerInset + (Vector3)block.InsetSize / 2) * BlockObject.VoxelSize - block.Origin;
            collider.size = (Vector3)block.InsetSize * BlockObject.VoxelSize;
        }
    }
}
