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
        { NPCID.KingSlime, 1 << 0 },
        { NPCID.EyeofCthulhu, 1 << 1 },
        { NPCID.EaterofWorldsHead, 1 << 2 },
        { NPCID.EaterofWorldsBody, 1 << 2 },
        { NPCID.EaterofWorldsTail, 1 << 2 },
        { NPCID.BrainofCthulhu, 1 << 3 },
        { NPCID.QueenBee, 1 << 4 },
        { NPCID.SkeletronHead, 1 << 5 },
        { NPCID.Deerclops, 1 << 6 },
        { NPCID.WallofFlesh, 1 << 7 },
        { NPCID.QueenSlimeBoss, 1 << 8 },
        { NPCID.TheDestroyer, 1 << 9 },
        { NPCID.Retinazer, 1 << 10 },
        { NPCID.Spazmatism, 1 << 10 },
        { NPCID.SkeletronPrime, 1 << 11 },
        { NPCID.Plantera, 1 << 12 },
        { NPCID.Golem, 1 << 13 },
        { NPCID.DukeFishron, 1 << 14 },
        { NPCID.HallowBoss, 1 << 15 },
        { NPCID.CultistBoss, 1 << 16 },
        { NPCID.DD2Betsy, 1 << 17 },
        { NPCID.MoonLordCore, 1 << 18 }
    };

    /// <summary>
    /// Maps configuration keys to boss NPC IDs.
    /// </summary>
    public static readonly Dictionary<string, int[]> ConfigKeyToBossIds = new()
    {
        { "KingSlime", new[] { (int)NPCID.KingSlime } },
        { "EyeOfCthulhu", new[] { (int)NPCID.EyeofCthulhu } },
        { "EaterOfWorlds", new[] { (int)NPCID.EaterofWorldsHead } },
        { "BrainOfCthulhu", new[] { (int)NPCID.BrainofCthulhu } },
        { "QueenBee", new[] { (int)NPCID.QueenBee } },
        { "Skeletron", new[] { (int)NPCID.SkeletronHead } },
        { "Deerclops", new[] { (int)NPCID.Deerclops } },
        { "WallOfFlesh", new[] { (int)NPCID.WallofFlesh } },
        { "QueenSlime", new[] { (int)NPCID.QueenSlimeBoss } },
        { "TheDestroyer", new[] { (int)NPCID.TheDestroyer } },
        { "TheTwins", new[] { (int)NPCID.Retinazer, (int)NPCID.Spazmatism } },
        { "SkeletronPrime", new[] { (int)NPCID.SkeletronPrime } },
        { "Plantera", new[] { (int)NPCID.Plantera } },
        { "Golem", new[] { (int)NPCID.Golem } },
        { "DukeFishron", new[] { (int)NPCID.DukeFishron } },
        { "EmpressOfLight", new[] { (int)NPCID.HallowBoss } },
        { "LunaticCultist", new[] { (int)NPCID.CultistBoss } },
        { "Betsy", new[] { (int)NPCID.DD2Betsy } },
        { "MoonLord", new[] { (int)NPCID.MoonLordCore } }
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