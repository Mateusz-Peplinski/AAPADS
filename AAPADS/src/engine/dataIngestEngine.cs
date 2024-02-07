using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AAPADS
{
    public class DataIngestEngine
    {
        // Trigger Event in MainWindow.xaml.cs
        public event EventHandler EVENT_WLAN_DATA_COLLECTED;

        // Lists which will store the access point information
        // Public because they will be accessed by the ViewModel to populate the GUI
        public List<string> SSID_LIST = new List<string>();
        public List<string> ENCRYPTION_TYPE_LIST = new List<string>();
        public List<string> BSSID_LIST = new List<string>();
        public List<int> SIGNAL_STRENGTH_LIST = new List<int>();
        public List<string> WIFI_STANDARD_LIST = new List<string>();
        public List<string> BAND_LIST = new List<string>();
        public List<int> CHANNEL_LIST = new List<int>();
        public List<string> FREQUENCY_LIST = new List<string>();
        public List<string> AUTH_LIST = new List<string>();

        // Dictionary of IEE802.11 channels and their frequencies
        public Dictionary<int, string> channelToFrequencies = new Dictionary<int, string>()
{
            // 2.4 GHz Band
            { 1, "2.412 GHz" },
            { 2, "2.417 GHz" },
            { 3, "2.422 GHz" },
            { 4, "2.427 GHz" },
            { 5, "2.432 GHz" },
            { 6, "2.437 GHz" },
            { 7, "2.442 GHz" },
            { 8, "2.447 GHz" },
            { 9, "2.452 GHz" },
            { 10, "2.457 GHz" },
            { 11, "2.462 GHz" },
            { 12, "2.467 GHz" },
            { 13, "2.472 GHz" },
            { 14, "2.484 GHz" },

            // 5 GHz Band
            { 36, "5.180 GHz" },
            { 40, "5.200 GHz" },
            { 44, "5.220 GHz" },
            { 48, "5.240 GHz" },                                                                                                 
            { 52, "5.260 GHz" },
            { 56, "5.280 GHz" },
            { 60, "5.300 GHz" },
            { 64, "5.320 GHz" },
            { 100, "5.500 GHz" },
            { 104, "5.520 GHz" },
            { 108, "5.540 GHz" },
            { 112, "5.560 GHz" },
            { 116, "5.580 GHz" },
            { 120, "5.600 GHz" },
            { 124, "5.620 GHz" },
            { 128, "5.640 GHz" },
            { 132, "5.660 GHz" },
            { 136, "5.680 GHz" },
            { 140, "5.700 GHz" },
            { 144, "5.720 GHz" },
            { 149, "5.745 GHz" },
            { 153, "5.765 GHz" },
            { 157, "5.785 GHz" },
            { 161, "5.805 GHz" },
            { 165, "5.825 GHz" }
        };

        // The status flag to show loading animation bar in OVERVIEW TAB in MainWindow.xaml
        public bool IS_LOADING_STATUS_FLAG = false;

        // The status flag for if traning phase has begun
        private bool TRANING_FLAG_STATUS = false;

        // The status flag for if detection phase has begun
        private bool DETECTION_FLAG_STATUS = false;

        // The status flag from the main loop in the scanning thread
        private bool IS_RUNNING_STATUS_FLAG = true;

        // The Semaphore used to lock the lists
        private SemaphoreSlim DATA_INGEST_ENGINE_THREAD_SEMAPHORE = new SemaphoreSlim(1);

        // Keep track if the program has been loaded before
        private int PROGRAM_LOAD_COUNTER = 0;

        // Used by ParseRecivedData() to keep track of BSSIDS in SSIDS
        private string CURRENT_SSID = null;
        private string CURRENT_ENCRYPTION = null;
        private string CURRENT_AUTH = null;
        private string CURRENT_BSSID = null;
        private int CURRENT_SIGNAL = 0;
        private string CURRENT_RADIO = null;
        private string CURRENT_BAND = null;
        private int CURRENT_CHANNEL = 0;
        private string CURRENT_FREQUENCY = null;

        // Database access object
        private wirelessProfileDatabaseAccess DATA_INGEST_ENGINE_DATABASE_ACCESS;

        // Normalization Engine object
        private NormalizationEngine INIT_NORMALIZATION_ENGINE;

        // PerformWifiScan() from WLANLibrary.dll
        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PerformWifiScan();

        // The Data Ingest Engine Start Function
        public void START_DATA_INGEST_ENGINE()
        {
            // Initialize the lists which will store the access point information
            InitializeDataIngestEngineList();

            // Initialize database connection for this engine
            DATA_INGEST_ENGINE_DATABASE_ACCESS = new wirelessProfileDatabaseAccess("wireless_profile.db");

            // Start the normalization engine
            INIT_NORMALIZATION_ENGINE = new NormalizationEngine();

            // main thread for constanly scanning and parsing output from netsh
            Task.Run(PreformWLANScanThread); 
        }
        private void InitializeDataIngestEngineList()
        {
            SSID_LIST = new List<string>();
            ENCRYPTION_TYPE_LIST = new List<string>();
            BSSID_LIST = new List<string>();
            SIGNAL_STRENGTH_LIST = new List<int>();
            WIFI_STANDARD_LIST = new List<string>();
            BAND_LIST = new List<string>();
            CHANNEL_LIST = new List<int>();
            FREQUENCY_LIST = new List<string>();
            AUTH_LIST = new List<string>();
        }
        private async Task PreformWLANScanThread()
        {
            while (IS_RUNNING_STATUS_FLAG)
            {
                await DATA_INGEST_ENGINE_THREAD_SEMAPHORE.WaitAsync();

                // Clear lists at the beginning of each loop iteration.
                ClearDataIngestEngineList();   

                // Only ran once in the first iteration of the loop. 
                if (PROGRAM_LOAD_COUNTER == 0)
                {
                    // Setting this true will show the loading UI
                    IS_LOADING_STATUS_FLAG = true; 

                    // increment the counter so it is != 0 
                    PROGRAM_LOAD_COUNTER++; 
                }

                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[ DATA INGEST ENGINE ] COLLECTING WLAN DATA...");

                    // Call PerformWifiScan() from WLANLibrary to refresh the access point information
                    // Without PerformWifiScan() Windows will ignore other WLAN beacons frames and only show the connected SSID
                    if (PerformWifiScan())
                    {
                        // Wait for 5 seconds between scans
                        await Task.Delay(TimeSpan.FromSeconds(5)); 

                        // Run the netsh wlan show networks mode=Bssid command
                        CollectBSSIDInformation();

                        // Show how much BSSID information was processed
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[ DATA INGEST ENGINE ] COLLECTED {SSID_LIST.Count} ACCESS POINTS DATA");

                        // Trigger Event in MainWindow.xaml.cs --> This will update the ViewModel and in return the GUI
                        EVENT_WLAN_DATA_COLLECTED?.Invoke(this, EventArgs.Empty);


                        // Fetch the staus flags from 'settings' from the database 
                        FetechTrainingFlagStatus();
                        FetechDetectionFlagStatus();

                        // AAPADS only needs to write to database if the detection capabilities have started
                        // The Status flags are feteched from 'settings' in the database
                        if (TRANING_FLAG_STATUS == true || DETECTION_FLAG_STATUS == true)
                        {
                            // Write the collected data to the database
                            InsertParsedDataToDatabase();
                        }
                    }
                    else
                    {
                        // ERROR: Calling PerformWifiScan()
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ DATA INGEST ENGINE ] FAILED TO INITIATE WI-FI SCAN");
                    }
                }
                catch (Exception ex)
                {
                    // MAJOR ERROR: Failed to peform scan
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ DATA INGEST ENGINE ] {ex.Message}.");
                    IS_RUNNING_STATUS_FLAG = false;
                }
                finally
                {
                    // The data has been collected with no error and the loading animation can be stoped
                    IS_LOADING_STATUS_FLAG = false;

                    // Release the lock on the data 
                    DATA_INGEST_ENGINE_THREAD_SEMAPHORE.Release();
                }

                // Wait for 5 seconds between scans
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private void FetechTrainingFlagStatus()
        {
            // Create a disposable connection to the settings database
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Fetch the status
                var settingValue = db.GetSetting("TrainingFlag");
                
                if (settingValue != null)
                {
                    // Parse the returned value to a boolean.
                    TRANING_FLAG_STATUS = settingValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        private void FetechDetectionFlagStatus()
        {
            // Create a disposable connection to the settings database
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Fetch the status
                var flagStatus = db.GetSetting("DetectionFlag");

                // DetectionFlag can be null if Detection Engine has not yet been initialized
                if (flagStatus == null)
                {
                    // Return flase if null because Detection Engine has not yet been initialized
                    DETECTION_FLAG_STATUS = false;
                }

                if (flagStatus != null)
                {
                    // Parse the returned value to a boolean.
                    DETECTION_FLAG_STATUS = flagStatus.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        private void ClearDataIngestEngineList()
        {
            // Clear the lists before the next loop iteration
            SSID_LIST.Clear();
            ENCRYPTION_TYPE_LIST.Clear();
            BSSID_LIST.Clear();
            SIGNAL_STRENGTH_LIST.Clear();
            WIFI_STANDARD_LIST.Clear();
            BAND_LIST.Clear();
            CHANNEL_LIST.Clear();
            FREQUENCY_LIST.Clear();
            AUTH_LIST.Clear();
        }
        private void CollectBSSIDInformation()
        {
            // Create 
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show networks mode=Bssid",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.OutputDataReceived += ProcessRecivedData;

                process.Start();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }
        }
        private void ProcessRecivedData(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                ParseRecivedData(e.Data);

            }
        }
        private void ParseRecivedData(string output)
        {
            output = output.Trim();
            int index = output.IndexOf(":");
            if (output.StartsWith("SSID") && (index == output.Length - 1 || string.IsNullOrWhiteSpace(output.Substring(index + 1))))
            {
                AddCurrentDataToGlobalLists();
                ClearCurrentData();
                CURRENT_SSID = "[HIDDEN NETWORK]";
                return;
            }
            if (index < 0) return;

            if (index >= output.Length - 1)
                return;

            string value = output.Substring(index + 1).Trim();

            string title = output.Substring(0, index).Split(' ')[0];

            switch (title)
            {
                case "SSID":
                    AddCurrentDataToGlobalLists();
                    ClearCurrentData();

                    CURRENT_SSID = value;
                    break;

                case "Encryption":
                    CURRENT_ENCRYPTION = value;
                    break;

                case "Authentication":
                    CURRENT_AUTH = value;
                    break;

                case "BSSID":
                    AddCurrentDataToGlobalLists();
                    ClearBssidRelatedData();

                    CURRENT_BSSID = value;
                    break;

                case "Signal":
                    int endIndex = output.IndexOf("%");
                    if (endIndex > index && endIndex < output.Length)
                    {
                        string signalValue = output.Substring(index + 1, endIndex - index - 1).Trim();
                        if (int.TryParse(signalValue, out int signal))
                        {
                            CURRENT_SIGNAL = signal;
                        }
                    }
                    break;

                case "Radio":
                    CURRENT_RADIO = value;
                    break;

                case "Band":
                    CURRENT_BAND = value;
                    break;

                case "Channel":
                    if (int.TryParse(value, out int channel) && channelToFrequencies.ContainsKey(channel))
                    {
                        CURRENT_CHANNEL = channel;
                        CURRENT_FREQUENCY = channelToFrequencies[channel];
                    }
                    break;
            }
        }
        private void AddCurrentDataToGlobalLists()
        {
            if (!string.IsNullOrEmpty(CURRENT_SSID) && !string.IsNullOrEmpty(CURRENT_BSSID))
            {
                SSID_LIST.Add(CURRENT_SSID);
                ENCRYPTION_TYPE_LIST.Add(CURRENT_ENCRYPTION);
                AUTH_LIST.Add(CURRENT_AUTH);
                BSSID_LIST.Add(CURRENT_BSSID);
                SIGNAL_STRENGTH_LIST.Add(CURRENT_SIGNAL);
                WIFI_STANDARD_LIST.Add(CURRENT_RADIO);
                BAND_LIST.Add(CURRENT_BAND);
                CHANNEL_LIST.Add(CURRENT_CHANNEL);
                FREQUENCY_LIST.Add(CURRENT_FREQUENCY);
            }
        }
        private void ClearCurrentData()
        {
            CURRENT_SSID = null;
            CURRENT_ENCRYPTION = null;
            CURRENT_AUTH = null;
            ClearBssidRelatedData();
        }
        private void ClearBssidRelatedData()
        {
            CURRENT_BSSID = null;
            CURRENT_SIGNAL = 0;
            CURRENT_RADIO = null;
            CURRENT_BAND = null;
            CURRENT_CHANNEL = 0;
            CURRENT_FREQUENCY = null;
        }
        private void InsertParsedDataToDatabase()
        {
            string LAST_TIME_FRAME_ID = DATA_INGEST_ENGINE_DATABASE_ACCESS.GetLastTimeFrameId();
            var idGenerator = new TimeFrameIdGenerator(LAST_TIME_FRAME_ID);


            string CURRENT_TIME_FRAME_ID = idGenerator.GenerateNextId();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[ DATA INGEST ENGINE ] SQL WRITE at {CURRENT_TIME_FRAME_ID}");

            for (int i = 0; i < SSID_LIST.Count; i++)
            {
                DATA_INGEST_ENGINE_DATABASE_ACCESS.DataIngestEngineInsertAccessPointData(
                    SSID_LIST[i],
                    BSSID_LIST[i],
                    SIGNAL_STRENGTH_LIST[i],
                    WIFI_STANDARD_LIST[i],
                    BAND_LIST[i],
                    CHANNEL_LIST[i],
                    FREQUENCY_LIST[i],
                    AUTH_LIST[i],
                    CURRENT_TIME_FRAME_ID
                );
            }
        }
        

    }


}