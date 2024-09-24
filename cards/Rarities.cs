using System.Collections.Generic;

namespace WankulCrazyPlugin.cards
{
    public static class RaritiesContainer
    {
        public static Dictionary<Rarity, string> Rarities = new Dictionary<Rarity, string>
        {
            { Rarity.C, "Commune" },
            { Rarity.UC, "Peu Commune" },
            { Rarity.R, "Rare" },
            { Rarity.UR1, "Ultra rare holo 1" },
            { Rarity.UR2, "Ultra rare holo 2" },
            { Rarity.LB, "Légendaire Bronze" },
            { Rarity.LA, "Légendaire Argent" },
            { Rarity.LO, "Légendaire Or" },
            { Rarity.PGW23, "PGW 2023" },
            { Rarity.NOEL23, "Noël 2023" },
            { Rarity.SPCIV, "Starter Pack Civilisations" },
            { Rarity.SPLEG, "Starter Pack Légendes" },
            { Rarity.ED, "Edition Speciale" },
            { Rarity.SPPOP, "Starter Pack Pop-Culture" },
            { Rarity.GP, "Gemmes Pack" },
            { Rarity.SPTV, "Starter Pack TV" },
            { Rarity.SPJV, "Starter Pack Jeux Vidéo" },
            { Rarity.EG, "Edition Gold" },
            { Rarity.SPCAR, "Starter Pack Carrières" },
            { Rarity.TOR, "Gagnant Ticket Or" }
        };
    }
}
