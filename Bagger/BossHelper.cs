using Terraria.ID;

namespace Bagger;

/// <summary>
/// Helper class for boss-related operations.
/// </summary>
public static class BossHelper
{
    /// <summary>
    /// Maps boss NPC IDs to their bitmask values.
    /// </summary>
    public static readonly Dictionary<int, int> BossMasks = new()
    {
        { NPCID.KingSlime, 1 << 0 },           // 1
        { NPCID.EyeofCthulhu, 1 << 1 },        // 2
        { NPCID.EaterofWorldsHead, 1 << 2 },  // 4
        { NPCID.EaterofWorldsBody, 1 << 2 },  // 4
        { NPCID.EaterofWorldsTail, 1 << 2 },  // 4
        { NPCID.BrainofCthulhu, 1 << 3 },     // 8
        { NPCID.QueenBee, 1 << 4 },           // 16
        { NPCID.SkeletronHead, 1 << 5 },      // 32
        { NPCID.Deerclops, 1 << 6 },          // 64
        { NPCID.WallofFlesh, 1 << 7 },        // 128
        { NPCID.QueenSlimeBoss, 1 << 8 },     // 256
        { NPCID.TheDestroyer, 1 << 9 },       // 512
        { NPCID.Retinazer, 1 << 10 },         // 1024
        { NPCID.Spazmatism, 1 << 10 },        // 1024
        { NPCID.SkeletronPrime, 1 << 11 },    // 2048
        { NPCID.Plantera, 1 << 12 },          // 4096
        { NPCID.Golem, 1 << 13 },             // 8192
        { NPCID.DukeFishron, 1 << 14 },       // 16384
        { NPCID.HallowBoss, 1 << 15 },        // 32768
        { NPCID.CultistBoss, 1 << 16 },       // 65536
        { NPCID.DD2Betsy, 1 << 17 },          // 131072
        { NPCID.MoonLordCore, 1 << 18 }       // 262144
    };

    /// <summary>
    /// Maps configuration keys to boss NPC IDs.
    /// </summary>
    public static readonly Dictionary<string, int[]> ConfigKeyToBossIds = new()
    {
        { "KingSlime", new[] { NPCID.KingSlime } },
        { "EyeOfCthulhu", new[] { NPCID.EyeofCthulhu } },
        { "EaterOfWorlds", new[] { NPCID.EaterofWorldsHead } },
        { "BrainOfCthulhu", new[] { NPCID.BrainofCthulhu } },
        { "QueenBee", new[] { NPCID.QueenBee } },
        { "Skeletron", new[] { NPCID.SkeletronHead } },
        { "Deerclops", new[] { NPCID.Deerclops } },
        { "WallOfFlesh", new[] { NPCID.WallofFlesh } },
        { "QueenSlime", new[] { NPCID.QueenSlimeBoss } },
        { "TheDestroyer", new[] { NPCID.TheDestroyer } },
        { "TheTwins", new[] { NPCID.Retinazer, NPCID.Spazmatism } },
        { "SkeletronPrime", new[] { NPCID.SkeletronPrime } },
        { "Plantera", new[] { NPCID.Plantera } },
        { "Golem", new[] { NPCID.Golem } },
        { "DukeFishron", new[] { NPCID.DukeFishron } },
        { "EmpressOfLight", new[] { NPCID.HallowBoss } },
        { "LunaticCultist", new[] { NPCID.CultistBoss } },
        { "Betsy", new[] { NPCID.DD2Betsy } },
        { "MoonLord", new[] { NPCID.MoonLordCore } }
    };

    public static int AddBossToMask(int mask, int npcType)
    {
        if (BossMasks.TryGetValue(npcType, out var bossMask))
            return mask | bossMask;
        return mask;
    }

    public static bool HasClaimedBoss(int mask, int npcType)
    {
        if (BossMasks.TryGetValue(npcType, out var bossMask))
            return (mask & bossMask) == bossMask;
        return false;
    }

    public static int GetBossMask(int npcType)
    {
        return BossMasks.TryGetValue(npcType, out var mask) ? mask : 0;
    }
}