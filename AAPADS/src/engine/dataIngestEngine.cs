using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
        public List<string> AUTH_LIST = new List<string>();
        public bool isLoading = false;  
        

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

        //private overviewViewDataModel liveLogDataModelConsole;
        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PerformWifiScan();


        public void Start()
        {
            //liveLogDataModelConsole = new overviewViewDataModel();

            SSID_LIST = new List<string>();
            ENCRYPTION_TYPE_LIST = new List<string>();
            BSSID_LIST = new List<string>();
            SIGNAL_STRENGTH_LIST = new List<int>();
            WIFI_STANDARD_LIST = new List<string>();
            BAND_LIST = new List<string>();
            CHANNEL_LIST = new List<int>();
            FREQUENCY_LIST = new List<string>();
            AUTH_LIST = new List<string>();

            Task.Run(RunNetworkScanning);
        }

        private async Task RunNetworkScanning()
        {
            while (isRunning)
            {
                await semaphore.WaitAsync();
                if (programLoadCount == 0)
                {
                    isLoading = true;
                    programLoadCount++;
                }
                try
                {
                    PerformWifiScan();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (SSID_LIST != null && ENCRYPTION_TYPE_LIST != null && BSSID_LIST != null &&
                    SIGNAL_STRENGTH_LIST != null && WIFI_STANDARD_LIST != null && BAND_LIST != null &&
                    CHANNEL_LIST != null && FREQUENCY_LIST != null && AUTH_LIST != null)
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

                RunNetshCommand();
                
                SSIDDataCollected?.Invoke(this, EventArgs.Empty);
                isLoading = false;
                semaphore.Release();

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }


        private void RunNetshCommand()
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
                process.OutputDataReceived += Process_OutputDataReceived;

                process.Start();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                ParseNetworkInformation(e.Data);
            }
        }
        

        private void ParseNetworkInformation(string output)
        {
            output = output.Trim();
            int index = output.IndexOf(":");
            if (index < 0 || index >= output.Length - 1)
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
       
    }
    

}