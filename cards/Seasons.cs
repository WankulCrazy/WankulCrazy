using System.Collections.Generic;

namespace WankulCrazyPlugin.cards
{
    public static class SeasonsContainer
    {
        public static Dictionary<Season, string> Seasons = new Dictionary<Season, string>
        {
            { Season.S01, "Origins" },
            { Season.S02, "Campus" },
            { Season.S03, "Battle" },
            { Season.HS, "Hors Serie" }
        };
    }
}