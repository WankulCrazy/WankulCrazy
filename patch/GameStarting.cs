using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.importer;

namespace WankulCrazyPlugin.patch;

public class GameStarting
{
    public static void OnLevelFinishedLoading(CGameManager __instance)
    {
        if (__instance.m_IsGameLevel) {
            PatchTexturesImporter.ReplaceGameTextures("shared1");
            OBJImporter.DoReplace();
        }
        else
        {
            PatchTexturesImporter.ReplaceGameTextures("shared0");
            OBJImporter.InitFiles();

            if (WankulCardsData.Instance.cards.Count == 0)
            {
                PatchTexturesImporter.ReplaceGameTextures("shared0");
                // Import JSON data
                JsonImporter.ImportJson();
                Plugin.Logger.LogInfo("JSON data imported");
            }
            else
            {
                Plugin.Logger.LogInfo("JSON data already imported");
            }
        }
    }
}

