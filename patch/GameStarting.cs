using WankulCrazyPlugin.cards;
using WankulCrazyPlugin.importer;

namespace WankulCrazyPlugin.patch;

public class GameStarting
{
    public static void Start()
    {
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

