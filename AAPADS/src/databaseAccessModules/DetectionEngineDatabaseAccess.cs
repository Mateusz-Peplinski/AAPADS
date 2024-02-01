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
    public class DetectionEngineDatabaseAccess : IDisposable
    {
        private SQLiteConnection connection;
        private readonly string dbPath;
        public SQLiteConnection Connection => connection;

        public DetectionEngineDatabaseAccess(String dbFileName)
        {
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbFileName);

            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {
            var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS ""DetectionData"" (
                    ""ID"" INTEGER NOT NULL UNIQUE,
                    ""TIME_FRAME_ID"" TEXT,
                    ""CRITICALITY_LEVEL"" TEXT,
                    ""RISK_LEVEL"" INTEGER,
                    ""DETECTION_STATUS"" TEXT,
                    ""DETECTION_TIME"" TEXT,
                    ""DETECTION_TITLE"" TEXT,
                    ""DETECTION_DESCRIPTION"" TEXT,
                    ""DETECTION_REMEDIATION"" TEXT,
                    ""DETECTION_ACCESS_POINT_SSID"" TEXT,
                    ""DETECTION_ACCESS_POINT_MAC_ADDRESS"" TEXT,
                    ""DETECTION_ACCESS_POINT_SIGNAL_STRENGTH"" TEXT,
                    ""DETECTION_ACCESS_POINT_OPEN_CHANNEL"" TEXT,
                    ""DETECTION_ACCESS_POINT_FREQUENCY"" TEXT,
                    ""DETECTION_ACCESS_POINT_IS_STILL_ACTIVE"" TEXT,
                    ""DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED"" TEXT,
                    ""DETECTION_ACCESS_POINT_ENCRYPTION"" TEXT,
                    ""DETECTION_ACCESS_POINT_CONNECTED_CLIENTS"" TEXT,
                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    );", connection);

            command.ExecuteNonQuery();
        }

        public void SaveDetectionData(DetectionEvent detectionEvent)
        {
            var sql = @"INSERT INTO DetectionData (
                    CRITICALITY_LEVEL, RISK_LEVEL, DETECTION_STATUS, DETECTION_TIME, DETECTION_TITLE, 
                    DETECTION_DESCRIPTION, DETECTION_REMEDIATION, DETECTION_ACCESS_POINT_SSID, DETECTION_ACCESS_POINT_MAC_ADDRESS, 
                    DETECTION_ACCESS_POINT_SIGNAL_STRENGTH, DETECTION_ACCESS_POINT_OPEN_CHANNEL, DETECTION_ACCESS_POINT_FREQUENCY, 
                    DETECTION_ACCESS_POINT_IS_STILL_ACTIVE, DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED, DETECTION_ACCESS_POINT_ENCRYPTION, 
                    DETECTION_ACCESS_POINT_CONNECTED_CLIENTS) 
                VALUES (
                    @CriticalityLevel, @RiskLevel, @DetectionStatus, @DetectionTime, @DetectionTitle, 
                    @DetectionDescription, @DetectionRemediation, @DetectionAccessPointSsid, @DetectionAccessPointMacAddress, 
                    @DetectionAccessPointSignalStrength, @DetectionAccessPointOpenChannel, @DetectionAccessPointFrequency, 
                    @DetectionAccessPointIsStillActive, @DetectionAccessPointTimeFirstDetected, @DetectionAccessPointEncryption, 
                    @DetectionAccessPointConnectedClients)";

            connection.Execute(sql, detectionEvent);
        }

        public IEnumerable<DetectionEvent> FetchAllDetectionData()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var sql = @"SELECT * FROM DetectionData";
            return connection.Query<DetectionEvent>(sql);
        }
        public void Dispose()
        {
            // Dispose the SQLite connection when this object is disposed
            connection?.Dispose();
        }
    }
}
