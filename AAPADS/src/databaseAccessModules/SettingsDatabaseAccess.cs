using System;
using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class SettingsDatabaseAccess : IDisposable
    {
        private SQLiteConnection connection;
        private readonly string dbPath;
        public SQLiteConnection Connection => connection;

        public SettingsDatabaseAccess(string dbFileName)
        {
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbFileName);
            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {
            var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS ""Settings"" (
                ""Key"" TEXT NOT NULL UNIQUE,
                ""Value"" TEXT,
                PRIMARY KEY(""Key"")
            );", connection);

            command.ExecuteNonQuery();
        }

        public string GetSetting(string key)
        {
            return connection.QueryFirstOrDefault<string>("SELECT Value FROM Settings WHERE Key = @Key", new { Key = key });
        }

        public void SaveSetting(string key, string value)
        {
            connection.Execute("INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@Key, @Value)", new { Key = key, Value = value });
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }

}
