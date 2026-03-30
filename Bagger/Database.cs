using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
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
                ParticipatedCounts TEXT DEFAULT '{}'
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public Dictionary<int, int> GetParticipatedCounts(string name)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ParticipatedCounts FROM Players WHERE Name = @name;";
        cmd.Parameters.AddWithValue("@name", name);

        var result = cmd.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<int, int>>(result.ToString()!) ?? new Dictionary<int, int>();
            }
            catch
            {
                return new Dictionary<int, int>();
            }
        }
        return new Dictionary<int, int>();
    }

    public bool SavePlayer(string name, Dictionary<int, int> participatedCounts)
    {
        using var conn = new SqliteConnection(_connString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO Players (Name, ParticipatedCounts) 
            VALUES (@name, @counts);
        ";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@counts", JsonConvert.SerializeObject(participatedCounts));

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