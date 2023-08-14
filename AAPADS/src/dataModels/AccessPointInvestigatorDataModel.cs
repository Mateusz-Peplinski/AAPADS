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
using System.Windows.Media;
using System.Windows;


namespace AAPADS
{
    public class SSIDItem
    {
        public string DisplaySSID { get; set; } 
        public string OriginalSSID { get; set; }
    }


    public class AccessPointInvestigatorDataModel : INotifyPropertyChanged
    {
        private CancellationTokenSource _cancellationTokenSource;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SSIDList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3200)]
            public char[] ssids;
            public int count;
        }
        SSIDList ssidList = new SSIDList();

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetAvailableSSIDs(ref SSIDList ssidList);

        [DllImport("WLANLibrary.dll", CharSet = CharSet.Ansi)]
        public static extern int GetRSSIForSSID(string ssid);

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PerformWifiScan();

        public IEnumerable<SSIDItem> GetSSIDItemsFromCharArray(char[] charArray, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var originalSSID = new string(charArray.Skip(i * 32).Take(32).ToArray()).TrimEnd('\0');

                var displaySSID = new string(originalSSID.Where(c => c >= 32 && c <= 126).ToArray());

                yield return new SSIDItem
                {
                    DisplaySSID = displaySSID,
                    OriginalSSID = originalSSID
                };
            }
        }
        
        public AccessPointInvestigatorDataModel()
        {
            ssidList.ssids = new char[3200];

            LoadSSIDs();
        }

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

        private SSIDItem _selectedSSIDItem;
        public SSIDItem SelectedSSIDItem
        {
            get => _selectedSSIDItem;
            set
            {
                _selectedSSIDItem = value;
                OnPropertyChanged(nameof(SelectedSSIDItem));

                RSSIDataForGraph3[0].Values.Clear();

                _cancellationTokenSource?.Cancel();

                if (_selectedSSIDItem != null)
                    LoadRSSIDataForSSID(_selectedSSIDItem.OriginalSSID);
            }
        }

        private SeriesCollection _rssiDataForGraph3 = new SeriesCollection
        {
            new LineSeries
            {
                Values = new ChartValues<int>(),
                Title = "RSSI Value",
                PointGeometrySize = 0,
                StrokeThickness = 0,
                Stroke = Brushes.SkyBlue,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 1),
                    EndPoint = new Point(0, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(255, 0, 0), 1),       // Red 
                        new GradientStop(Color.FromRgb(255, 69, 0), 0.8),
                        new GradientStop(Color.FromRgb(255, 165, 0), 0.6),
                        new GradientStop(Color.FromRgb(255, 255, 0), 0.4),
                        new GradientStop(Color.FromRgb(173, 255, 47), 0.25),
                        new GradientStop(Color.FromRgb(0, 128, 0), 0)        // Green 

                    }
                }
            }
        };

        public SeriesCollection RSSIDataForGraph3
        {
            get => _rssiDataForGraph3;
            set
            {
                _rssiDataForGraph3 = value;
                OnPropertyChanged(nameof(RSSIDataForGraph3));
            }
        }

        public void LoadSSIDs()
        {
            GetAvailableSSIDs(ref ssidList);
            foreach (var ssidItem in GetSSIDItemsFromCharArray(ssidList.ssids, ssidList.count))
            {
                SSIDs.Add(ssidItem);
            }
        }
        public void RefreshSSIDs()
        {
            GetAvailableSSIDs(ref ssidList);
            var currentSSIDs = GetSSIDItemsFromCharArray(ssidList.ssids, ssidList.count).ToList();

            // Add new SSIDs
            foreach (var ssid in currentSSIDs)
            {
                if (!SSIDs.Any(s => s.OriginalSSID == ssid.OriginalSSID))
                {
                    SSIDs.Add(ssid);
                }
            }

            // Remove SSIDs that are no longer available
            for (int i = SSIDs.Count - 1; i >= 0; i--)
            {
                if (!currentSSIDs.Any(s => s.OriginalSSID == SSIDs[i].OriginalSSID))
                {
                    SSIDs.RemoveAt(i);
                }
            }
        }

        private void LoadRSSIDataForSSID(string ssid)
        {
            StartRSSIPolling();
        }

        private async void StartRSSIPolling()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            int refreshInterval = 10;  
            int counter = 0;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var rssi = ConvertSignalQualityToRssi(GetRSSIForSSID(SelectedSSIDItem?.OriginalSSID ?? string.Empty));

                RSSIDataForGraph3[0].Values.Add(rssi);
                if (RSSIDataForGraph3[0].Values.Count > 100)
                {
                    RSSIDataForGraph3[0].Values.RemoveAt(0);
                }

                PerformWifiScan();

                counter++;
                if (counter >= refreshInterval)
                {
                    RefreshSSIDs();
                    counter = 0; 
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
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
