using UnityEngine;
using WankulCrazyPlugin.importer;
using WankulCrazyPlugin.utils;

namespace WankulCrazyPlugin.patch
{
    public class Saves
    {
        public static void Save()
        {
            SavesManager.ModSave();
        }

        public static void Load()
        {
            SavesManager.ModLoad();
        }
    }
}
