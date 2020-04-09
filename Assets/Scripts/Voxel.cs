using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    [System.Serializable]
    public class Voxel
    {
        public Color color;
        public float metallic;
        public float smoothness;
        [ColorUsage(false, true)] public Color emission;
    }
}
