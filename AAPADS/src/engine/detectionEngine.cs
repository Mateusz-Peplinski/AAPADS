using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AAPADS.src.engine
{
    public class DetectionEngine
    {
        //public List<string> CRITICALITY_LEVEL = new List<string>();
        //public List<int> RISK_LEVEL = new List<int>();
        //public List<string> DETECTION_STATUS = new List<string>();
        //public List<string> DETECTION_TIME = new List<string>();
        //public List<string> DETECTION_TITLE = new List<string>();
        //public List<string> DETECTION_DESCRIPTION = new List<string>();
        //public List<string> DETECTION_REMEDIATION = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_SSID = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_MAC_ADDRESS = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_SIGNAL_STRENGTH = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_OPEN_CHANNEL = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_FREQUENCY = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_IS_STILL_ACTIVE = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_ENCRYPTION = new List<string>();
        //public List<string> DETECTION_ACCESS_POINT_CONNECTED_CLIENTS = new List<string>();


        private List<string> _SIMILAR_SSIDS_GENERATED_LIST;

        private string _DEFAULT_WLAN_SSID;

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

            // Fetch the SSID that the device is connected too
            string connectedSSID = LoadConnectedWLANNameFromDatabase();

            // Generate a list of similar SSIDs for PB1 RULE 3 - SSID Spoofing 
            _SIMILAR_SSIDS_GENERATED_LIST = GenerateSimilarSSIDs(connectedSSID);

            _DEFAULT_WLAN_SSID = LoadConnectedWLANNameFromDatabase();

            IsDetectionComplete = true;
            // when detection is done invoke event so UI can update            
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);

            Task.Run(async () => await StartDetection(_detectionCts.Token));
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



                    //Delay to prevnt tight loop and high CPU usage
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
            // Fetech all the Access point data for the most recently process TIME_FRAME_ID
            var currentAccessPoints = FetchDataForTimeFrame(latestTimeFrameId);

            // Fetech the list of Known Access Points that where detected during the traning phase
            var knownBSSIDs = _databaseAccess.GetKnownBSSIDs();

            //  Filters currentAccessPoints to include only those access points whose BSSID is not found in the list of knownBSSIDs
            var newAccessPoints = currentAccessPoints.Where(ap => !knownBSSIDs.Any(known => known.BSSID == ap.BSSID)).ToList();

            // Process the new access points if any were found
            if (newAccessPoints.Any())
            { 
                HandleProcessBlockOneRules(newAccessPoints);
            }

            // PROCESS BLOCK 2 - Security Misconfigurations
        }

        public void StopDetectionEngine()
        {
            _detectionCts.Cancel();
        }

        private void HandleProcessBlockOneRules(List<dot11DataIngestDataForTimeFrameID> newAccessPoints)
        {
            // PROCESS BLOCK 1 - Check if a new access point has been detected and proccess then
            // RULE 1 - SSID Beacon Flooding - A sudden appearance of many SSIDs
            // RULE 2 - Unknown Access Point - A new unknown acess point is discovered in close proximity
            // RULE 3 - SSID Spoofing - A sudden appearance of a similar looking SSID
            // RULE 4 - Evil Twin Attack - A sudden appearance of a duplicate SSID and BSSID
            // RULE 5 - WiFi Jamming - A sudden decrease in signal from all access points in proximity [NOT DONE YET]
            // RULE 6 - Rogue Access Point - A sudden appearance of duplicate SSID on a different MAC addresses

            // RULE 1 - SSID Beacon Flooding - A sudden appearance of many SSIDs
            int NewlyDetectedAccessPointCount = newAccessPoints.Count;

            if (NewlyDetectedAccessPointCount >= 10 && NewlyDetectedAccessPointCount <= 15)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND - POSSIBLE SSID BEACON FLOODING");

                var PossibleSSIDBeaconFloodingdetectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_2",
                    RiskLevel = 30,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = $"Possible SSID Beacon Flooding",
                    DetectionDescription = $"AAPADS systems have registered a notable increase in the number of {NewlyDetectedAccessPointCount} SSIDs being broadcasted in your proximity, raising concerns about a potential SSID beacon flooding scenario. Such an increase is often indicative of a device or a group of devices emitting a large number of SSID beacons in a short period, which can be a tactic used in reconnaissance phases of cyber attacks or to create confusion and disrupt wireless network operations. " +
                                        $"\nThis activity does not match the usual wireless traffic patterns observed in this environment, suggesting an anomaly that warrants closer inspection.",
                    DetectionRemediation = @"",
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
                SaveDetectionToDatabase(PossibleSSIDBeaconFloodingdetectionEvent);
            }
            if (NewlyDetectedAccessPointCount > 16)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND - SSID BEACON FLOODING"); 

                var SSIDBeaconFloodingdetectionEvent = new DetectionEvent
                {
                    CriticalityLevel = "LEVEL_3",
                    RiskLevel = 60,
                    DetectionStatus = "Active",
                    DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                    DetectionTitle = $"SSID Beacon Flooding",
                    DetectionDescription = $"AAPADS systems have registered a notable increase in the number of {NewlyDetectedAccessPointCount} SSIDs being broadcasted in your proximity, raising concerns about a potential SSID beacon flooding scenario. Such an increase is often indicative of a device or a group of devices emitting a large number of SSID beacons in a short period, which can be a tactic used in reconnaissance phases of cyber attacks or to create confusion and disrupt wireless network operations. " +
                                        $"\nThis activity does not match the usual wireless traffic patterns observed in this environment, suggesting an anomaly that warrants closer inspection.",
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
                SaveDetectionToDatabase(SSIDBeaconFloodingdetectionEvent);

            }
            //For each in handle many access points show up at once.
            foreach (var ap in newAccessPoints) 
            {
                // Add the new access point to the KnownBSSIDs SQL TABLE to prevent many repative events
                _databaseAccess.InsertAndReportNewBssid(ap.SSID, ap.BSSID);

                // RULE 2 - Unknown Access Point - A new unknown acess point is discovered in close proximity
                if (ap.Signal_Strength > 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND - NEW UNKNOWN ACCESS POINT DETECTED");

                    var NewAccessPointdetectionEvent = new DetectionEvent
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
                    SaveDetectionToDatabase(NewAccessPointdetectionEvent);
                }

                // RULE 3 - SSID Spoofing - A sudden appearance of a similar looking SSID
                // Compare each new access point's SSID with the list of similar SSIDs
                if (_SIMILAR_SSIDS_GENERATED_LIST.Any(similarSSID => similarSSID == ap.SSID))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ DETECTION ENGINE ] DETECTION FOUND - SSID SPOOFING");

                    // If a match is found, it indicates a potential SSID spoofing attempt
                    var SSIDSpoofingdetectionEvent = new DetectionEvent
                    {
                        CriticalityLevel = "LEVEL_4",
                        RiskLevel = 75,
                        DetectionStatus = "Active",
                        DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                        DetectionTitle = $"SSID Spoofing Detected [SSID: {ap.SSID}]",
                        DetectionDescription = $"A new access point with an SSID similar to your connected network ({ap.SSID}) has been detected, indicating a potential SSID spoofing attempt.",
                        DetectionRemediation = "AAPADS noticed a new Wi-Fi network nearby with an SSID similar to your connected network. Stick to Known Networks. Check your router settings for anything odd. Consider hiding your Wi-Fi name ('Hide SSID') for privacy. " +
                                               "\nInform your household or office not to connect to unfamiliar networks. When unsure, avoid connecting and seek advice.",
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
                    SaveDetectionToDatabase(SSIDSpoofingdetectionEvent);
                }

            }
        }
        private void SaveDetectionToDatabase(DetectionEvent detectionEvent)
        {
            using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
            {
                db.SaveDetectionData(detectionEvent);
            }
            TriggerDetectionEvent();
        }
        private void TriggerDetectionEvent()
        {
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);
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
        private string LoadConnectedWLANNameFromDatabase()
        {
            String WLANName = "";
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                var settingValue = db.GetSetting("DefaultWLANName");

                if (settingValue != null)
                {
                    WLANName = settingValue;
                }
            }
            return WLANName;
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
        private List<dot11DataIngestDataForTimeFrameID> FetchDataForTimeFrame(string timeFrameId)
        {

            string query = "SELECT * FROM WirelessProfile WHERE TIME_FRAME_ID = @TIME_FRAME_ID";
            var parameters = new { TIME_FRAME_ID = timeFrameId };

            var data = _databaseAccess.Connection.Query<dot11DataIngestDataForTimeFrameID>(query, parameters).ToList();
            return data;


        }
        List<string> GenerateSimilarSSIDs(string ssid)
        {
            var variations = new List<string>();

            // Basic numeric appendages
            for (int i = 1; i <= 10; i++)
            {
                variations.Add(ssid + i.ToString());
            }

            // Common substitutions and alterations
            variations.Add(ssid.Replace("-", "_"));
            variations.Add(ssid.Replace("_", "-"));
            variations.Add(ssid + "_guest");
            variations.Add(ssid + "-guest");
            variations.Add(ssid + " guest");
            variations.Add(ssid + "_Guest");
            variations.Add(ssid + "-Guest");
            variations.Add(ssid + " Guest");
            variations.Add(ssid + "_GUEST");
            variations.Add(ssid + "-GUEST");
            variations.Add(ssid + " GUEST");
            variations.Add(ssid + "5G");
            variations.Add(ssid + "5g");
            variations.Add(ssid + " 5G");
            variations.Add(ssid + " 5g");
            variations.Add(ssid + "2_4g");
            variations.Add(ssid + "24g");
            variations.Add(ssid + "2.4g");
            variations.Add(ssid + "2.4G");
            variations.Add(ssid + "2_4G");
            variations.Add(ssid + "24G");
            variations.Add(ssid + " 2_4g");
            variations.Add(ssid + " 24g");
            variations.Add(ssid + " 2.4g");
            variations.Add(ssid + " 2.4G");
            variations.Add(ssid + " 2_4G");
            variations.Add(ssid + " 24G");
            variations.Add(ssid.ToUpper());
            variations.Add(ssid.ToLower());

            // Removing and adding common separators
            variations.Add(ssid.Replace("-", ""));
            variations.Add(ssid.Replace("_", ""));
            variations.Add(ssid.Replace(" ", ""));
            variations.Add(" " + ssid);
            variations.Add("-" + ssid);
            variations.Add("_" + ssid);

            // Try Replace spaces with other chars
            if (ssid.Contains(" "))
            {
                variations.Add(ssid.Replace(" ", "-")); // Replace spaces with hyphens
                variations.Add(ssid.Replace(" ", "_")); // Replace spaces with underscores
            }

            // Case variations for each character (for shorter SSIDs)  
            // 16 is used as MAX to produce 65536 SSID variations
            if (ssid.Length <= 16)
            {
                variations.AddRange(CreateCasePermutations(ssid));
            }

            // Adding common prefixes/suffixes
            var commonPrefixesSuffixes = new[] { " Free", " Secure", " Public", " Private", "home", "FREE", " SECURE", " PUBLIC", " PRIVATE", " HOME","_Free", "_Secure", "_Public", "_Private", "_home", "_FREE", "_SECURE", "_PUBLIC", "_PRIVATE", "_HOME" };
            foreach (var item in commonPrefixesSuffixes)
            {
                variations.Add(item + ssid);
                variations.Add(ssid + item);
            }

            return variations.Distinct().ToList(); // Return unique variations
        }
        List<string> CreateCasePermutations(string ssid)
        {
            if (string.IsNullOrEmpty(ssid)) return new List<string>();

            var charArrays = new List<List<char>>();
            foreach (var c in ssid)
            {
                var chars = new List<char> { char.ToUpper(c), char.ToLower(c) };
                charArrays.Add(chars);
            }

            IEnumerable<string> permutations = new[] { "" };
            foreach (var arr in charArrays)
            {
                permutations = from perm in permutations
                               from c in arr
                               select perm + c;
            }

            return permutations.ToList();
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
