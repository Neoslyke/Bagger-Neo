using Terraria.ID;

namespace Bagger;

public static class BossHelper
{
    public static readonly Dictionary<int, int> BossToPrimary = new()
    {
        { NPCID.KingSlime, NPCID.KingSlime },
        { NPCID.EyeofCthulhu, NPCID.EyeofCthulhu },
        { NPCID.EaterofWorldsHead, NPCID.EaterofWorldsHead },
        { NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead },
        { NPCID.EaterofWorldsTail, NPCID.EaterofWorldsHead },
        { NPCID.BrainofCthulhu, NPCID.BrainofCthulhu },
        { NPCID.QueenBee, NPCID.QueenBee },
        { NPCID.SkeletronHead, NPCID.SkeletronHead },
        { NPCID.Deerclops, NPCID.Deerclops },
        { NPCID.WallofFlesh, NPCID.WallofFlesh },
        { NPCID.QueenSlimeBoss, NPCID.QueenSlimeBoss },
        { NPCID.TheDestroyer, NPCID.TheDestroyer },
        { NPCID.Retinazer, NPCID.Retinazer },
        { NPCID.Spazmatism, NPCID.Retinazer },
        { NPCID.SkeletronPrime, NPCID.SkeletronPrime },
        { NPCID.Plantera, NPCID.Plantera },
        { NPCID.Golem, NPCID.Golem },
        { NPCID.DukeFishron, NPCID.DukeFishron },
        { NPCID.HallowBoss, NPCID.HallowBoss },
        { NPCID.CultistBoss, NPCID.CultistBoss },
        { NPCID.DD2Betsy, NPCID.DD2Betsy },
        { NPCID.MoonLordCore, NPCID.MoonLordCore }
    };

    public static readonly Dictionary<string, int> ConfigKeyToBossId = new()
    {
        { "KingSlime", NPCID.KingSlime },
        { "EyeOfCthulhu", NPCID.EyeofCthulhu },
        { "EaterOfWorlds", NPCID.EaterofWorldsHead },
        { "BrainOfCthulhu", NPCID.BrainofCthulhu },
        { "QueenBee", NPCID.QueenBee },
        { "Skeletron", NPCID.SkeletronHead },
        { "Deerclops", NPCID.Deerclops },
        { "WallOfFlesh", NPCID.WallofFlesh },
        { "QueenSlime", NPCID.QueenSlimeBoss },
        { "TheDestroyer", NPCID.TheDestroyer },
        { "TheTwins", NPCID.Retinazer },
        { "SkeletronPrime", NPCID.SkeletronPrime },
        { "Plantera", NPCID.Plantera },
        { "Golem", NPCID.Golem },
        { "DukeFishron", NPCID.DukeFishron },
        { "EmpressOfLight", NPCID.HallowBoss },
        { "LunaticCultist", NPCID.CultistBoss },
        { "Betsy", NPCID.DD2Betsy },
        { "MoonLord", NPCID.MoonLordCore }
    };

    public static readonly Dictionary<int, string> BossIdToName = new()
    {
        { NPCID.KingSlime, "King Slime" },
        { NPCID.EyeofCthulhu, "Eye of Cthulhu" },
        { NPCID.EaterofWorldsHead, "Eater of Worlds" },
        { NPCID.BrainofCthulhu, "Brain of Cthulhu" },
        { NPCID.QueenBee, "Queen Bee" },
        { NPCID.SkeletronHead, "Skeletron" },
        { NPCID.Deerclops, "Deerclops" },
        { NPCID.WallofFlesh, "Wall of Flesh" },
        { NPCID.QueenSlimeBoss, "Queen Slime" },
        { NPCID.TheDestroyer, "The Destroyer" },
        { NPCID.Retinazer, "The Twins" },
        { NPCID.SkeletronPrime, "Skeletron Prime" },
        { NPCID.Plantera, "Plantera" },
        { NPCID.Golem, "Golem" },
        { NPCID.DukeFishron, "Duke Fishron" },
        { NPCID.HallowBoss, "Empress of Light" },
        { NPCID.CultistBoss, "Lunatic Cultist" },
        { NPCID.DD2Betsy, "Betsy" },
        { NPCID.MoonLordCore, "Moon Lord" }
    };

    public static int GetPrimaryBossId(int npcType)
    {
        return BossToPrimary.TryGetValue(npcType, out var primary) ? primary : 0;
    }

    public static string GetBossName(int bossId)
    {
        return BossIdToName.TryGetValue(bossId, out var name) ? name : $"Unknown ({bossId})";
    }

    public static int GetUnclaimedCount(int bossId, Dictionary<int, int> participated)
    {
        int totalKills = Bagger.Config.BossKillCounts.TryGetValue(bossId, out var total) ? total : 0;
        int participatedKills = participated.TryGetValue(bossId, out var p) ? p : 0;
        return Math.Max(0, totalKills - participatedKills);
    }
}