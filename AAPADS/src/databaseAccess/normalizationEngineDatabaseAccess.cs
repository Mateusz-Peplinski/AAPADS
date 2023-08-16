using Dapper;
using System;
using System.Data.SQLite;
using System.IO;

namespace AAPADS
{
    public class normalizationEngineDatabaseAccess : IDisposable
    {
        private SQLiteConnection connection;
        private readonly string dbPath;
        public SQLiteConnection Connection => connection;

        public normalizationEngineDatabaseAccess(string dbFileName)
        {
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbFileName);


            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            CreateTablesIfNotExists();
        }
        private void CreateTablesIfNotExists()
        {
            var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS ""NE_DB"" (
            ""ID"" INTEGER NOT NULL UNIQUE,
            ""TIME_FRAME_ID"" TEXT,
            ""AP_COUNT"" TEXT,
            ""AP2_4GHZ_AP_COUNT"" TEXT,
            ""AP5_GHZ_AP_COUNT"" TEXT,
            PRIMARY KEY(""ID"" AUTOINCREMENT)
            );", connection);

            command.ExecuteNonQuery();
        }
        public void InsertNormalizationEngineData(string timeFrameID, int AccessPointCount, int AP24GHzCount, int AP5GHzCount)
        {
            var normalizedData = new
            {
                TIME_FRAME_ID = timeFrameID,
                AP_COUNT = AccessPointCount,
                AP2_4GHZ_AP_COUNT = AP24GHzCount,
                AP5_GHZ_AP_COUNT = AP5GHzCount,
            };
            connection.Execute("INSERT INTO NE_DB (TIME_FRAME_ID, AP_COUNT, AP2_4GHZ_AP_COUNT, AP5_GHZ_AP_COUNT) VALUES (@TIME_FRAME_ID, @AP_COUNT, @AP2_4GHZ_AP_COUNT, @AP5_GHZ_AP_COUNT)", normalizedData);
        }


        public string GetLastTimeFrameID()
        {
            var result = connection.QueryFirstOrDefault<string>("SELECT TIME_FRAME_ID FROM NE_DB ORDER BY ID DESC LIMIT 1");
            return result ?? "A0";
        }
        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
