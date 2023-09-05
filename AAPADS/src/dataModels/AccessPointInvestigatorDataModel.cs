using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace AAPADS
{
    public class ACCESS_POINT_DATA
    {
        public string DISPLAY_SSID { get; set; }
        public string ORIGNAL_SSID { get; set; }
        public string AUTH_METHOD { get; set; }
        public string ENCRYPTION_TYPE { get; set; }
        public string BSSID { get; set; }
        public int CHANNEL { get; set; }
        public string BSS_TYPE { get; set; }
        public string BSSID_DOT11_TYPE { get; set; }
        public string BSSID_DOT11_TYPE_DESC { get; set; }
        public int BEACON_PERIOD { get; set; }
        public uint FREQUENCY { get; set; }
    }
    public class AccessPointInvestigatorDataModel : INotifyPropertyChanged
    {

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetVisibleNetworks([Out] NETWORK_INFO[] networks, int maxNetworks);

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetAvailableSSIDs(ref SSIDList ssidList);

        [DllImport("WLANLibrary.dll", CharSet = CharSet.Ansi)]
        public static extern int GetRSSIForSSID(string ssid);

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PerformWifiScan();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SSIDList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3200)]
            public char[] ssids;
            public int count;
        }
        SSIDList ssidList = new SSIDList();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NETWORK_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ssid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string bssid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string authMethod;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string encryptionType;

            public int bssidPhyType;

            public int channel;

            public int bssType;

            public int beaconPeriod;

            public uint frequency;
        }

        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<ACCESS_POINT_DATA> _ssids = new ObservableCollection<ACCESS_POINT_DATA>();
        public ObservableCollection<ACCESS_POINT_DATA> SSIDs
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

        private ACCESS_POINT_DATA _selectedSSIDItem;
        public ACCESS_POINT_DATA SELECTED_SSID_ITEM
        {
            get => _selectedSSIDItem;
            set
            {
                if (_selectedSSIDItem?.BSSID == value?.BSSID)
                    return; // If selected BSSID has not changed continue

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
                        new GradientStop(Color.FromArgb(60,110, 204, 37), 0)
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
            NETWORK_INFO[] networkInfos = new NETWORK_INFO[100];

            int count = GetVisibleNetworks(networkInfos, networkInfos.Length);

            Console.WriteLine($"Number of SSIDs retrieved: {count}");

            var newSSIDs = new List<ACCESS_POINT_DATA>();

            foreach (var network in networkInfos.Take(count))
            {
                var ssidItem = new ACCESS_POINT_DATA
                {
                    BSSID = network.bssid,
                    DISPLAY_SSID = string.IsNullOrWhiteSpace(network.ssid) ? "[HIDDEN NETWORK]" : network.ssid,
                    AUTH_METHOD = network.authMethod,
                    ENCRYPTION_TYPE = network.encryptionType,
                    CHANNEL = network.channel,
                    BSSID_DOT11_TYPE = mapBSSPHYType(network.bssidPhyType),
                    BSSID_DOT11_TYPE_DESC = mapBSSPHYTypeDesc(network.bssidPhyType),
                    BSS_TYPE = mapBssType(network.bssType),
                    BEACON_PERIOD = network.beaconPeriod,
                    FREQUENCY = network.frequency,
                };

                newSSIDs.Add(ssidItem);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update the SSID list in place
                foreach (var ssid in newSSIDs)
                {
                    Console.WriteLine($"New SSID: {ssid.DISPLAY_SSID}, BSSID: {ssid.BSSID}");
                    if (!SSIDs.Any(s => s.BSSID == ssid.BSSID)) // Use BSSID for uniqueness
                    {
                        SSIDs.Add(ssid);
                    }
                }

                for (int i = SSIDs.Count - 1; i >= 0; i--)
                {
                    if (!newSSIDs.Any(s => s.BSSID == SSIDs[i].BSSID)) // Use BSSID for uniqueness
                    {
                        SSIDs.RemoveAt(i);
                    }
                }
            });
        }
        public void RefreshSSIDs()
        {
            try
            {
                // Load the new WLAN - C library call 
                LoadWLANData();
            }
            catch
            {
                MessageBox.Show("Error calling LoadWLANData from WLANLibrary.dll");
            };

            var currentSSIDs = SSIDs.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Add new SSIDs
                foreach (var ssid in currentSSIDs)
                {
                    if (!SSIDs.Any(s => s.BSSID == ssid.BSSID)) // Use BSSID for uniqueness
                    {
                        SSIDs.Add(ssid);
                    }
                }

                // Remove SSIDs that are no longer in the current SSIDs list
                for (int i = SSIDs.Count - 1; i >= 0; i--)
                {
                    if (!currentSSIDs.Any(s => s.BSSID == SSIDs[i].BSSID)) // Use BSSID for uniqueness
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
                    var rssi = GetRSSIForSSID(SELECTED_SSID_ITEM?.ORIGNAL_SSID ?? string.Empty);

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

                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
        private string mapBssType(int value)
        {
            string BSSType;

            switch (value)
            {
                case 1:
                    BSSType = "INFRASTRUCTURE";
                    break;
                case 2:
                    BSSType = "INDEPENDENT";
                    break;
                case 3:
                    BSSType = "ANY";
                    break;
                default:
                    BSSType = "UNKNOWN";
                    break;
            }
            return BSSType;
        }

        private string mapBSSPHYType(int value)
        {
            //dot11_phy_type_unknown = 0,
            //dot11_phy_type_any = 0,
            //dot11_phy_type_fhss = 1,
            //dot11_phy_type_dsss = 2,
            //dot11_phy_type_irbaseband = 3,
            //dot11_phy_type_ofdm = 4,
            //dot11_phy_type_hrdsss = 5,
            //dot11_phy_type_erp = 6,
            //dot11_phy_type_ht = 7,
            //dot11_phy_type_vht = 8,
            //dot11_phy_type_dmg = 9,
            //dot11_phy_type_he = 10,
            //dot11_phy_type_eht = 11,
            //dot11_phy_type_IHV_start = 0x80000000,
            //dot11_phy_type_IHV_end = 0xffffffff


            string BSSPHYType;

            switch (value) { 
                case 0:
                    BSSPHYType = "UNKNOWN";
                    break;
                case 1:
                    BSSPHYType = "(O) 802.11"; //Orignal 802.11
                    break;
                case 2:
                    BSSPHYType = "(O) 802.11"; //Orignal 802.11
                    break;  
                case 3:
                    BSSPHYType = "(IR) 802.11"; // Infrared baseband 802.11
                    break;
                case 4:
                    BSSPHYType= "802.11a/g";
                    break;
                case 5:
                    BSSPHYType = "802.11b";
                    break;
                case 6:
                    BSSPHYType = "802.11g";
                    break;
                case 7:
                    BSSPHYType = "802.11n";
                    break;
                case 8:
                    BSSPHYType = "802.11ac";
                    break;
                case 9:
                    BSSPHYType = "802.11ad";
                    break;
                case 10:
                    BSSPHYType = "802.11ax";
                    break;
                case 11:
                    BSSPHYType = "802.11be";
                    break;
                default : 
                    BSSPHYType = "UNKNOWN";
                    break;
            }


            return BSSPHYType;
        }
        private string mapBSSPHYTypeDesc(int value)
        {
            string BSSPHYTypeDesc;

            switch (value)
            {
                case 0:
                    BSSPHYTypeDesc = "An unknown or uninitialized PHY type";
                    break;
                case 1:
                    BSSPHYTypeDesc = "Frequency-hopping spread-spectrum (FHSS) PHY. Bluetooth devices can use FHSS or an adaptation of FHSS";
                    break;
                case 2:
                    BSSPHYTypeDesc = "Direct sequence spread spectrum (DSSS) PHY type.";
                    break;
                case 3:
                    BSSPHYTypeDesc = "Infrared (IR) baseband PHY type";
                    break;
                case 4:
                    BSSPHYTypeDesc = "Orthogonal frequency division multiplexing (OFDM) PHY type. 802.11a devices can use OFDM";
                    break;
                case 5:
                    BSSPHYTypeDesc = "High-rate DSSS (HRDSSS) PHY type";
                    break;
                case 6:
                    BSSPHYTypeDesc = "Extended rate PHY type (ERP). 802.11g devices can use ERP";
                    break;
                case 7:
                    BSSPHYTypeDesc = "802.11n PHY type";
                    break;
                case 8:
                    BSSPHYTypeDesc = "The very high throughput (VHT) PHY type specified in IEEE 802.11ac";
                    break;
                case 9:
                    BSSPHYTypeDesc = "Directional Multi-Gigabit (DMG) 802.11ad PHY";
                    break;
                case 10:
                    BSSPHYTypeDesc = "High Efficiency (HE) 802.11ax PHY";
                    break;
                case 11:
                    BSSPHYTypeDesc = "Extremely high-throughput (EHT) 802.11be PHY";
                    break;
                default:
                    BSSPHYTypeDesc = "An unknown or uninitialized PHY type";
                    break;
            }


            return BSSPHYTypeDesc;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
