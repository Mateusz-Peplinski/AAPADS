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

            // PROCESS BLOCK 1 - Check if a new access point has been detected and proccess then
            // RULE 1 - Rogue Access Point - A sudden appearance of duplicate SSID on a different MAC addresses 
            // RULE 2 - Evil Twin Attack - A sudden appearance of a duplicate SSID and BSSID
            // RULE 3 - SSID Spoofing - A sudden appearance of a similar looking SSID
            // RULE 4 - WiFi Jamming - A sudden decrease in signal from all access points in proximity [NOT DONE YET]
            // RULE 5 - SSID Beacon Flooding - A sudden appearance of many SSIDs
            
            var newAccessPoints = currentAccessPoints.Where(ap => !knownBSSIDs.Any(known => known.BSSID == ap.BSSID)).ToList();

            //int count = newAccessPoints.Count;

            if (newAccessPoints.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND");
                HandleProcessBlockOneRules(newAccessPoints);
            }

            // PROCESS BLOCK 2 - Security Misconfigurations
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
        private void HandleProcessBlockOneRules(List<dot11DataIngestDataForTimeFrameID> newAccessPoints)
        {
            int NewlyDetectedAccessPointCount = newAccessPoints.Count;

            if (NewlyDetectedAccessPointCount >= 10 && NewlyDetectedAccessPointCount <= 15)
            {
                var detectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_2",
                    RiskLevel = 30,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = $"Possible SSID Beacon Flooding",
                    DetectionDescription = $"AAPADS systems have registered a notable increase in the number of SSIDs being broadcasted in your proximity, raising concerns about a potential SSID beacon flooding scenario. Such an increase is often indicative of a device or a group of devices emitting a large number of SSID beacons in a short period, which can be a tactic used in reconnaissance phases of cyber attacks or to create confusion and disrupt wireless network operations. " +
                                            $"\nThis activity does not match the usual wireless traffic patterns observed in this environment, suggesting an anomaly that warrants closer inspection.",
                    DetectionRemediation = @"While the current information does not confirm malicious intent behind the surge in SSID broadcasts, caution and further investigation are advised to ensure network integrity and security. To address this situation, consider the following steps:

                                            1. Conduct a detailed analysis of the SSID signals' origin, attempting to pinpoint the physical location or specific devices responsible for the surge.
                                            2. Compare the newly detected SSIDs against a list of known and trusted networks to identify any that may be impersonating legitimate access points.
                                            3. Increase the monitoring intensity of network traffic to quickly identify any unauthorized access or further anomalies in wireless activity.
                                            4. If any of the new SSIDs attempt to mimic the naming conventions of your network or other nearby trusted networks, treat them as potential threats and investigate accordingly.
                                            5. Advise network users to remain vigilant when connecting to wireless networks, especially those that have recently appeared or do not have a verified source.

                                            Given the potential security implications of this event, documenting all findings and any steps taken in response is crucial for future reference and potential escalation to cybersecurity specialists if the situation does not resolve or escalates.",
                    DetectionAccessPointSsid = "N/A",
                    DetectionAccessPointMacAddress = "N/A",
                    DetectionAccessPointSignalStrength = "N/A",
                    DetectionAccessPointOpenChannel = "N/A",
                    DetectionAccessPointFrequency = "N/A",
                    DetectionAccessPointIsStillActive = "UNKNOWN", // Need to make a mechanism for this
                    DetectionAccessPointTimeFirstDetected = "N/A",
                    DetectionAccessPointEncryption = "N/A",
                    DetectionAccessPointConnectedClients = "N/A"
                };
                using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
                {
                    db.SaveDetectionData(detectionEvent);
                }
                DetectionDiscovered?.Invoke(this, EventArgs.Empty);
            }
            if (NewlyDetectedAccessPointCount > 16 )
            {
                var detectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_3",
                    RiskLevel = 60,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = $"SSID Beacon Flooding",
                    DetectionDescription = $"AAPADS systems have registered a notable increase in the number of SSIDs being broadcasted in your proximity, raising concerns about a potential SSID beacon flooding scenario. Such an increase is often indicative of a device or a group of devices emitting a large number of SSID beacons in a short period, which can be a tactic used in reconnaissance phases of cyber attacks or to create confusion and disrupt wireless network operations. " +
                                            $"\nThis activity does not match the usual wireless traffic patterns observed in this environment, suggesting an anomaly that warrants closer inspection.",
                    DetectionRemediation = @"While the current information does not confirm malicious intent behind the surge in SSID broadcasts, caution and further investigation are advised to ensure network integrity and security. To address this situation, consider the following steps:

                                            1. Conduct a detailed analysis of the SSID signals' origin, attempting to pinpoint the physical location or specific devices responsible for the surge.
                                            2. Compare the newly detected SSIDs against a list of known and trusted networks to identify any that may be impersonating legitimate access points.
                                            3. Increase the monitoring intensity of network traffic to quickly identify any unauthorized access or further anomalies in wireless activity.
                                            4. If any of the new SSIDs attempt to mimic the naming conventions of your network or other nearby trusted networks, treat them as potential threats and investigate accordingly.
                                            5. Advise network users to remain vigilant when connecting to wireless networks, especially those that have recently appeared or do not have a verified source.

                                            Given the potential security implications of this event, documenting all findings and any steps taken in response is crucial for future reference and potential escalation to cybersecurity specialists if the situation does not resolve or escalates.",
                    DetectionAccessPointSsid = "N/A",
                    DetectionAccessPointMacAddress = "N/A",
                    DetectionAccessPointSignalStrength = "N/A",
                    DetectionAccessPointOpenChannel = "N/A",
                    DetectionAccessPointFrequency = "N/A",
                    DetectionAccessPointIsStillActive = "UNKNOWN", // Need to make a mechanism for this
                    DetectionAccessPointTimeFirstDetected = "N/A",
                    DetectionAccessPointEncryption = "N/A",
                    DetectionAccessPointConnectedClients = "N/A"
                };
                using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
                {
                    db.SaveDetectionData(detectionEvent);
                }
                DetectionDiscovered?.Invoke(this, EventArgs.Empty);
            }

            foreach (var ap in newAccessPoints) //For each in case many access points show up at once.
            {
                // Process each new access point
                // Console.WriteLine($"[ DETECTION ENGINE ] New Access Point Detected: SSID = {ap.SSID}, BSSID = {ap.BSSID}, SignalStrength = {ap.Signal_Strength}%");

                _databaseAccess.InsertAndReportNewBssid(ap.SSID, ap.BSSID); // 

                var detectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_1",
                    RiskLevel = 10,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = $"New Unknown Access Point Detected [SSID: {ap.SSID}]",
                    DetectionDescription = $"An new device with SSID {ap.SSID} and BSSID: {ap.BSSID} has been detected in your proximity",
                    DetectionRemediation = $"The access point {ap.SSID} does not attempt to mimic your wireless network. It is likely a hotspot wireless network.\nHowever if this device has never been seen before it should be investigated.",
                    DetectionAccessPointSsid = ap.SSID,
                    DetectionAccessPointMacAddress = ap.BSSID,
                    DetectionAccessPointSignalStrength = ap.Signal_Strength.ToString(),
                    DetectionAccessPointOpenChannel = ap.Channel.ToString(),
                    DetectionAccessPointFrequency = ap.Frequency,
                    DetectionAccessPointIsStillActive = "UNKNOWN", // Need to make a mechanism for this
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
