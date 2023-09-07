using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
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
        //#####################################################################################
        //#####                       DLL METHODS & STRUCT BELOW                        #######            
        //#####################################################################################
        #region
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
        #endregion
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
        private ACCESS_POINT_DATA _selectedSSIDItem;
        public double RSSI_VALUE
        {
            get => _rssiValueForGuage;
            set
            {
                _rssiValueForGuage = value;
                OnPropertyChanged(nameof(RSSI_VALUE));
            }
        }
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
                var (data24GHz, data5GHz) = PopulateSSIDInfoList(SSIDs.ToList());
                RefreshChannelAllocationChartsAsync(data24GHz, data5GHz);
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
        public async Task LoadWLANData()
        {
            NETWORK_INFO[] networkInfos = new NETWORK_INFO[100];

            int count = GetVisibleNetworks(networkInfos, networkInfos.Length);

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

        //#####################################################################################
        //#####                            GRAPHS BELOW                                 #######            
        //#####################################################################################
        #region

        public SeriesCollection CHANNEL_ALLOCATION_SERIES_5GHZ { get; set; } = new SeriesCollection();
        public SeriesCollection CHANNEL_ALLOCATION_SERIES_24GHZ { get; set; } = new SeriesCollection();

        public void RunOnUIThread(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }
        private (List<SSIDInfoForCHAllocation24GHz>, List<SSIDInfoForCHAllocation5GHz>) PopulateSSIDInfoList(List<ACCESS_POINT_DATA> accessPointDataList)
        {
            var ssidInfoList24Ghz = new List<SSIDInfoForCHAllocation24GHz>();
            var ssidInfoList5Ghz = new List<SSIDInfoForCHAllocation5GHz>();

            for (int i = 0; i < accessPointDataList.Count; i++)
            {
                var currentData = accessPointDataList[i];
                uint frequency = currentData.FREQUENCY;

                // Assuming 2.4GHz band frequencies are between 2400 and 2500 MHz
                // and 5GHz band frequencies are between 5000 and 6000 MHz
                if (frequency >= 2400000 && frequency <= 2500000)
                {
                    ssidInfoList24Ghz.Add(new SSIDInfoForCHAllocation24GHz
                    {
                        SSID = currentData.DISPLAY_SSID,
                        BSSID = currentData.BSSID,
                        Channel = currentData.CHANNEL,
                        SignalStrength = 150 
                    });
                }
                else if (frequency >= 5000000 && frequency <= 6000000)
                {
                    ssidInfoList5Ghz.Add(new SSIDInfoForCHAllocation5GHz
                    {
                        SSID = currentData.DISPLAY_SSID,
                        BSSID = currentData.BSSID,
                        Channel = currentData.CHANNEL,
                        SignalStrength = 150 
                    });
                }
            }

            return (ssidInfoList24Ghz, ssidInfoList5Ghz);
        }
         
        public async Task RefreshChannelAllocationChartsAsync(List<SSIDInfoForCHAllocation24GHz> data24GHz, List<SSIDInfoForCHAllocation5GHz> data5GHz)
        {
            var channelCount24GHz = await Task.Run(() => ChannelAllocationProcessData24GHz(data24GHz));
            UpdateChannelAllocationChart24GHz(channelCount24GHz);


            var channelCount5GHz = await Task.Run(() => ChannelAllocationProcessData5GHz(data5GHz));
            UpdateChannelAllocationChart5GHz(channelCount5GHz);
        }
        private Dictionary<int, List<(double rssi, string bssid)>> ChannelAllocationProcessData24GHz(List<SSIDInfoForCHAllocation24GHz> data)
        {
            var channelData = new Dictionary<int, List<(double rssi, string bssid)>>();

            foreach (var info in data)
            {
                if (!channelData.ContainsKey(info.Channel))
                {
                    channelData[info.Channel] = new List<(double rssi, string bssid)>();
                }

                // Estimate RSSI value 
                double rssi = (info.SignalStrength / 2) - 100;
                channelData[info.Channel].Add((rssi, info.BSSID));
            }

            return channelData;
        }
        private Dictionary<int, List<(double rssi, string bssid)>> ChannelAllocationProcessData5GHz(List<SSIDInfoForCHAllocation5GHz> data)
        {
            var channelData = new Dictionary<int, List<(double rssi, string bssid)>>();

            foreach (var info in data)
            {
                if (!channelData.ContainsKey(info.Channel))
                {
                    channelData[info.Channel] = new List<(double rssi, string bssid)>();
                }

                // Estimate RSSI value 
                double rssi = (info.SignalStrength / 2) - 100;
                channelData[info.Channel].Add((rssi, info.BSSID));
            }

            return channelData;
        }
        private void UpdateChannelAllocationChart5GHz(Dictionary<int, List<(double rssi, string bssid)>> data)
        {

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => UpdateChannelAllocationChart5GHz(data));
                return;
            }

            var defaultGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(61, 235, 154), 1),       // blue-green
                    new GradientStop(Color.FromArgb(60, 110, 204, 37), 0)   // Green 
                }
            };

            var selectedGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(239, 57, 69), 1),        // Red
                    new GradientStop(Color.FromArgb(60, 239, 57, 69), 0)  // Transparent Red
                }
            };

            CHANNEL_ALLOCATION_SERIES_5GHZ.Clear();

            foreach (var entry in data)
            {
                var channel = entry.Key;
                var signalStrengthsOnChannel = entry.Value;
                string channelToolTip = channel.ToString();

                foreach (var (rssi, bssid) in signalStrengthsOnChannel)
                {
                    var lineSeries = new LineSeries
                    {
                        Title = $"5GHz Channel:",
                        Values = new ChartValues<ObservablePoint>(),
                        PointGeometrySize = 10,
                        StrokeThickness = 2,
                        Stroke = bssid == SELECTED_SSID_ITEM?.BSSID
                        ? new SolidColorBrush(Color.FromRgb(239, 57, 69)) // Red for selected BSSID
                        : new SolidColorBrush(Color.FromRgb(66, 255, 192)),
                        Foreground = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                        Fill = bssid == SELECTED_SSID_ITEM?.BSSID ? selectedGradient : defaultGradient,
                        DataLabels = true
                    };
                    Dictionary<int, (int Start, int Peak, int End)> channelFrequencies5GHz = new Dictionary<int, (int Start, int Peak, int End)>
                    {
                    {36, (5170, 5180, 5190)},
                    {40, (5190, 5200, 5210)},
                    {44, (5210, 5220, 5230)},
                    {48, (5230, 5240, 5250)},
                    {52, (5250, 5260, 5270)},
                    {56, (5270, 5280, 5290)},
                    {60, (5290, 5300, 5310)},
                    {64, (5310, 5320, 5330)},
                    {100, (5490, 5500, 5510)},
                    {104, (5510, 5520, 5530)},
                    {108, (5530, 5540, 5550)},
                    {112, (5550, 5560, 5570)},
                    {116, (5570, 5580, 5590)},
                    {120, (5590, 5600, 5610)},
                    {124, (5610, 5620, 5630)},
                    {128, (5630, 5640, 5650)},
                    {132, (5650, 5660, 5670)},
                    {136, (5670, 5680, 5690)},
                    {140, (5690, 5700, 5710)},
                    {144, (5710, 5720, 5730)},
                    {149, (5735, 5745, 5755)},
                    {153, (5755, 5765, 5775)},
                    {157, (5775, 5785, 5795)},
                    {161, (5795, 5805, 5815)},
                    {165, (5815, 5825, 5835)}
                    };

                    if (channelFrequencies5GHz.ContainsKey(channel))
                    {
                        var freqRange = channelFrequencies5GHz[channel];

                        // Start Frequency, y-value is -100
                        lineSeries.Values.Add(new ObservablePoint(freqRange.Start, -100));

                        // Peak Frequency, y-value is RSSI
                        lineSeries.Values.Add(new ObservablePoint(freqRange.Peak, rssi));

                        // End Frequency, y-value is -100
                        lineSeries.Values.Add(new ObservablePoint(freqRange.End, -100));
                    }
                    lineSeries.LabelPoint = point =>
                    {
                        var maxVal = (lineSeries.Values as ChartValues<ObservablePoint>).Max(p => p.Y);
                        if (point.Y == maxVal)
                        {
                            return $"CH: {channel}";
                        }
                        return "";
                    };


                    CHANNEL_ALLOCATION_SERIES_5GHZ.Add(lineSeries);
                }
            }
        }
        private void UpdateChannelAllocationChart24GHz(Dictionary<int, List<(double rssi, string bssid)>> data)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => UpdateChannelAllocationChart24GHz(data));
                return;
            }

            var defaultGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(61, 235, 154), 1),       // blue-green
                    new GradientStop(Color.FromArgb(60, 110, 204, 37), 0)   // Green 
                }
            };

            var selectedGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(239, 57, 69), 1),        // Red
                    new GradientStop(Color.FromArgb(60, 239, 57, 69), 0)  // Transparent Red
                }
            };

            CHANNEL_ALLOCATION_SERIES_24GHZ.Clear();

            foreach (var entry in data)
            {
                var channel = entry.Key;
                var signalStrengthsOnChannel = entry.Value;
                string channelToolTip = channel.ToString();

                foreach (var (rssi, bssid) in signalStrengthsOnChannel)
                {
                    var lineSeries = new LineSeries
                    {
                        Title = $"2.4GHz Channel:",
                        Values = new ChartValues<ObservablePoint>(),
                        PointGeometrySize = 10,
                        StrokeThickness = 2,
                        Stroke = bssid == SELECTED_SSID_ITEM?.BSSID
                        ? new SolidColorBrush(Color.FromRgb(239, 57, 69)) // Red for selected BSSID
                        : new SolidColorBrush(Color.FromRgb(66, 255, 192)),
                        Foreground = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                        Fill = bssid == SELECTED_SSID_ITEM?.BSSID ? selectedGradient : defaultGradient,
                        DataLabels = true
                    };

                    Dictionary<int, (int Start, int Peak, int End)> channelFrequencies = new Dictionary<int, (int Start, int Peak, int End)>
                        {
                            {1, (2402, 2412, 2422)},
                            {2, (2407, 2417, 2427)},
                            {3, (2412, 2422, 2432)},
                            {4, (2417, 2427, 2437)},
                            {5, (2422, 2432, 2442)},
                            {6, (2427, 2437, 2447)},
                            {7, (2432, 2442, 2452)},
                            {8, (2437, 2447, 2457)},
                            {9, (2442, 2452, 2462)},
                            {10, (2447, 2457, 2467)},
                            {11, (2452, 2462, 2472)},
                            {12, (2457, 2467, 2477)},
                            {13, (2462, 2472, 2482)},
                            {14, (2473, 2484, 2495)}
                        };

                    if (channelFrequencies.ContainsKey(channel))
                    {
                        var freqRange = channelFrequencies[channel];

                        // Start Frequency, y-value is -100
                        lineSeries.Values.Add(new ObservablePoint(freqRange.Start, -100));

                        // Peak Frequency, y-value is RSSI
                        lineSeries.Values.Add(new ObservablePoint(freqRange.Peak, rssi));

                        // End Frequency, y-value is -100
                        lineSeries.Values.Add(new ObservablePoint(freqRange.End, -100));
                    }
                    lineSeries.LabelPoint = point =>
                    {
                        var maxVal = (lineSeries.Values as ChartValues<ObservablePoint>).Max(p => p.Y);
                        if (point.Y == maxVal)
                        {
                            //return channel.ToString();
                            return $"CH: {channel}";
                        }
                        return "";
                    };



                    CHANNEL_ALLOCATION_SERIES_24GHZ.Add(lineSeries);
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class SSIDInfoForCHAllocation24GHz
    {
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public int Channel { get; set; }
        public double SignalStrength { get; set; }
    }
    public class SSIDInfoForCHAllocation5GHz
    {
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public int Channel { get; set; }
        public double SignalStrength { get; set; }
    }
}
