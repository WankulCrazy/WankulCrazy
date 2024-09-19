using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UIElements;
using Logger = HarmonyLib.Tools.Logger;

namespace WankulCrazyPlugin.patch;

public class GameStarting
{

    static bool Start(TitleScreen __instance) {
        // Import JSON data
        JsonImporter.ImportJson();
        Plugin.Logger.LogInfo("JSON data imported");
        return true;
    }
}

