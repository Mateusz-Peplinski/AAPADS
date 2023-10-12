using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Task.Run(() => START_NORMALIZATION_ENGINE());
        }

        private void Initialize()
        {
            _lastProcessedTimeFrameId = _dbAccess.GetLastTimeFrameID();

            if (string.IsNullOrEmpty(_lastProcessedTimeFrameId) || !DataExistsForTimeFrameId("#A001"))
            {
                _lastProcessedTimeFrameId = "#A001";
            }
        }


        private void START_NORMALIZATION_ENGINE()
        {
            while (true)
            {
                var nextTimeFrameId = GetNextTimeFrameId(_lastProcessedTimeFrameId);

                if (!string.IsNullOrEmpty(nextTimeFrameId))
                {
                    // Fetch the data for the nextTimeFrameId
                    var dataForTimeFrame = FetchDataForTimeFrame(nextTimeFrameId);

                    // Calculate averages or other statistical measures
                    var NormalizedData = Normalize(dataForTimeFrame);

                    // Insert the calculated values into the NE_DB table
                    _dbAccess.InsertNormalizationEngineData(nextTimeFrameId, NormalizedData.timeFRAMEIDTime, NormalizedData.AccessPointCount, NormalizedData.AP24GHzCount, NormalizedData.AP5GHzCount);

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


        private (int AccessPointCount, string timeFRAMEIDTime, int AP24GHzCount, int AP5GHzCount) Normalize(List<WirelessData> data)
        {
            int totalAPs = 0;
            int total24GHzAPs = 0;
            int total5GHzAPs = 0;
            string time = "NULL";

            List<string> knownSSIDsList = new List<string>();
            string commaSeparatedKnownSSIDS = "";

            foreach (var accessPoint in data) // All the colleced data --> 
            {

                if (((accessPoint.SignalStrength / 2) - 100) < 60) //Only care about SSID that have approx RSSI value of 60 or less

                _dbAccess.InsertSSIDAndBSSIDIfDoesNotExist(accessPoint.SSID, accessPoint.BSSID); // If new data add it TABLE knownSsids-->


                {
                    if (accessPoint.Band == "2.4 GHz")
                    {
                        total24GHzAPs++;
                    }
                    if (accessPoint.Band == "5 GHz")
                    {
                        total5GHzAPs++;
                    }

                    totalAPs++;
                    time = accessPoint.Time;
                }

            }

            commaSeparatedKnownSSIDS = string.Join(",", knownSSIDsList);
            return (totalAPs, time, total24GHzAPs, total5GHzAPs, commaSeparatedKnownSSIDS);

            return (totalAPs, time, total24GHzAPs, total5GHzAPs); // Retunt counts that are used as Avgs 
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
