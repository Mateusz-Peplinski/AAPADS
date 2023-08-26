using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public ObservableCollection<SSIDItem> BSSIDs { get; set; } = new ObservableCollection<SSIDItem>();


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
                _selectedSSIDItem = value;
                OnPropertyChanged(nameof(SELECTED_SSID_ITEM));

                RSSIDataForGraphSignalStrengthOverTime[0].Values.Clear();

                _cancellationTokenSource?.Cancel();

                if (_selectedSSIDItem != null)
                {
                    //LoadRSSIDataForSSID(_selectedSSIDItem.BSSID);
                }
            }
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
            //PerformWifiScan();
            //LoadWLANData();
        }
        //public void LoadWLANData()
        //{
        //    GetAvailableSSIDs_Extended(ref _extendedSsidList);
        //    Console.WriteLine($"Number of SSIDs retrieved: {_extendedSsidList.count}");
        //    foreach (var bssidItem in GetAllBSSIDItems())
        //    {
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            Console.WriteLine($"BSSID: {bssidItem.BSSID}, SSID: {bssidItem.DisplaySSID}, Auth: {bssidItem.AuthMethod}, Encryption: {bssidItem.EncryptionType}, Channel: {bssidItem.Channel}");
        //            SSIDs.Add(bssidItem);
        //        });

        //    }
        //}
        //public IEnumerable<SSIDItem> GetAllBSSIDItems()
        //{
        //    for (int i = 0; i < _extendedSsidList.count; i++)
        //    {
        //        for (int j = 0; j < MAX_BSSIDS_PER_SSID; j++)
        //        {
        //            string bssid = new string(_extendedSsidList.bssids.Skip((i * MAX_BSSIDS_PER_SSID + j) * 17).Take(17).ToArray()).TrimEnd('\0');
        //            if (!string.IsNullOrEmpty(bssid))
        //            {
        //                string authMethod = new string(_extendedSsidList.authMethods.Skip(i * MAX_SSID_LENGTH).Take(MAX_SSID_LENGTH).ToArray()).TrimEnd('\0');
        //                string encryptionType = new string(_extendedSsidList.encryptionTypes.Skip(i * MAX_SSID_LENGTH).Take(MAX_SSID_LENGTH).ToArray()).TrimEnd('\0');
        //                int channel = _extendedSsidList.Channel[i * MAX_BSSIDS_PER_SSID + j];
        //                string originalSSID = new string(_extendedSsidList.ssids.Skip(i * MAX_SSID_LENGTH).Take(MAX_SSID_LENGTH).ToArray()).TrimEnd('\0');

        //                yield return new SSIDItem
        //                {
        //                    AuthMethod = authMethod,
        //                    EncryptionType = encryptionType,
        //                    BSSID = bssid,
        //                    Channel = channel,
        //                    OriginalSSID = originalSSID
        //                };
        //            }
        //        }
        //    }
        //}


        //public IEnumerable<SSIDItem> GetSSIDItemsFromCharArray(char[] charArray, int count)
        //{
        //    for (int i = 0; i < count; i++)
        //    {
        //        var originalSSID = new string(charArray.Skip(i * 32).Take(32).ToArray()).TrimEnd('\0');

        //        var displaySSID = new string(originalSSID.Where(c => c >= 32 && c <= 126).ToArray());
        //        if (string.IsNullOrWhiteSpace(displaySSID))
        //        {
        //            displaySSID = "[HIDDEN NETWORK]";
        //        }
        //        yield return new SSIDItem
        //        {
        //            DisplaySSID = displaySSID,
        //            OriginalSSID = originalSSID
        //        };
        //    }
        //}
       
        //public void RefreshSSIDs()
        //{
        //    GetAvailableSSIDs_Extended(ref _extendedSsidList);
        //    var currentSSIDs = GetSSIDItemsFromCharArray(_extendedSsidList.ssids, _extendedSsidList.count).ToList();
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        // Add new SSIDs
        //        foreach (var ssid in currentSSIDs)
        //        {
        //            if (!SSIDs.Any(s => s.OriginalSSID == ssid.OriginalSSID))
        //            {
        //                SSIDs.Add(ssid);
        //            }
        //        }

        //        // Remove SSIDs 
        //        for (int i = SSIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (!currentSSIDs.Any(s => s.OriginalSSID == SSIDs[i].OriginalSSID))
        //            {
        //                SSIDs.RemoveAt(i);
        //            }
        //        }
        //    });
        //}

        //private void LoadRSSIDataForSSID(string ssid)
        //{
        //    PopulateRSSIValueDataInGaugeAndLineSeries();
        //}

        //private async void PopulateRSSIValueDataInGaugeAndLineSeries()
        //{
        //    _cancellationTokenSource?.Cancel();
        //    _cancellationTokenSource = new CancellationTokenSource();

        //    int refreshInterval = 10;
        //    int counter = 0;

        //    while (!_cancellationTokenSource.Token.IsCancellationRequested)
        //    {
        //        var rssi = ConvertSignalQualityToRssi(GetRSSIForSSID(SELECTED_SSID_ITEM?.OriginalSSID ?? string.Empty));

        //        RSSIDataForGraphSignalStrengthOverTime[0].Values.Add(rssi);
        //        if (RSSIDataForGraphSignalStrengthOverTime[0].Values.Count > 100)
        //        {
        //            RSSIDataForGraphSignalStrengthOverTime[0].Values.RemoveAt(0);
        //        }
        //        RSSI_VALUE = rssi;
        //        PerformWifiScan();

        //        counter++;
        //        if (counter >= refreshInterval)
        //        {
        //            RefreshSSIDs();
        //            counter = 0;
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //    }
        //}

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
