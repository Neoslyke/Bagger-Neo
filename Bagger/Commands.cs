using Microsoft.Xna.Framework;
using Terraria;
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
        
        if (player.HasPermission("bagger.admin"))
        {
            player.SendInfoMessage("/bag reset - Reset all claim data (admin)");
        }
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

    private static int GetFreeInventorySlots(TSPlayer player)
    {
        int freeSlots = 0;
        // Main inventory slots 0-49 (excluding hotbar would be 10-49, but we include all main inventory)
        for (int i = 0; i < 50; i++)
        {
            var item = player.TPlayer.inventory[i];
            if (item == null || item.IsAir || item.type == ItemID.None)
            {
                freeSlots++;
            }
        }
        return freeSlots;
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

        var config = Bagger.Config;

        var bossRewards = new Dictionary<string, (int[] bossIds, List<Configuration.ItemData> items)>
        {
            { "King Slime", (new[] { (int)NPCID.KingSlime }, config.KingSlimeDrop) },
            { "Eye of Cthulhu", (new[] { (int)NPCID.EyeofCthulhu }, config.EyeOfCthulhuDrop) },
            { "Eater of Worlds", (new[] { (int)NPCID.EaterofWorldsHead }, config.EaterOfWorldsDrop) },
            { "Brain of Cthulhu", (new[] { (int)NPCID.BrainofCthulhu }, config.BrainOfCthulhuDrop) },
            { "Queen Bee", (new[] { (int)NPCID.QueenBee }, config.QueenBeeDrop) },
            { "Skeletron", (new[] { (int)NPCID.SkeletronHead }, config.SkeletronDrop) },
            { "Deerclops", (new[] { (int)NPCID.Deerclops }, config.DeerclopsDrop) },
            { "Wall of Flesh", (new[] { (int)NPCID.WallofFlesh }, config.WallOfFleshDrop) },
            { "Queen Slime", (new[] { (int)NPCID.QueenSlimeBoss }, config.QueenSlimeDrop) },
            { "The Destroyer", (new[] { (int)NPCID.TheDestroyer }, config.TheDestroyerDrop) },
            { "The Twins", (new[] { (int)NPCID.Retinazer, (int)NPCID.Spazmatism }, config.TheTwinsDrop) },
            { "Skeletron Prime", (new[] { (int)NPCID.SkeletronPrime }, config.SkeletronPrimeDrop) },
            { "Plantera", (new[] { (int)NPCID.Plantera }, config.PlanteraDrop) },
            { "Golem", (new[] { (int)NPCID.Golem }, config.GolemDrop) },
            { "Duke Fishron", (new[] { (int)NPCID.DukeFishron }, config.DukeFishronDrop) },
            { "Empress of Light", (new[] { (int)NPCID.HallowBoss }, config.EmpressOfLightDrop) },
            { "Lunatic Cultist", (new[] { (int)NPCID.CultistBoss }, config.LunaticCultistDrop) },
            { "Betsy", (new[] { (int)NPCID.DD2Betsy }, config.BetsyDrop) },
            { "Moon Lord", (new[] { (int)NPCID.MoonLordCore }, config.MoonLordDrop) }
        };

        // First pass: determine what can be claimed and count required slots
        var toClaim = new List<(string bossName, int bossMask, List<Configuration.ItemData> items)>();
        int totalItemSlots = 0;

        foreach (var (bossName, (bossIds, items)) in bossRewards)
        {
            var primaryBossId = bossIds[0];
            var bossMask = BossHelper.GetBossMask(primaryBossId);

            // Skip if already claimed
            if ((mask & bossMask) == bossMask)
                continue;

            // Check if boss is defeated
            var isDefeated = bossIds.Length > 1 
                ? bossIds.All(id => config.DownedBosses.Contains(id))
                : config.DownedBosses.Contains(primaryBossId);

            if (!isDefeated)
                continue;

            // Skip if no items configured
            if (items == null || items.Count == 0)
                continue;

            toClaim.Add((bossName, bossMask, items));
            totalItemSlots += items.Count;
        }

        // Check if there's anything to claim
        if (toClaim.Count == 0)
        {
            player.SendWarningMessage("[Bagger] No new boss bags available to claim.");
            player.SendInfoMessage("You may have already claimed them or participated in the fights.");
            return;
        }

        // Check inventory space
        int freeSlots = GetFreeInventorySlots(player);
        if (freeSlots < totalItemSlots)
        {
            player.SendErrorMessage($"[Bagger] Not enough inventory space!");
            player.SendErrorMessage($"Required: {totalItemSlots} slot(s), Available: {freeSlots} slot(s)");
            player.SendInfoMessage("Please free up inventory space and try again.");
            return;
        }

        // Second pass: actually give items
        var claimedCount = 0;
        foreach (var (bossName, bossMask, items) in toClaim)
        {
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

        player.SendSuccessMessage($"[Bagger] Successfully claimed {claimedCount} boss bag(s)!");
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