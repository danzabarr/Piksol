using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Piksol
{
    public static class IO
    {
        public static readonly string PalettesDirectory = Application.dataPath + "/Palettes/";
        public static readonly string PalettesExtension = ".txt";

        public static readonly string BlocksDirectory = Application.dataPath + "/Blocks/";
        public static readonly string BlocksExtension = ".txt";

        public static bool LoadPalette(string name, out Palette palette)
            => Load(PalettesDirectory, name + PalettesExtension, out palette);

        public static bool SavePalette(string name, Palette palette, bool overwrite)
            => Save(PalettesDirectory, name + PalettesExtension, palette, overwrite);

        public static bool LoadBlock(string name, out BlockObject block)
            => Load(BlocksDirectory, name + BlocksExtension, out block);

        public static bool SaveBlock(string name, BlockObject block, bool overwrite)
            => Save(BlocksDirectory, name + BlocksExtension, block, overwrite);

        public static bool Load<T>(string directoryPath, string nameAndExtension, out T obj)
        {
            obj = default;

            if (directoryPath == null || nameAndExtension == null)
                return false;

            if (!Directory.Exists(directoryPath))
            {
                DirectoryInfo info = Directory.CreateDirectory(directoryPath);

                if (!info.Exists)
                    return false;
            }

            string path = directoryPath + nameAndExtension;

            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);

            if (json == null)
                return false;

            obj = JsonUtility.FromJson<T>(json);

            return obj != null;
        }

        public static bool Save<T>(string directoryPath, string nameAndExtension, T obj, bool overwrite)
        {
            if (directoryPath == null || nameAndExtension == null)
                return false;

            if (obj == null)
                return false;

            if (!Directory.Exists(directoryPath))
            {
                DirectoryInfo info = Directory.CreateDirectory(directoryPath);

                if (!info.Exists)
                    return false;
            }

            string path = directoryPath + nameAndExtension;

            if (!overwrite && File.Exists(path))
                return false;

            string json = JsonUtility.ToJson(obj);

            if (json == null)
                return false;

            File.WriteAllText(path, json);

            return true;
        }
    }
}
