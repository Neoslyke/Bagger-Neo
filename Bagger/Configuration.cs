using Newtonsoft.Json;
using Terraria.ID;
using TShockAPI;

namespace Bagger;

public class Configuration
{
    private static readonly string ConfigPath = Path.Combine(TShock.SavePath, "Bagger.json");

    [JsonProperty("BossKillCounts")]
    public Dictionary<int, int> BossKillCounts { get; set; } = new();

    [JsonProperty("KingSlime")]
    public List<ItemData> KingSlimeDrop { get; set; } = new();

    [JsonProperty("EyeOfCthulhu")]
    public List<ItemData> EyeOfCthulhuDrop { get; set; } = new();

    [JsonProperty("EaterOfWorlds")]
    public List<ItemData> EaterOfWorldsDrop { get; set; } = new();

    [JsonProperty("BrainOfCthulhu")]
    public List<ItemData> BrainOfCthulhuDrop { get; set; } = new();

    [JsonProperty("QueenBee")]
    public List<ItemData> QueenBeeDrop { get; set; } = new();

    [JsonProperty("Skeletron")]
    public List<ItemData> SkeletronDrop { get; set; } = new();

    [JsonProperty("Deerclops")]
    public List<ItemData> DeerclopsDrop { get; set; } = new();

    [JsonProperty("WallOfFlesh")]
    public List<ItemData> WallOfFleshDrop { get; set; } = new();

    [JsonProperty("QueenSlime")]
    public List<ItemData> QueenSlimeDrop { get; set; } = new();

    [JsonProperty("TheDestroyer")]
    public List<ItemData> TheDestroyerDrop { get; set; } = new();

    [JsonProperty("TheTwins")]
    public List<ItemData> TheTwinsDrop { get; set; } = new();

    [JsonProperty("SkeletronPrime")]
    public List<ItemData> SkeletronPrimeDrop { get; set; } = new();

    [JsonProperty("Plantera")]
    public List<ItemData> PlanteraDrop { get; set; } = new();

    [JsonProperty("Golem")]
    public List<ItemData> GolemDrop { get; set; } = new();

    [JsonProperty("DukeFishron")]
    public List<ItemData> DukeFishronDrop { get; set; } = new();

    [JsonProperty("EmpressOfLight")]
    public List<ItemData> EmpressOfLightDrop { get; set; } = new();

    [JsonProperty("LunaticCultist")]
    public List<ItemData> LunaticCultistDrop { get; set; } = new();

    [JsonProperty("Betsy")]
    public List<ItemData> BetsyDrop { get; set; } = new();

    [JsonProperty("MoonLord")]
    public List<ItemData> MoonLordDrop { get; set; } = new();

    public static Configuration Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                var config = new Configuration();
                config.SetDefaults();
                config.Save();
                return config;
            }

            var json = File.ReadAllText(ConfigPath);
            return JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Bagger] Config load error: {ex.Message}");
            var config = new Configuration();
            config.SetDefaults();
            return config;
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Bagger] Config save error: {ex.Message}");
        }
    }

    private void SetDefaults()
    {
        KingSlimeDrop = new List<ItemData>
        {
            new(ItemID.KingSlimeBossBag, 1),
            new(ItemID.Solidifier, 1)
        };

        EyeOfCthulhuDrop = new List<ItemData>
        {
            new(ItemID.EyeOfCthulhuBossBag, 1)
        };

        EaterOfWorldsDrop = new List<ItemData>
        {
            new(ItemID.EaterOfWorldsBossBag, 1)
        };

        BrainOfCthulhuDrop = new List<ItemData>
        {
            new(ItemID.BrainOfCthulhuBossBag, 1)
        };

        QueenBeeDrop = new List<ItemData>
        {
            new(ItemID.QueenBeeBossBag, 1)
        };

        SkeletronDrop = new List<ItemData>
        {
            new(ItemID.SkeletronBossBag, 1)
        };

        DeerclopsDrop = new List<ItemData>
        {
            new(ItemID.DeerclopsBossBag, 1)
        };

        WallOfFleshDrop = new List<ItemData>
        {
            new(ItemID.WallOfFleshBossBag, 1),
            new(ItemID.Pwnhammer, 1)
        };

        QueenSlimeDrop = new List<ItemData>
        {
            new(ItemID.QueenSlimeBossBag, 1),
            new(ItemID.GreaterHealingPotion, 50)
        };

        TheDestroyerDrop = new List<ItemData>
        {
            new(ItemID.DestroyerBossBag, 1),
            new(ItemID.HallowedBar, 30)
        };

        TheTwinsDrop = new List<ItemData>
        {
            new(ItemID.TwinsBossBag, 1),
            new(ItemID.HallowedBar, 30)
        };

        SkeletronPrimeDrop = new List<ItemData>
        {
            new(ItemID.SkeletronPrimeBossBag, 1),
            new(ItemID.HallowedBar, 30)
        };

        PlanteraDrop = new List<ItemData>
        {
            new(ItemID.PlanteraBossBag, 1)
        };

        GolemDrop = new List<ItemData>
        {
            new(ItemID.GolemBossBag, 1),
            new(ItemID.Picksaw, 1)
        };

        DukeFishronDrop = new List<ItemData>
        {
            new(ItemID.FishronBossBag, 1)
        };

        EmpressOfLightDrop = new List<ItemData>
        {
            new(ItemID.FairyQueenBossBag, 1)
        };

        LunaticCultistDrop = new List<ItemData>
        {
            new(ItemID.CultistBossBag, 1),
            new(ItemID.LunarCraftingStation, 1)
        };

        BetsyDrop = new List<ItemData>
        {
            new(ItemID.BossBagBetsy, 1)
        };

        MoonLordDrop = new List<ItemData>
        {
            new(ItemID.MoonLordBossBag, 1)
        };
    }

    public class ItemData
    {
        [JsonProperty("ItemID")]
        public int ID { get; set; }

        [JsonProperty("Stack")]
        public int Stack { get; set; }

        public ItemData() { }

        public ItemData(int id, int stack)
        {
            ID = id;
            Stack = stack;
        }
    }
}