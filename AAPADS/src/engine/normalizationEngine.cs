using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace AAPADS
{
    public class NormalizationEngine
    {
        private normalizationEngineDatabaseAccess _dbAccess;
        private string _lastProcessedTimeFrameId;

        public NormalizationEngine()
        {
            _dbAccess = new normalizationEngineDatabaseAccess("wireless_profile.db");
            Initialize();
            Task.Run(() => StartNormalization());
        }

        private void Initialize()
        {
            _lastProcessedTimeFrameId = _dbAccess.GetLastTimeFrameID();

            if (string.IsNullOrEmpty(_lastProcessedTimeFrameId) || !DataExistsForTimeFrameId("#A001"))
            {
                _lastProcessedTimeFrameId = "#A001";
            }
        }


        private void StartNormalization()
        {
            while (true) 
            {
                var nextTimeFrameId = GetNextTimeFrameId(_lastProcessedTimeFrameId);

                if (!string.IsNullOrEmpty(nextTimeFrameId))
                {
                    // Fetch the data for the nextTimeFrameId
                    var dataForTimeFrame = FetchDataForTimeFrame(nextTimeFrameId);

                    // Calculate averages or other statistical measures
                    var counts = CalculateAverages(dataForTimeFrame);

                    // Insert the calculated values into the NE_DB table
                    _dbAccess.InsertNormalizationEngineData(nextTimeFrameId, counts.AccessPointCount, counts.AP24GHzCount, counts.AP5GHzCount);

                    // Update the last processed TIME_FRAME_ID
                    _lastProcessedTimeFrameId = nextTimeFrameId;

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("[ NORMALIZATION ENGINE ] No new data sleeping for 10 seconds");
                    System.Threading.Thread.Sleep(10000); 
                }
            }
        }

        private string GetNextTimeFrameId(string currentId)
        {
            var idGenerator = new TimeFrameIdGenerator(currentId);
            var nextId = idGenerator.GenerateNextId();

            // Check if data exists for the next ID
            var dataExists = DataExistsForTimeFrameId(nextId);

            if (dataExists)
            {
                return nextId;
            }
            else
            {
                return null;
            }
        }

        private bool DataExistsForTimeFrameId(string timeFrameId)
        {
            string query = "SELECT COUNT(*) FROM WirelessProfile WHERE TIME_FRAME_ID = @TIME_FRAME_ID";
            var parameters = new { TIME_FRAME_ID = timeFrameId };

            int count = _dbAccess.Connection.ExecuteScalar<int>(query, parameters);

            return count > 0;
        }



        private List<WirelessData> FetchDataForTimeFrame(string timeFrameId)
        {
            
            string query = "SELECT * FROM WirelessProfile WHERE TIME_FRAME_ID = @TIME_FRAME_ID";
            var parameters = new { TIME_FRAME_ID = timeFrameId };

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[ NORMALIZATION ENGINE ] SQL READ from {timeFrameId}");

            var data = _dbAccess.Connection.Query<WirelessData>(query, parameters).ToList();

            return data;
        }


        private (int AccessPointCount, int AP24GHzCount, int AP5GHzCount) CalculateAverages(List<WirelessData> data)
        {
            //int total24GHzAPs = 0;

            //foreach (var accessPoint in data.)
            //{
            //    if (accessPoint == "2.4 GHz")
            //    {
            //        total24GHzAPs++;
            //    }
            //}

            return (0, 1, 2); // Placeholder
        }
    }

    public class WirelessData
    {
        public int ID { get; set; }
        public string Time { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public int SignalStrength { get; set; }
        public string WifiStandard { get; set; }
        public string Band { get; set; }
        public int Channel { get; set; }
        public string Frequency { get; set; }
        public string Authentication { get; set; }
        public string TimeFrameID { get; set; }
    }


}
