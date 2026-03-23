using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace Bagger;

public class DatabaseManager
{
    private const string TableName = "Bagger";

    public DatabaseManager()
    {
        var sql = TShock.DB.GetSqlType() == SqlType.Sqlite
            ? $@"CREATE TABLE IF NOT EXISTS {TableName} (
                    Name TEXT PRIMARY KEY,
                    ClaimedBossesMask INTEGER DEFAULT 0
                )"
            : $@"CREATE TABLE IF NOT EXISTS {TableName} (
                    Name VARCHAR(50) PRIMARY KEY,
                    ClaimedBossesMask INT DEFAULT 0
                )";

        TShock.DB.Query(sql);
    }

    public int GetClaimedBossMask(string name)
    {
        using var reader = TShock.DB.QueryReader($"SELECT ClaimedBossesMask FROM {TableName} WHERE Name = @0", name);
        
        if (reader.Read())
            return reader.Get<int>("ClaimedBossesMask");
        
        return 0;
    }

    public bool InsertPlayer(string name, int mask = 0)
    {
        return TShock.DB.Query($"INSERT INTO {TableName} (Name, ClaimedBossesMask) VALUES (@0, @1)", name, mask) != 0;
    }

    public bool SavePlayer(string name, int mask)
    {
        return TShock.DB.Query($"UPDATE {TableName} SET ClaimedBossesMask = @0 WHERE Name = @1", mask, name) != 0;
    }

    public bool IsPlayerInDb(string name)
    {
        return TShock.DB.QueryScalar<int>($"SELECT COUNT(*) FROM {TableName} WHERE Name = @0", name) > 0;
    }

    public bool ClearData()
    {
        return TShock.DB.Query($"DELETE FROM {TableName}") >= 0;
    }
}