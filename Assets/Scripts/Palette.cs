using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    [System.Serializable]
    public class Palette 
    {
        private static Dictionary<string, Palette> database = new Dictionary<string, Palette>();

        public static readonly Palette Default = new Palette()
        {
            Name = "Default",
            voxels = new Voxel[]
            {
                new Voxel()
                {
                    color = Color.red,
                    smoothness = 0.5f,
                },

                new Voxel()
                {
                    color = Color.green,
                    smoothness = 0.5f,
                },

                new Voxel()
                {
                    color = Color.blue,
                    smoothness = 0.5f,
                },
            }
        };

        public static bool Load(string name, out Palette palette)
        {
            palette = Default;
            if (string.IsNullOrEmpty(name))
                return false;

            if (database.TryGetValue(name, out palette))
            {
                palette.Name = name;
                return true;
            }

            if (IO.LoadPalette(name, out palette))
            {
                palette.Name = name;
                database[name] = palette;
                return true;
            }

            return false;
        }
        public string Name { get; private set; }
        [SerializeField] private Voxel[] voxels;
        public Voxel this[int index] => voxels[index];
        public int Length => voxels.Length;
    }
}
