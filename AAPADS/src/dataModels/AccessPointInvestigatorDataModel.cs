using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace AAPADS
{
    public class SSIDItem
    {
        public string DisplaySSID { get; set; }
        public string OriginalSSID { get; set; }
        public string AuthMethod { get; set; }
        public string EncryptionType { get; set; }
        public string BSSID { get; set; }
        public int Channel { get; set; }
    }



    public class AccessPointInvestigatorDataModel : INotifyPropertyChanged
    {
        const int MAX_SSID_LENGTH = 32;
        const int MAX_BSSIDS_PER_SSID = 5;

        private CancellationTokenSource _cancellationTokenSource;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SSIDList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3200)]
            public char[] ssids;
            public int count;
        }
        SSIDList ssidList = new SSIDList();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NetworkInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ssid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string bssid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string authMethod;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string encryptionType;

            public int channel;
        }


        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetVisibleNetworks([Out] NetworkInfo[] networks, int maxNetworks);


        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetAvailableSSIDs(ref SSIDList ssidList);

        [DllImport("WLANLibrary.dll", CharSet = CharSet.Ansi)]
        public static extern int GetRSSIForSSID(string ssid);

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PerformWifiScan();


        private ObservableCollection<SSIDItem> _ssids = new ObservableCollection<SSIDItem>();
        public ObservableCollection<SSIDItem> SSIDs
        {
            get => _ssids;
            set
            {
                _ssids = value;
                OnPropertyChanged(nameof(SSIDs));
            }
        }

        private double _rssiValueForGuage;
        public double RSSI_VALUE
        {
            get => _rssiValueForGuage;
            set
            {
                _rssiValueForGuage = value;
                OnPropertyChanged(nameof(RSSI_VALUE));
            }
        }

        private SSIDItem _selectedSSIDItem;
        public SSIDItem SELECTED_SSID_ITEM
        {
            get => _selectedSSIDItem;
            set
            {
                if (_selectedSSIDItem?.DisplaySSID == value?.DisplaySSID)
                    return; // If selected SSID has not changed contine

                _selectedSSIDItem = value;
                OnPropertyChanged(nameof(SELECTED_SSID_ITEM));

                // Only clear the graph if the SSID has changed
                ClearGraphData();

                // Start collecting data for the new SSID
                StartDataCollectionForSSID(_selectedSSIDItem?.BSSID);
            }
        }

        private void ClearGraphData()
        {
            RSSIDataForGraphSignalStrengthOverTime[0].Values.Clear();
        }

        private void StartDataCollectionForSSID(string ssid)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            PopulateRSSIValueDataInGaugeAndLineSeries();
        }




        private SeriesCollection _rssiDataForGraph = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<int>(),
                Title = "RSSI Value",
                PointGeometrySize = 0,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Color.FromRgb(66, 255, 192)),
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 1),
                    EndPoint = new Point(0, 0),
                    GradientStops = new GradientStopCollection
                    {
                        //new GradientStop(Color.FromRgb(255, 0, 0), 1),       // Red 
                        //new GradientStop(Color.FromRgb(255, 69, 0), 0.8),
                        //new GradientStop(Color.FromRgb(255, 165, 0), 0.6),
                        //new GradientStop(Color.FromRgb(255, 255, 0), 0.4),
                        //new GradientStop(Color.FromRgb(173, 255, 47), 0.25),
                        //new GradientStop(Color.FromRgb(0, 128, 0), 0)        // Green 

                        new GradientStop(Color.FromRgb(61, 235, 154), 1),       // blue-green
                        new GradientStop(Color.FromRgb(110, 204, 37), 0)        // Green 
                    }
                }
            }
        };

        public SeriesCollection RSSIDataForGraphSignalStrengthOverTime
        {
            get => _rssiDataForGraph;
            set
            {
                _rssiDataForGraph = value;
                OnPropertyChanged(nameof(RSSIDataForGraphSignalStrengthOverTime));
            }
        }

        public AccessPointInvestigatorDataModel()
        {
            ssidList.ssids = new char[3200];

            PerformWifiScan();

            LoadWLANData();
        }
        public void LoadWLANData()
        {
            NetworkInfo[] networkInfos = new NetworkInfo[100]; // Assuming a max of 100 networks for example

            int count = GetVisibleNetworks(networkInfos, networkInfos.Length);

            Console.WriteLine($"Number of SSIDs retrieved: {count}");

            var newSSIDs = new List<SSIDItem>();

            foreach (var network in networkInfos.Take(count))
            {
                var ssidItem = new SSIDItem
                {
                    BSSID = network.bssid,
                    DisplaySSID = string.IsNullOrWhiteSpace(network.ssid) ? "[HIDDEN NETWORK]" : network.ssid,
                    AuthMethod = network.authMethod,
                    EncryptionType = network.encryptionType,
                    Channel = network.channel
                };

                newSSIDs.Add(ssidItem);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update the SSID list in place
                foreach (var ssid in newSSIDs)
                {
                    if (!SSIDs.Any(s => s.DisplaySSID == ssid.DisplaySSID))
                    {
                        SSIDs.Add(ssid);
                    }
                }

                for (int i = SSIDs.Count - 1; i >= 0; i--)
                {
                    if (!newSSIDs.Any(s => s.DisplaySSID == SSIDs[i].DisplaySSID))
                    {
                        SSIDs.RemoveAt(i);
                    }
                }
            });
        }




        public IEnumerable<SSIDItem> GetSSIDItemsFromCharArray(char[] charArray, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var originalSSID = new string(charArray.Skip(i * 32).Take(32).ToArray()).TrimEnd('\0');

                var displaySSID = new string(originalSSID.Where(c => c >= 32 && c <= 126).ToArray());
                if (string.IsNullOrWhiteSpace(displaySSID) || displaySSID == "")
                {
                    displaySSID = "[HIDDEN NETWORK]";
                }
                yield return new SSIDItem
                {
                    DisplaySSID = displaySSID,
                    OriginalSSID = originalSSID
                };
            }
        }

        public void RefreshSSIDs()
        {
            
            try 
            {
                // Load the new WLAN - C libary call 
                LoadWLANData();
            } 
            catch { MessageBox.Show("Error calling LoadWLANData from WLANLibrary.dll"); };
            
            
            var currentSSIDs = SSIDs.ToList();
            

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new SSIDs
                foreach (var ssid in currentSSIDs)
                {
                    if (!SSIDs.Any(s => s.OriginalSSID == ssid.OriginalSSID))
                    {
                        SSIDs.Add(ssid);
                    }
                }

                // Remove SSIDs that are no longer in the current SSIDs list
                for (int i = SSIDs.Count - 1; i >= 0; i--)
                {
                    if (!currentSSIDs.Any(s => s.OriginalSSID == SSIDs[i].OriginalSSID))
                    {
                        SSIDs.RemoveAt(i);
                    }
                }
            });
        }

        private async void PopulateRSSIValueDataInGaugeAndLineSeries()
        {
            int refreshInterval = 10;
            int counter = 0;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // GetRSSIForSSID C Libray dll call
                try
                {
                    var rssi = ConvertSignalQualityToRssi(GetRSSIForSSID(SELECTED_SSID_ITEM?.OriginalSSID ?? string.Empty));

                    RSSIDataForGraphSignalStrengthOverTime[0].Values.Add(rssi);
                    if (RSSIDataForGraphSignalStrengthOverTime[0].Values.Count > 100)
                    {
                        RSSIDataForGraphSignalStrengthOverTime[0].Values.RemoveAt(0);
                    }
                    RSSI_VALUE = rssi;
                    PerformWifiScan();
                }
                catch
                {
                    MessageBox.Show("Error calling method from WLANLibrary.dll \nREF: \n  PerformWifiScan(); \n  GetRSSIForSSID();");
                }

                counter++;
                if (counter >= refreshInterval)
                {
                    RefreshSSIDs();
                    counter = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private int ConvertSignalQualityToRssi(int signalQuality)
        {
            return (signalQuality / 2) - 100;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
