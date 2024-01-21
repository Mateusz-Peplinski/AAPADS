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
                @"CREATE TABLE IF NOT EXISTS ""NormWirelessProfile"" (
            ""ID"" INTEGER NOT NULL UNIQUE,
            ""TIME_FRAME_ID"" TEXT,
            ""TIME"" TEXT,
            ""AP_COUNT"" TEXT,
            ""AP2_4GHZ_AP_COUNT"" TEXT,
            ""AP5_GHZ_AP_COUNT"" TEXT,
            PRIMARY KEY(""ID"" AUTOINCREMENT)
            );", connection);

            var knownBSSIDSCommand = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS ""KnownBSSIDS"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""SSID"" TEXT,
                ""BSSID"" TEXT,
                ""FIRST_DETECTED_TIME"" TEXT,
                ""FIRST_DETECTED_DATE"" TEXT,
                UNIQUE(SSID, BSSID),
                PRIMARY KEY(""ID"" AUTOINCREMENT)
                );", connection);

            knownBSSIDSCommand.ExecuteNonQuery();
            command.ExecuteNonQuery();
        }
        // ###############################################################
        // ############     TABLE NormWirelessProfile       ##############
        // ###############################################################
        public void InsertNormalizationEngineData(string timeFrameID, string timeFRAMEIDTime, int AccessPointCount, int AP24GHzCount, int AP5GHzCount)
        {
            var normalizedData = new
            {
                TIME_FRAME_ID = timeFrameID,
                TIME = timeFRAMEIDTime,
                AP_COUNT = AccessPointCount,
                AP2_4GHZ_AP_COUNT = AP24GHzCount,
                AP5_GHZ_AP_COUNT = AP5GHzCount,
            };
            connection.Execute("INSERT INTO NormWirelessProfile (TIME_FRAME_ID, TIME, AP_COUNT, AP2_4GHZ_AP_COUNT, AP5_GHZ_AP_COUNT) VALUES (@TIME_FRAME_ID, @TIME, @AP_COUNT, @AP2_4GHZ_AP_COUNT, @AP5_GHZ_AP_COUNT)", normalizedData);
        }

        // ###############################################################
        // ############          TABLE KnownBSSIDS          ##############
        // ###############################################################
        public void InsertSsidBssiIfDoesNotExist(string ssid, string bssid)
        {
            var ssidBssidExists = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM KnownBSSIDS WHERE SSID = @SSID AND BSSID = @BSSID", new { SSID = ssid, BSSID = bssid });

            if (ssidBssidExists == 0)
            {
                connection.Execute("INSERT INTO KnownBSSIDS (SSID, BSSID, FIRST_DETECTED_TIME, FIRST_DETECTED_DATE) VALUES (@SSID, @BSSID, @FIRST_DETECTED_TIME, @FIRST_DETECTED_DATE )", new { SSID = ssid, BSSID = bssid, FIRST_DETECTED_TIME = DateTime.Now.ToString("hh:mm:ss tt"), FIRST_DETECTED_DATE = DateTime.Now.ToShortDateString() });
            }

        }


        public string GetLastTimeFrameID()
        {
            var result = connection.QueryFirstOrDefault<string>("SELECT TIME_FRAME_ID FROM NormWirelessProfile ORDER BY ID DESC LIMIT 1");
            return result ?? "A0";
        }
        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
