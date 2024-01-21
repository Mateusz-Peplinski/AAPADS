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
            var dropTableCommand = new SQLiteCommand("DROP TABLE IF EXISTS settings;", connection);
            dropTableCommand.ExecuteNonQuery();

            var createTableCommand = new SQLiteCommand(
                @"CREATE TABLE settings (
                Key TEXT NOT NULL UNIQUE,
                Value TEXT
                );", connection);

            createTableCommand.ExecuteNonQuery();
        }



        public string GetSetting(string key)
        {
            var command = new SQLiteCommand("SELECT Value FROM settings WHERE Key = @Key LIMIT 1", connection);
            command.Parameters.AddWithValue("@Key", key);
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? (string)result : null;
        }

        public void SaveSetting(string key, string value)
        {
            var command = new SQLiteCommand("INSERT OR REPLACE INTO settings (Key, Value) VALUES (@Key, @Value)", connection);
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value);
            command.ExecuteNonQuery();
        }

        // Note: To use these methods safely, ensure that 'key' is never sourced from user input to prevent SQL injection.
        // In a real-world application, you'd want to use a more robust data access approach, such as an ORM.


        public void Dispose()
        {
            connection?.Dispose();
        }
    }

}
