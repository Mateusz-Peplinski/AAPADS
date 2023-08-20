using AAPADS;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

public class overviewViewDataModel : baseDataModel, INotifyPropertyChanged
{
    public ObservableCollection<dataModelStructure> AccessPoints { get; }

    public SeriesCollection FrequencySeriesCollection { get; set; } = new SeriesCollection
    {
        new LiveCharts.Wpf.LineSeries
        {
            Title = "2.4GHz",
            Values = new ChartValues<int>()
        },
        new LiveCharts.Wpf.LineSeries
        {
            Title = "5GHz",
            Values = new ChartValues<int>()
        }
    };
    public SeriesCollection ChannelAllocationSeries { get; set; } = new SeriesCollection();
    public Func<double, string> YAxisFormatter { get; set; }

    public Func<double, string> DateTimeFormatter { get; set; }

    private int _totalDetectedAP;
    private int _totalSecureAP;
    private int _total24GHzNetworks;
    private int _total5GHzNetworks;
    private bool _isLoading;
    private double _summarySectionHeight = 300;
    public int TOTAL_DETECTED_AP
    {
        get { return _totalDetectedAP; }
        set
        {
            if (_totalDetectedAP != value)
            {
                _totalDetectedAP = value;
                OnPropertyChanged(nameof(TOTAL_DETECTED_AP));
            }
        }
    }
    public int TOTAL_SECURE_AP
    {
        get { return _totalSecureAP; }
        set
        {
            if (_totalSecureAP != value)
            {
                _totalSecureAP = value;
                OnPropertyChanged(nameof(TOTAL_SECURE_AP));
            }
        }
    }
    public int TOTAL_2_4_GHz_AP
    {
        get { return _total24GHzNetworks; }
        set
        {
            if (_total24GHzNetworks != value)
            {
                _total24GHzNetworks = value;
                OnPropertyChanged(nameof(TOTAL_2_4_GHz_AP));
            }
        }
    }
    public int TOTAL_5_GHz_AP
    {
        get { return _total5GHzNetworks; }
        set
        {
            if (_total5GHzNetworks != value)
            {
                _total5GHzNetworks = value;
                OnPropertyChanged(nameof(TOTAL_5_GHz_AP));
            }
        }
    }
    public bool IS_LOADING
    {
        get { return _isLoading; }
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IS_LOADING));
            }
        }
    }
    public double SummarySectionHeight
    {
        get { return _summarySectionHeight; }
        set
        {
            _summarySectionHeight = value;
            OnPropertyChanged(nameof(SummarySectionHeight));
        }
    }


    public overviewViewDataModel()
    {
        AccessPoints = new ObservableCollection<dataModelStructure>();

        FrequencySeriesCollection = new SeriesCollection
        {

            new LiveCharts.Wpf.LineSeries
            {
                Title = "2.4GHz",
                Values = new ChartValues<int>(),
                PointGeometrySize = 10,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 73, 60)),
                PointForeground = new SolidColorBrush(Color.FromRgb(255, 73, 60)),
                StrokeThickness = 2,
                Fill = new LinearGradientBrush
                {
                        StartPoint = new Point(0, 1),
                        EndPoint = new Point(0, 0),
                        GradientStops = new GradientStopCollection
                        {

                        new GradientStop(Color.FromRgb(239, 57, 69), 1),
                        new GradientStop(Color.FromArgb(0,239, 57, 69), 0.3),
                        new GradientStop(Color.FromArgb(0,255, 255, 255), 0)
                        }
                }
            },

            new LiveCharts.Wpf.LineSeries
            {
                Title = "5GHz",
                Values = new ChartValues<int>(),
                PointGeometrySize = 10,
                Stroke = new SolidColorBrush(Color.FromRgb(22, 125, 255)),
                PointForeground = new SolidColorBrush(Color.FromRgb(22, 125, 255)),
                StrokeThickness = 2,
                Fill = new LinearGradientBrush
                {
                        StartPoint = new Point(0, 1),
                        EndPoint = new Point(0, 0),
                        GradientStops = new GradientStopCollection
                        {

                        new GradientStop(Color.FromRgb(19, 120, 216), 1),       // blue-bright
                        new GradientStop(Color.FromArgb(0, 19, 120, 216), 0.3),       // blue-bright
                        new GradientStop(Color.FromArgb(0,255, 255, 255), 0)        // blue 
                        }
                }
            }

        };
        YAxisFormatter = value => value.ToString("N0");
        DateTimeFormatter = value => DateTime.Now.ToString("hh:mm:ss");


    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public async Task UpdateAccessPoints(DataIngestEngine dataIngestEngine)
    {
        AccessPoints.Clear();

        for (int i = 0; i < dataIngestEngine.SSID_LIST.Count; i++)
        {
            AccessPoints.Add(new dataModelStructure
            {
                SSID = dataIngestEngine.SSID_LIST[i],
                BSSID = dataIngestEngine.BSSID_LIST[i],
                RSSI = dataIngestEngine.SIGNAL_STRENGTH_LIST[i],
                WLAN_STANDARD = dataIngestEngine.WIFI_STANDARD_LIST[i],
                WIFI_CHANNEL = dataIngestEngine.CHANNEL_LIST[i],
                FREQ_BAND = dataIngestEngine.BAND_LIST[i],
                ENCRYPTION_USED = dataIngestEngine.ENCRYPTION_TYPE_LIST[i],
                FREQUENCY = dataIngestEngine.FREQUENCY_LIST[i],
                AUTHENTICATION = dataIngestEngine.AUTH_LIST[i],
            });

        }
        TOTAL_DETECTED_AP = dataIngestEngine.SSID_LIST.Count;
        TOTAL_SECURE_AP = calculateTotalSecureAccessPoints(dataIngestEngine);
        TOTAL_2_4_GHz_AP = calculateTotal24GHzAccessPoints(dataIngestEngine);
        TOTAL_5_GHz_AP = calculateTotal5GHzAccessPoints(dataIngestEngine);
        IS_LOADING = dataIngestEngine.isLoading;

        if (IS_LOADING == false)
        {
            UpdateFrequencyGraph(TOTAL_2_4_GHz_AP, TOTAL_5_GHz_AP);
            await RefreshChannelAllocationChartAsync(PopulateSSIDInfoList(dataIngestEngine));
        }
    }
    private int calculateTotalSecureAccessPoints(DataIngestEngine dataIngestEngine)
    {

        int secureAccessPointCount = 0;

        foreach (var acessPointEncMethod in dataIngestEngine.ENCRYPTION_TYPE_LIST)
        {
            if (acessPointEncMethod == "None")
            {

            }
            else
            {
                secureAccessPointCount++;
            }
        }

        return secureAccessPointCount;
    }
    private int calculateTotal24GHzAccessPoints(DataIngestEngine dataIngestEngine)
    {
        int total24GHzAPs = 0;

        foreach (var accessPoint in dataIngestEngine.BAND_LIST)
        {
            if (accessPoint == "2.4 GHz")
            {
                total24GHzAPs++;
            }
        }

        return total24GHzAPs;
    }
    private int calculateTotal5GHzAccessPoints(DataIngestEngine dataIngestEngine)
    {
        int total5GHzAPs = 0;
        foreach (var accessPoint in dataIngestEngine.BAND_LIST)
        {
            if (accessPoint == "5 GHz")
            {
                total5GHzAPs++;
            }
        }

        return total5GHzAPs;
    }
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
    private void UpdateFrequencyGraph(int value24GHz, int value5GHz)
    {
        var series24GHz = FrequencySeriesCollection[0];
        var series5GHz = FrequencySeriesCollection[1];

        RunOnUIThread(() =>
        {
            series24GHz.Values.Add(value24GHz);
            series5GHz.Values.Add(value5GHz);

            int maxPoints = 30;
            if (series24GHz.Values.Count > maxPoints)
                series24GHz.Values.RemoveAt(0);

            if (series5GHz.Values.Count > maxPoints)
                series5GHz.Values.RemoveAt(0);
        });
    }

    public async Task RefreshChannelAllocationChartAsync(List<SSIDInfoForCHAllocation> data)
    {

        var channelCount = await Task.Run(() => ChannelAllocationProcessData(data));

        UpdateChannelAllocationChart(channelCount);
    }

    private Dictionary<int, List<double>> ChannelAllocationProcessData(List<SSIDInfoForCHAllocation> data)
    {
        var channelData = new Dictionary<int, List<double>>();

        foreach (var info in data)
        {
            if (!channelData.ContainsKey(info.Channel))
            {
                channelData[info.Channel] = new List<double>();
            }

            // Estimate RSSI value 
            double rssi = (info.SignalStrength / 2) - 100;
            channelData[info.Channel].Add(rssi);
        }

        return channelData;
    }
    private void UpdateChannelAllocationChart(Dictionary<int, List<double>> data)
    {

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => UpdateChannelAllocationChart(data));
            return;
        }

        ChannelAllocationSeries.Clear();

        foreach (var entry in data)
        {
            var channel = entry.Key;
            var signalStrengthsOnChannel = entry.Value;

            foreach (var rssi in signalStrengthsOnChannel)
            {
                var lineSeries = new LiveCharts.Wpf.LineSeries
                {
                    Title = $"2.4GHz Frequency Allocation Graph",
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometrySize = 10,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(Color.FromRgb(66, 255, 192)),
                    Fill = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 1),
                        EndPoint = new Point(0, 0),
                        GradientStops = new GradientStopCollection
                    {

                        new GradientStop(Color.FromRgb(61, 235, 154), 1),       // blue-green
                        new GradientStop(Color.FromArgb(60,110, 204, 37), 0)        // Green 
                    }
                    }
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

                ChannelAllocationSeries.Add(lineSeries);
            }
        }
    }
    private List<SSIDInfoForCHAllocation> PopulateSSIDInfoList(DataIngestEngine dataIngestEngine)
    {
        var ssidInfoList = new List<SSIDInfoForCHAllocation>();

        for (int i = 0; i < dataIngestEngine.SSID_LIST.Count; i++)
        {
            if (dataIngestEngine.BAND_LIST[i] == "2.4 GHz")
            {
                ssidInfoList.Add(new SSIDInfoForCHAllocation
                {
                    SSID = dataIngestEngine.SSID_LIST[i],
                    Channel = dataIngestEngine.CHANNEL_LIST[i],
                    SignalStrength = dataIngestEngine.SIGNAL_STRENGTH_LIST[i]
                });
            }

        }

        return ssidInfoList;
    }
    public void Dispose()
    {

    }

    public class SSIDInfoForCHAllocation
    {
        public string SSID { get; set; }
        public int Channel { get; set; }
        public double SignalStrength { get; set; }
    }

}

