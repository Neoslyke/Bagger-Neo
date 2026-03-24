using Microsoft.Data.Sqlite;
using TShockAPI;

namespace Bagger;

public class Database
{
    private static readonly string DbPath = Path.Combine(TShock.SavePath, "Bagger.sqlite");
    private readonly string _connString = $"Data Source={DbPath}";

    public Database()
    {
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            CreateTable();
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Bagger] Database error: {ex.Message}");
        }
    }

    private void CreateTable()
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Players (
                Name TEXT PRIMARY KEY,
                ClaimedBossesMask INTEGER DEFAULT 0
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public int GetClaimedBossMask(string name)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ClaimedBossesMask FROM Players WHERE Name = @name;";
        cmd.Parameters.AddWithValue("@name", name);

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public bool InsertPlayer(string name, int mask = 0)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Players (Name, ClaimedBossesMask) 
            VALUES (@name, @mask);
        ";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@mask", mask);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool SavePlayer(string name, int mask)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO Players (Name, ClaimedBossesMask) 
            VALUES (@name, @mask);
        ";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@mask", mask);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool IsPlayerInDb(string name)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Players WHERE Name = @name;";
        cmd.Parameters.AddWithValue("@name", name);

        var result = cmd.ExecuteScalar();
        return result != null && Convert.ToInt32(result) > 0;
    }

    public bool ClearData()
    {
        try
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Players;";
            cmd.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[Bagger] Clear data error: {ex.Message}");
            return false;
        }
    }
}