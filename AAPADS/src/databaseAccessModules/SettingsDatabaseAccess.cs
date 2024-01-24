using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

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
            var command = connection.CreateCommand();
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS settings (
                Key TEXT NOT NULL UNIQUE,
                Value TEXT
              );";
            command.ExecuteNonQuery();
        }



        public string GetSetting(string key)
        {
            return connection.QueryFirstOrDefault<string>("SELECT Value FROM settings WHERE Key = @Key", new { Key = key });
        }

        public void SaveSetting(string key, string value)
        {
            connection.Execute("INSERT OR REPLACE INTO settings (Key, Value) VALUES (@Key, @Value)", new { Key = key, Value = value });
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }

}
