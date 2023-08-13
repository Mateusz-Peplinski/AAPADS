using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class wirelessProfileDatabaseAccess : IDisposable
    {
        private SQLiteConnection connection;
        private readonly string dbPath;

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
            PRIMARY KEY(""ID"" AUTOINCREMENT)
            );", connection);

            command.ExecuteNonQuery();
        }

        public void InsertWifiData(string ssid, string bssid, int signalStrength,
                                   string wifiStandard, string band, int channel, string frequency, string auth)
        {
            var wifiData = new
            {
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

            connection.Execute("INSERT INTO WirelessProfile (Time, SSID, BSSID, SIGNAL_STRENGTH, WIFI_STANDARD, BAND, CHANNEL, FREQUENCY, AUTHENTICATION) VALUES (@Time, @SSID, @BSSID, @SIGNAL_STRENGTH, @WIFI_STANDARD, @BAND, @CHANNEL, @FREQUENCY, @AUTHENTICATION)", wifiData);
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
