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

        public event EventHandler SSIDDataCollected;
        public List<string> SSID_LIST = new List<string>();
        public List<string> ENCRYPTION_TYPE_LIST = new List<string>();
        public List<string> BSSID_LIST = new List<string>();
        public List<int> SIGNAL_STRENGTH_LIST = new List<int>();
        public List<string> WIFI_STANDARD_LIST = new List<string>();
        public List<string> BAND_LIST = new List<string>();
        public List<int> CHANNEL_LIST = new List<int>();
        public List<string> FREQUENCY_LIST = new List<string>();
        public List<string> AUTH_LIST = new List<string>();
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

        public bool isLoading = false;
        private bool TraningFlagStatus = false;
        private bool DetectionFlagStatus = false;

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private bool isRunning = true;
        private int programLoadCount = 0;

        private string currentSSID = null;
        private string currentEncryption = null;
        private string currentAuth = null;
        private string currentBSSID = null;
        private int currentSignal = 0;
        private string currentRadio = null;
        private string currentBand = null;
        private int currentChannel = 0;
        private string currentFrequency = null;

        private wirelessProfileDatabaseAccess _dbAccess;
        private NormalizationEngine _normalizationEngine;

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PerformWifiScan();

        public void START_DATA_INGEST_ENGINE()
        {

            InitializeDataIngestEngineList();

            // Initlize database access for this engine
            _dbAccess = new wirelessProfileDatabaseAccess("wireless_profile.db");

            // Start the normalization engine
            _normalizationEngine = new NormalizationEngine();

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
            while (isRunning)
            {
                await semaphore.WaitAsync();

                // Clear lists at the beginning of each iteration.
                ClearDataIngestEngineList();   

                if (programLoadCount == 0)
                {
                    isLoading = true;
                    programLoadCount++;
                }

                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[ DATA INGEST ENGINE ] COLLECTING WLAN DATA...");

                    if (PerformWifiScan())//Call WLANLibrary to refresh the access point information that windows has cached
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds between scans
                        SystemRunNETSHCommand();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[ DATA INGEST ENGINE ] COLLECTED {SSID_LIST.Count} ACCESS POINTS DATA");

                        SSIDDataCollected?.Invoke(this, EventArgs.Empty);


                        FetechTrainingFlagStatus();
                        FetechDetectionFlagStatus();

                        if (TraningFlagStatus == true || DetectionFlagStatus == true)
                        {
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Console.WriteLine("[ DATA INGEST ENGINE ] Traning Flag Status {0}", TraningFlagStatus);
                            //ADD START_DATA_INGEST_ENGINE_WRITE --> Only need to write if Detection has started --> need to add bool value that
                            //will be set when user selected detection start 
                            InsertParsedDataToDatabase();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ DATA INGEST ENGINE ] FAILED TO INITIATE WI-FI SCAN.");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ DATA INGEST ENGINE ] {ex.Message}.");
                }
                finally
                {
                    isLoading = false;
                    semaphore.Release();
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private void FetechTrainingFlagStatus()
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                var settingValue = db.GetSetting("TrainingFlag");
                
                if (settingValue != null)
                {
                    // Parse the returned value to a boolean.
                    TraningFlagStatus = settingValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        private void FetechDetectionFlagStatus()
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                var flagStatus = db.GetSetting("DetectionFlag");
                if (flagStatus == null)
                {
                    DetectionFlagStatus = false;
                }

                if (flagStatus != null)
                {
                    // Parse the returned value to a boolean.
                    DetectionFlagStatus = flagStatus.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        private void ClearDataIngestEngineList()
        {
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

        private void SystemRunNETSHCommand()
        {
            //liveLogDataModelConsole.AppendToLog("init netsh");

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
                process.OutputDataReceived += ProcessNETSHReciveData;

                process.Start();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }
        }

        private void ProcessNETSHReciveData(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                ParseNETSHReciveData(e.Data);

            }
        }


        private void ParseNETSHReciveData(string output)
        {
            output = output.Trim();
            int index = output.IndexOf(":");
            if (output.StartsWith("SSID") && (index == output.Length - 1 || string.IsNullOrWhiteSpace(output.Substring(index + 1))))
            {
                AddCurrentDataToGlobalLists();
                ClearCurrentData();
                currentSSID = "[HIDDEN NETWORK]";
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

                    currentSSID = value;
                    break;

                case "Encryption":
                    currentEncryption = value;
                    break;

                case "Authentication":
                    currentAuth = value;
                    break;

                case "BSSID":
                    AddCurrentDataToGlobalLists();
                    ClearBssidRelatedData();

                    currentBSSID = value;
                    break;

                case "Signal":
                    int endIndex = output.IndexOf("%");
                    if (endIndex > index && endIndex < output.Length)
                    {
                        string signalValue = output.Substring(index + 1, endIndex - index - 1).Trim();
                        if (int.TryParse(signalValue, out int signal))
                        {
                            currentSignal = signal;
                        }
                    }
                    break;

                case "Radio":
                    currentRadio = value;
                    break;

                case "Band":
                    currentBand = value;
                    break;

                case "Channel":
                    if (int.TryParse(value, out int channel) && channelToFrequencies.ContainsKey(channel))
                    {
                        currentChannel = channel;
                        currentFrequency = channelToFrequencies[channel];
                    }
                    break;
            }
        }

        private void AddCurrentDataToGlobalLists()
        {
            if (!string.IsNullOrEmpty(currentSSID) && !string.IsNullOrEmpty(currentBSSID))
            {
                SSID_LIST.Add(currentSSID);
                ENCRYPTION_TYPE_LIST.Add(currentEncryption);
                AUTH_LIST.Add(currentAuth);
                BSSID_LIST.Add(currentBSSID);
                SIGNAL_STRENGTH_LIST.Add(currentSignal);
                WIFI_STANDARD_LIST.Add(currentRadio);
                BAND_LIST.Add(currentBand);
                CHANNEL_LIST.Add(currentChannel);
                FREQUENCY_LIST.Add(currentFrequency);
            }
        }

        private void ClearCurrentData()
        {
            currentSSID = null;
            currentEncryption = null;
            currentAuth = null;
            ClearBssidRelatedData();
        }

        private void ClearBssidRelatedData()
        {
            currentBSSID = null;
            currentSignal = 0;
            currentRadio = null;
            currentBand = null;
            currentChannel = 0;
            currentFrequency = null;
        }

        private void InsertParsedDataToDatabase()
        {
            string LAST_TIME_FRAME_ID = _dbAccess.GetLastTimeFrameId();
            var idGenerator = new TimeFrameIdGenerator(LAST_TIME_FRAME_ID);


            string CURRENT_TIME_FRAME_ID = idGenerator.GenerateNextId();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[ DATA INGEST ENGINE ] SQL WRITE at {CURRENT_TIME_FRAME_ID}");

            for (int i = 0; i < SSID_LIST.Count; i++)
            {
                _dbAccess.DataIngestEngineInsertAccessPointData(
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