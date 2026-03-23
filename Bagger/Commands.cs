using Microsoft.Xna.Framework;
using Terraria.ID;
using TShockAPI;

namespace Bagger;

internal static class Commands
{
    public static void HandleCommand(CommandArgs args)
    {
        var subcmd = args.Parameters.Count > 0 ? args.Parameters[0].ToLower() : "help";

        switch (subcmd)
        {
            case "list":
                ListDefeatedBosses(args.Player);
                break;
            case "reset":
                ResetProgress(args.Player);
                break;
            case "all":
            case "claim":
                ClaimAllBags(args.Player);
                break;
            case "status":
                ShowStatus(args.Player);
                break;
            default:
                ShowHelp(args.Player);
                break;
        }
    }

    private static void ShowHelp(TSPlayer player)
    {
        player.SendMessage("[Bagger] Commands:", Color.GreenYellow);
        player.SendInfoMessage("/bag list - View defeated bosses");
        player.SendInfoMessage("/bag all - Claim all available boss bags");
        player.SendInfoMessage("/bag status - View your claim status");
        player.SendInfoMessage("/bag reset - Reset all claim data (admin)");
    }

    private static void ListDefeatedBosses(TSPlayer player)
    {
        if (Bagger.Config.DownedBosses.Count == 0)
        {
            player.SendWarningMessage("[Bagger] No bosses have been defeated yet.");
            return;
        }

        var bossNames = Bagger.Config.DownedBosses
            .Select(id => TShock.Utils.GetNPCById(id)?.FullName ?? $"Unknown ({id})")
            .Distinct();

        player.SendInfoMessage("[Bagger] Defeated Bosses:");
        player.SendSuccessMessage(string.Join(", ", bossNames));
    }

    private static void ShowStatus(TSPlayer player)
    {
        if (!Bagger.DB.IsPlayerInDb(player.Name))
        {
            player.SendInfoMessage("[Bagger] You have not claimed any boss bags yet.");
            player.SendInfoMessage($"Available to claim: {GetUnclaimedBossCount(0)} boss(es)");
            return;
        }

        var mask = Bagger.DB.GetClaimedBossMask(player.Name);
        var claimed = new List<string>();
        var available = new List<string>();

        foreach (var (configKey, bossIds) in BossHelper.ConfigKeyToBossIds)
        {
            var primaryBossId = bossIds[0];
            var bossName = TShock.Utils.GetNPCById(primaryBossId)?.FullName ?? configKey;

            if (!Bagger.Config.DownedBosses.Intersect(bossIds).Any())
                continue;

            if (BossHelper.HasClaimedBoss(mask, primaryBossId))
                claimed.Add(bossName);
            else
                available.Add(bossName);
        }

        player.SendInfoMessage("[Bagger] Your Status:");
        
        if (claimed.Count > 0)
            player.SendMessage($"[c/FF5F59:Claimed/Participated:] {string.Join(", ", claimed)}", Color.White);
        
        if (available.Count > 0)
            player.SendMessage($"[c/7CFC00:Available to claim:] {string.Join(", ", available)}", Color.White);
        else
            player.SendSuccessMessage("You have claimed all available boss bags!");
    }

    private static void ClaimAllBags(TSPlayer player)
    {
        if (Bagger.Config.DownedBosses.Count == 0)
        {
            player.SendWarningMessage("[Bagger] No bosses have been defeated yet.");
            return;
        }

        var mask = Bagger.DB.IsPlayerInDb(player.Name) 
            ? Bagger.DB.GetClaimedBossMask(player.Name) 
            : 0;

        var claimedCount = 0;
        var config = Bagger.Config;

        // Process each boss reward
        var bossRewards = new Dictionary<string, (int[] bossIds, List<Configuration.ItemData> items)>
        {
            { "King Slime", (new[] { NPCID.KingSlime }, config.KingSlimeDrop) },
            { "Eye of Cthulhu", (new[] { NPCID.EyeofCthulhu }, config.EyeOfCthulhuDrop) },
            { "Eater of Worlds", (new[] { NPCID.EaterofWorldsHead }, config.EaterOfWorldsDrop) },
            { "Brain of Cthulhu", (new[] { NPCID.BrainofCthulhu }, config.BrainOfCthulhuDrop) },
            { "Queen Bee", (new[] { NPCID.QueenBee }, config.QueenBeeDrop) },
            { "Skeletron", (new[] { NPCID.SkeletronHead }, config.SkeletronDrop) },
            { "Deerclops", (new[] { NPCID.Deerclops }, config.DeerclopsDrop) },
            { "Wall of Flesh", (new[] { NPCID.WallofFlesh }, config.WallOfFleshDrop) },
            { "Queen Slime", (new[] { NPCID.QueenSlimeBoss }, config.QueenSlimeDrop) },
            { "The Destroyer", (new[] { NPCID.TheDestroyer }, config.TheDestroyerDrop) },
            { "The Twins", (new[] { NPCID.Retinazer, NPCID.Spazmatism }, config.TheTwinsDrop) },
            { "Skeletron Prime", (new[] { NPCID.SkeletronPrime }, config.SkeletronPrimeDrop) },
            { "Plantera", (new[] { NPCID.Plantera }, config.PlanteraDrop) },
            { "Golem", (new[] { NPCID.Golem }, config.GolemDrop) },
            { "Duke Fishron", (new[] { NPCID.DukeFishron }, config.DukeFishronDrop) },
            { "Empress of Light", (new[] { NPCID.HallowBoss }, config.EmpressOfLightDrop) },
            { "Lunatic Cultist", (new[] { NPCID.CultistBoss }, config.LunaticCultistDrop) },
            { "Betsy", (new[] { NPCID.DD2Betsy }, config.BetsyDrop) },
            { "Moon Lord", (new[] { NPCID.MoonLordCore }, config.MoonLordDrop) }
        };

        foreach (var (bossName, (bossIds, items)) in bossRewards)
        {
            var primaryBossId = bossIds[0];
            var bossMask = BossHelper.GetBossMask(primaryBossId);

            // Skip if already claimed
            if ((mask & bossMask) == bossMask)
                continue;

            // Check if boss is defeated (for Twins, check both)
            var isDefeated = bossIds.Length > 1 
                ? bossIds.All(id => config.DownedBosses.Contains(id))
                : config.DownedBosses.Contains(primaryBossId);

            if (!isDefeated)
                continue;

            // Give items
            foreach (var item in items)
            {
                player.GiveItem(item.ID, item.Stack);
            }

            mask |= bossMask;
            claimedCount++;
            
            player.SendSuccessMessage($"[Bagger] Claimed {bossName} rewards!");
        }

        // Save to database
        if (Bagger.DB.IsPlayerInDb(player.Name))
            Bagger.DB.SavePlayer(player.Name, mask);
        else
            Bagger.DB.InsertPlayer(player.Name, mask);

        if (claimedCount == 0)
        {
            player.SendWarningMessage("[Bagger] No new boss bags available to claim.");
            player.SendInfoMessage("You may have already claimed them or participated in the fights.");
        }
        else
        {
            player.SendSuccessMessage($"[Bagger] Successfully claimed {claimedCount} boss bag(s)!");
        }
    }

    private static void ResetProgress(TSPlayer player)
    {
        if (!player.HasPermission("bagger.admin"))
        {
            player.SendErrorMessage("You don't have permission to reset Bagger progress.");
            return;
        }

        if (Bagger.DB.ClearData())
        {
            Bagger.Config.DownedBosses.Clear();
            Bagger.Config.Save();
            player.SendSuccessMessage("[Bagger] All progress has been reset.");
        }
        else
        {
            player.SendErrorMessage("[Bagger] Failed to reset progress.");
        }
    }

    private static int GetUnclaimedBossCount(int mask)
    {
        var count = 0;
        foreach (var (_, bossIds) in BossHelper.ConfigKeyToBossIds)
        {
            var primaryBossId = bossIds[0];
            if (Bagger.Config.DownedBosses.Contains(primaryBossId) && 
                !BossHelper.HasClaimedBoss(mask, primaryBossId))
            {
                count++;
            }
        }
        return count;
    }
}