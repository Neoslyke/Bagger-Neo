using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Bagger;

[ApiVersion(2, 1)]
public class Bagger : TerrariaPlugin
{
    public override string Name => "Bagger";
    public override string Author => "Neoslyke, Soofa, 羽学";
    public override Version Version => new(2, 2, 0);
    public override string Description => "Allows players who missed boss fights to claim boss bags.";

    internal static Database DB = null!;
    internal static Configuration Config = null!;

    public Bagger(Main game) : base(game) { }

    public override void Initialize()
    {
        DB = new Database();
        Config = Configuration.Load();

        GeneralHooks.ReloadEvent += OnReload;
        ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
        ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
        
        TShockAPI.Commands.ChatCommands.Add(new Command("bagger.use", Commands.HandleCommand, "bag", "bagger")
        {
            HelpText = "Claim boss bags you missed."
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= OnReload;
            ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
            ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
            TShockAPI.Commands.ChatCommands.RemoveAll(c => c.Names.Contains("bag") || c.Names.Contains("bagger"));
        }
        base.Dispose(disposing);
    }

    private static void OnReload(ReloadEventArgs args)
    {
        Config = Configuration.Load();
        args.Player?.SendSuccessMessage("[Bagger] Configuration reloaded.");
    }

    private void OnGamePostInitialize(EventArgs args)
    {
        SyncDownedBosses();
        Config.Save();
    }

    private void SyncDownedBosses()
    {
        var bossChecks = new Dictionary<int, bool>
        {
            { NPCID.KingSlime, NPC.downedSlimeKing },
            { NPCID.EyeofCthulhu, NPC.downedBoss1 },
            { NPCID.EaterofWorldsHead, NPC.downedBoss2 && !WorldGen.crimson },
            { NPCID.BrainofCthulhu, NPC.downedBoss2 && WorldGen.crimson },
            { NPCID.QueenBee, NPC.downedQueenBee },
            { NPCID.SkeletronHead, NPC.downedBoss3 },
            { NPCID.Deerclops, NPC.downedDeerclops },
            { NPCID.WallofFlesh, Main.hardMode },
            { NPCID.QueenSlimeBoss, NPC.downedQueenSlime },
            { NPCID.TheDestroyer, NPC.downedMechBoss1 },
            { NPCID.Retinazer, NPC.downedMechBoss2 },
            { NPCID.Spazmatism, NPC.downedMechBoss2 },
            { NPCID.SkeletronPrime, NPC.downedMechBoss3 },
            { NPCID.Plantera, NPC.downedPlantBoss },
            { NPCID.Golem, NPC.downedGolemBoss },
            { NPCID.DukeFishron, NPC.downedFishron },
            { NPCID.HallowBoss, NPC.downedEmpressOfLight },
            { NPCID.CultistBoss, NPC.downedAncientCultist },
            { NPCID.DD2Betsy, DD2Event.DownedInvasionT3 },
            { NPCID.MoonLordCore, NPC.downedMoonlord }
        };

        foreach (var (npcId, isDefeated) in bossChecks)
        {
            if (isDefeated && IsBestiaryUnlocked(npcId) && !Config.BossKillCounts.ContainsKey(npcId))
            {
                Config.BossKillCounts[npcId] = 1;
            }
        }
    }

    private void OnNpcKilled(NpcKilledEventArgs args)
    {
        var npc = args.npc;
        
        if (!npc.boss || !IsBestiaryUnlocked(npc.type))
            return;

        int bossType = BossHelper.GetPrimaryBossId(npc.type);
        
        if (bossType == 0)
            return;

        if (!Config.BossKillCounts.ContainsKey(bossType))
            Config.BossKillCounts[bossType] = 0;
        
        Config.BossKillCounts[bossType]++;
        Config.Save();

        foreach (var player in TShock.Players.Where(p => p?.Active == true))
        {
            var participated = DB.GetParticipatedCounts(player.Name);
            
            if (!participated.ContainsKey(bossType))
                participated[bossType] = 0;
            
            participated[bossType]++;
            DB.SavePlayer(player.Name, participated);
        }
    }

    private static bool IsBestiaryUnlocked(int npcId)
    {
        var entry = Main.BestiaryDB.FindEntryByNPCID(npcId);
        if (entry == null) return false;
        
        var unlockState = entry.UIInfoProvider.GetEntryUICollectionInfo().UnlockState;
        return unlockState == Terraria.GameContent.Bestiary.BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
    }
}