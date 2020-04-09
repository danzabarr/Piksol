using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public enum RotationMode
    {
        Fixed,
        AroundYAxis,
        AlignToFace
    }

    public class Block
    {
        private int[] textures = new int[6];

        public static int RotateIndex(int face, int rotation, RotationMode mode)
        {
            switch (mode)
            {
                case RotationMode.Fixed:
                    return face;
                case RotationMode.AroundYAxis:



                    return rotation % 4;
                case RotationMode.AlignToFace:
                    break;
            }

            return face;
        }

        public bool IsTransparent { get; set; }
        public bool IsCollidable { get; set; }
        public RotationMode RotationMode { get; set; }
        public int GetTextureID(int index) => textures[index];
        public void SetTextureID(int index, int id) => textures[index] = id;
        public int GetTextureID(int face, int rotation) => textures[RotateIndex(face, rotation, RotationMode)];
    }
}
