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
                ListUnclaimedBosses(args.Player);
                break;
            case "reset":
                ResetProgress(args.Player);
                break;
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
        player.SendInfoMessage("/bag list - View unclaimed boss bags");
        player.SendInfoMessage("/bag claim - Claim all available boss bags");
        player.SendInfoMessage("/bag status - View your detailed status");
        
        if (player.HasPermission("bagger.admin"))
        {
            player.SendInfoMessage("/bag reset - Reset all claim data (admin)");
        }
    }

    private static void ListUnclaimedBosses(TSPlayer player)
    {
        var participated = Bagger.DB.GetParticipatedCounts(player.Name);
        var unclaimed = new List<string>();

        foreach (var (bossId, _) in Bagger.Config.BossKillCounts)
        {
            int unclaimedCount = BossHelper.GetUnclaimedCount(bossId, participated);
            if (unclaimedCount > 0)
            {
                string bossName = BossHelper.GetBossName(bossId);
                unclaimed.Add($"{bossName} ({unclaimedCount})");
            }
        }

        if (unclaimed.Count == 0)
        {
            player.SendSuccessMessage("[Bagger] You have no unclaimed boss bags.");
            return;
        }

        player.SendInfoMessage("[Bagger] Unclaimed Boss Bags:");
        player.SendSuccessMessage(string.Join(", ", unclaimed));
    }

    private static void ShowStatus(TSPlayer player)
    {
        var participated = Bagger.DB.GetParticipatedCounts(player.Name);
        var participatedList = new List<string>();
        var unclaimedList = new List<string>();

        foreach (var (bossId, totalKills) in Bagger.Config.BossKillCounts)
        {
            string bossName = BossHelper.GetBossName(bossId);
            int participatedCount = participated.TryGetValue(bossId, out var p) ? p : 0;
            int unclaimedCount = Math.Max(0, totalKills - participatedCount);

            if (participatedCount > 0)
            {
                participatedList.Add($"{bossName} ({participatedCount})");
            }

            if (unclaimedCount > 0)
            {
                unclaimedList.Add($"{bossName} ({unclaimedCount})");
            }
        }

        player.SendInfoMessage("[Bagger] Your Status:");

        if (participatedList.Count > 0)
            player.SendMessage($"[c/FF5F59:Participated:] {string.Join(", ", participatedList)}", Color.White);
        else
            player.SendMessage("[c/FF5F59:Participated:] None", Color.White);

        if (unclaimedList.Count > 0)
            player.SendMessage($"[c/7CFC00:Unclaimed:] {string.Join(", ", unclaimedList)}", Color.White);
        else
            player.SendMessage("[c/7CFC00:Unclaimed:] None", Color.White);
    }

    private static int GetFreeInventorySlots(TSPlayer player)
    {
        int freeSlots = 0;
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
        var participated = Bagger.DB.GetParticipatedCounts(player.Name);
        var config = Bagger.Config;

        var bossRewards = new Dictionary<int, List<Configuration.ItemData>>
        {
            { NPCID.KingSlime, config.KingSlimeDrop },
            { NPCID.EyeofCthulhu, config.EyeOfCthulhuDrop },
            { NPCID.EaterofWorldsHead, config.EaterOfWorldsDrop },
            { NPCID.BrainofCthulhu, config.BrainOfCthulhuDrop },
            { NPCID.QueenBee, config.QueenBeeDrop },
            { NPCID.SkeletronHead, config.SkeletronDrop },
            { NPCID.Deerclops, config.DeerclopsDrop },
            { NPCID.WallofFlesh, config.WallOfFleshDrop },
            { NPCID.QueenSlimeBoss, config.QueenSlimeDrop },
            { NPCID.TheDestroyer, config.TheDestroyerDrop },
            { NPCID.Retinazer, config.TheTwinsDrop },
            { NPCID.SkeletronPrime, config.SkeletronPrimeDrop },
            { NPCID.Plantera, config.PlanteraDrop },
            { NPCID.Golem, config.GolemDrop },
            { NPCID.DukeFishron, config.DukeFishronDrop },
            { NPCID.HallowBoss, config.EmpressOfLightDrop },
            { NPCID.CultistBoss, config.LunaticCultistDrop },
            { NPCID.DD2Betsy, config.BetsyDrop },
            { NPCID.MoonLordCore, config.MoonLordDrop }
        };

        var toClaim = new List<(int bossId, int count, List<Configuration.ItemData> items)>();
        int totalItemSlots = 0;

        foreach (var (bossId, items) in bossRewards)
        {
            if (items == null || items.Count == 0)
                continue;

            int unclaimedCount = BossHelper.GetUnclaimedCount(bossId, participated);
            if (unclaimedCount <= 0)
                continue;

            toClaim.Add((bossId, unclaimedCount, items));
            totalItemSlots += items.Count * unclaimedCount;
        }

        if (toClaim.Count == 0)
        {
            player.SendWarningMessage("[Bagger] No boss bags available to claim.");
            return;
        }

        int freeSlots = GetFreeInventorySlots(player);
        if (freeSlots < totalItemSlots)
        {
            player.SendErrorMessage($"[Bagger] Not enough inventory space!");
            player.SendErrorMessage($"Required: {totalItemSlots} slot(s), Available: {freeSlots} slot(s)");
            player.SendInfoMessage("Please free up inventory space and try again.");
            return;
        }

        int totalBagsClaimed = 0;

        foreach (var (bossId, count, items) in toClaim)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var item in items)
                {
                    player.GiveItem(item.ID, item.Stack);
                }
            }

            if (!participated.ContainsKey(bossId))
                participated[bossId] = 0;
            
            participated[bossId] += count;
            totalBagsClaimed += count;

            string bossName = BossHelper.GetBossName(bossId);
            player.SendSuccessMessage($"[Bagger] Claimed {bossName} x{count}!");
        }

        Bagger.DB.SavePlayer(player.Name, participated);
        player.SendSuccessMessage($"[Bagger] Successfully claimed {totalBagsClaimed} boss bag(s)!");
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
            Bagger.Config.BossKillCounts.Clear();
            Bagger.Config.Save();
            player.SendSuccessMessage("[Bagger] All progress has been reset.");
        }
        else
        {
            player.SendErrorMessage("[Bagger] Failed to reset progress.");
        }
    }
}