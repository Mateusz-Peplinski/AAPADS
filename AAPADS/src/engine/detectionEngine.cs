using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace AAPADS.src.engine
{
    public class DetectionEngine
    {
        public List<string> CRITICALITY_LEVEL = new List<string>();
        public List<int> RISK_LEVEL = new List<int>();
        public List<string> DETECTION_STATUS = new List<string>();
        public List<string> DETECTION_TIME = new List<string>();
        public List<string> DETECTION_TITLE = new List<string>();
        public List<string> DETECTION_DESCRIPTION = new List<string>();
        public List<string> DETECTION_REMEDIATION = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_SSID = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_MAC_ADDRESS = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_SIGNAL_STRENGTH = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_OPEN_CHANNEL = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_FREQUENCY = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_IS_STILL_ACTIVE = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_ENCRYPTION = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_CONNECTED_CLIENTS = new List<string>();

        public event EventHandler DetectionDiscovered;
        public bool IsDetectionComplete { get; private set; } = false;

        private CancellationTokenSource _detectionCts = new CancellationTokenSource();


        private DetectionEngineDatabaseAccess _databaseAccess;
        public void START_DETECTION_ENGINE()
        {
            _databaseAccess = new DetectionEngineDatabaseAccess("wireless_profile.db");

            // Uncomment if database needs detection data
            // This function will populate the database with sample detection data
            //WriteSQLDataTest();


            //_databaseAccess.AlterKnownBSSIDsTable();



            IsDetectionComplete = true;
            // when detection is done invoke event so UI can update            
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);

            Task.Run(async () => await StartDetection(_detectionCts.Token));
        }
        private void WriteSQLDataTest()
        {
            var detectionEvent = new DetectionEvent
            {
                CriticalityLevel = "LEVEL_5",
                RiskLevel = 95,
                DetectionStatus = "Active",
                DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                DetectionTitle = "[TEST EVENT] Unauthorized Access Detected",
                DetectionDescription = "An unauthorized device has been detected attempting to access the network.",
                DetectionRemediation = "Investigate the device and take appropriate security measures.",
                DetectionAccessPointSsid = "ExampleSSID",
                DetectionAccessPointMacAddress = "00:1A:2B:3C:4D:5E",
                DetectionAccessPointSignalStrength = "-50 dBm",
                DetectionAccessPointOpenChannel = "6",
                DetectionAccessPointFrequency = "2.412 GHz",
                DetectionAccessPointIsStillActive = "Yes",
                DetectionAccessPointTimeFirstDetected = "14:20:15 [Date: 01 Jan 2024]",
                DetectionAccessPointEncryption = "WPA2",
                DetectionAccessPointConnectedClients = "12"
            };
            using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
            {
                db.SaveDetectionData(detectionEvent);
            }
        }

        private async Task StartDetection(CancellationToken cancellationToken)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[ DETECTION ENGINE ] STARTING...");
                while (!cancellationToken.IsCancellationRequested)
                {
                    
                    string latestTimeFrameId = _databaseAccess.GetLastTimeFrameID(); //Fetch the last proccess TIME_FRAME_ID that NormEng last processed

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[ DETECTION ENGINE ] PROCESSING {0}", latestTimeFrameId);

                    SignitureDetectionRoutineRuleCheck(latestTimeFrameId);

                    

                    //Delay just to prevent tight loop and high CPU usage
                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] DETECTION ENGINE WAS CANCELLED.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ DETECTION ENGINE ] AN ERROR OCCURRED: {ex.Message}");
            }
        }
        private void SignitureDetectionRoutineRuleCheck(String latestTimeFrameId)
        {
            var currentAccessPoints = FetchDataForTimeFrame(latestTimeFrameId);
            var knownBSSIDs = _databaseAccess.GetKnownBSSIDs();

            var newAccessPoints = currentAccessPoints.Where(ap => !knownBSSIDs.Any(known => known.BSSID == ap.BSSID)).ToList();

            if (newAccessPoints.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND");
                HandleNewAccessPointsDetection(newAccessPoints);
            }
        }

        public void StopDetectionEngine()
        {
            _detectionCts.Cancel();
        }
        private List<dot11DataIngestDataForTimeFrameID> FetchDataForTimeFrame(string timeFrameId)
        {

            string query = "SELECT * FROM WirelessProfile WHERE TIME_FRAME_ID = @TIME_FRAME_ID";
            var parameters = new { TIME_FRAME_ID = timeFrameId };

            var data = _databaseAccess.Connection.Query<dot11DataIngestDataForTimeFrameID>(query, parameters).ToList();
            return data;


        }
        private void HandleNewAccessPointsDetection(List<dot11DataIngestDataForTimeFrameID> newAccessPoints)
        {
            foreach (var ap in newAccessPoints)
            {
                // Process each new access point, for example:
                //Console.WriteLine($"[ DETECTION ENGINE ] New Access Point Detected: SSID = {ap.SSID}, BSSID = {ap.BSSID}, SignalStrength = {ap.Signal_Strength}%");

                _databaseAccess.InsertAndReportNewBssid(ap.SSID, ap.BSSID);

                var detectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_2",
                    RiskLevel = 30,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = "New Unknown Access Point Detected",
                    DetectionDescription = "An unauthorized device has been detected attempting to access the network.",
                    DetectionRemediation = "Investigate the device and take appropriate security measures.",
                    DetectionAccessPointSsid = ap.SSID,
                    DetectionAccessPointMacAddress = ap.BSSID,
                    DetectionAccessPointSignalStrength = ap.Signal_Strength.ToString(),
                    DetectionAccessPointOpenChannel = ap.Channel.ToString(),
                    DetectionAccessPointFrequency = ap.Frequency,
                    DetectionAccessPointIsStillActive = "Yes",
                    DetectionAccessPointTimeFirstDetected = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionAccessPointEncryption = ap.Authentication,
                    DetectionAccessPointConnectedClients = "N/A"
                };
                using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
                {
                    db.SaveDetectionData(detectionEvent);
                }
                DetectionDiscovered?.Invoke(this, EventArgs.Empty);
            }
        }

        private string FetchNextTimeFrameID(string currentId)
        {
            var idGenerator = new TimeFrameIdGenerator(currentId);
            var nextId = idGenerator.GenerateNextId();

            // Check if data exists for the next ID
            var dataExists = CheckDataExistsForTimeFrameID(nextId);

            if (dataExists)
            {
                return nextId;
            }
            else
            {
                return null;
            }
        }
        private bool CheckDataExistsForTimeFrameID(string timeFrameId)
        {
            string query = "SELECT COUNT(*) FROM WirelessProfile WHERE TIME_FRAME_ID = @TIME_FRAME_ID";
            var parameters = new { TIME_FRAME_ID = timeFrameId };

            int count = _databaseAccess.Connection.ExecuteScalar<int>(query, parameters);

            return count > 0;
        }
    }

    public class dot11DataIngestDataForTimeFrameID
    {
        public int ID { get; set; }
        public string Time { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public int Signal_Strength { get; set; }
        public string Wifi_Standard { get; set; }
        public string Band { get; set; }
        public int Channel { get; set; }
        public string Frequency { get; set; }
        public string Authentication { get; set; }
        public string TimeFrameID { get; set; } //use this for the ID of the event
    }
}
