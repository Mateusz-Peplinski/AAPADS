using Dapper;
using System;
using System.Data.SQLite;
using System.IO;

namespace AAPADS
{
    public class wirelessProfileDatabaseAccess : IDisposable
    {
        private SQLiteConnection connection;
        private readonly string dbPath;
        public SQLiteConnection Connection => connection;

        public wirelessProfileDatabaseAccess(string dbFileName)
        {
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbFileName);


            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();


            CreateTablesIfNotExists();
        }
        private void CreateTablesIfNotExists()
        {
            var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS ""WirelessProfile"" (
            ""ID"" INTEGER NOT NULL UNIQUE,
            ""Time"" TEXT NOT NULL,
            ""SSID"" TEXT,
            ""BSSID"" TEXT,
            ""SIGNAL_STRENGTH"" INTEGER,
            ""WIFI_STANDARD"" TEXT,
            ""BAND"" TEXT,
            ""CHANNEL"" INTEGER,
            ""FREQUENCY"" TEXT,
            ""AUTHENTICATION""	TEXT,
            ""TIME_FRAME_ID"" TEXT,
            PRIMARY KEY(""ID"" AUTOINCREMENT)
            );", connection);

            command.ExecuteNonQuery();
        }

        public void DATA_INGEST_ENGINE_INSERT_ACCESS_POINT_DATA(string ssid, string bssid, int signalStrength,
                                   string wifiStandard, string band, int channel, string frequency, string auth, string timeFrameID)
        {
            var wifiData = new
            {
                TIME_FRAME_ID = timeFrameID,
                Time = DateTime.Now.ToString("HH:mm:ss dd:MM:yyyy"),
                SSID = ssid,
                BSSID = bssid,
                SIGNAL_STRENGTH = signalStrength,
                WIFI_STANDARD = wifiStandard,
                BAND = band,
                CHANNEL = channel,
                FREQUENCY = frequency,
                AUTHENTICATION = auth
            };

            connection.Execute("INSERT INTO WirelessProfile (TIME_FRAME_ID, Time, SSID, BSSID, SIGNAL_STRENGTH, WIFI_STANDARD, BAND, CHANNEL, FREQUENCY, AUTHENTICATION) VALUES (@TIME_FRAME_ID, @Time, @SSID, @BSSID, @SIGNAL_STRENGTH, @WIFI_STANDARD, @BAND, @CHANNEL, @FREQUENCY, @AUTHENTICATION)", wifiData);
        }
        public string GetLastTimeFrameId()
        {
            var result = connection.QueryFirstOrDefault<string>("SELECT TIME_FRAME_ID FROM WirelessProfile ORDER BY ID DESC LIMIT 1");
            return result ?? "A0";
        }
        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
