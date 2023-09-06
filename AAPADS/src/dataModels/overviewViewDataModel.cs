using AAPADS;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Linq;
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
    public SeriesCollection ChannelAllocationSeries5GHz { get; set; } = new SeriesCollection();
    public SeriesCollection ChannelAllocationSeries24GHz { get; set; } = new SeriesCollection();

    public Func<double, string> YAxisFormatter { get; set; }

    public Func<double, string> DateTimeFormatter { get; set; }

    private int _totalDetectedAP;
    private int _totalSecureAP;
    private int _total24GHzNetworks;
    private int _total5GHzNetworks;
    private bool _isLoading;
    private double _AvgSignalStrenght;
    private double _summarySectionHeight = 400;
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
    public double AVG_SIGNAL_STRENGTH
    {
        get { return _AvgSignalStrenght; }
        set
        {
            if (_AvgSignalStrenght != value)
            {
                _AvgSignalStrenght = value;
                OnPropertyChanged(nameof(AVG_SIGNAL_STRENGTH));
            }
        }
    }

    public overviewViewDataModel()
    {
        AccessPoints = new ObservableCollection<dataModelStructure>();

        FrequencySeriesCollection = new SeriesCollection
        {

            new LineSeries
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

            new LineSeries
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
        AVG_SIGNAL_STRENGTH = CalculateAverageSignalStrength(dataIngestEngine);
        IS_LOADING = dataIngestEngine.isLoading;
        if (IS_LOADING == false)
        {
            UpdateFrequencyGraph(TOTAL_2_4_GHz_AP, TOTAL_5_GHz_AP);
            var (data24GHz, data5GHz) = PopulateSSIDInfoList(dataIngestEngine);
            await RefreshChannelAllocationChartsAsync(data24GHz, data5GHz);

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
    private double CalculateAverageSignalStrength(DataIngestEngine dataIngestEngine)
    {
        double averageSignalStrength = 0;
        for (int i = 0; i < dataIngestEngine.SSID_LIST.Count; i++)
        {
            averageSignalStrength += dataIngestEngine.SIGNAL_STRENGTH_LIST[i];
        }
        averageSignalStrength = averageSignalStrength / dataIngestEngine.SSID_LIST.Count;

        return averageSignalStrength;
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

    public async Task RefreshChannelAllocationChartsAsync(List<SSIDInfoForCHAllocation24GHz> data24GHz, List<SSIDInfoForCHAllocation5GHz> data5GHz)
    {
        var channelCount24GHz = await Task.Run(() => ChannelAllocationProcessData24GHz(data24GHz));
        UpdateChannelAllocationChart24GHz(channelCount24GHz);

        var channelCount5GHz = await Task.Run(() => ChannelAllocationProcessData5GHz(data5GHz));
        UpdateChannelAllocationChart5GHz(channelCount5GHz);
    }
    private Dictionary<int, List<double>> ChannelAllocationProcessData5GHz(List<SSIDInfoForCHAllocation5GHz> data)
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
    private void UpdateChannelAllocationChart5GHz(Dictionary<int, List<double>> data)
    {

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => UpdateChannelAllocationChart5GHz(data));
            return;
        }

        ChannelAllocationSeries5GHz.Clear();

        foreach (var entry in data)
        {
            var channel = entry.Key;
            var signalStrengthsOnChannel = entry.Value;
            string channelToolTip = channel.ToString();

            foreach (var rssi in signalStrengthsOnChannel)
            {
                var lineSeries = new LineSeries
                {
                    Title = $"5GHz Channel:",
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
                    },
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
                        return channel.ToString(); 
                    }
                    return ""; 
                };


                ChannelAllocationSeries5GHz.Add(lineSeries);
            }
        }
    }

    private Dictionary<int, List<double>> ChannelAllocationProcessData24GHz(List<SSIDInfoForCHAllocation24GHz> data)
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
    private void UpdateChannelAllocationChart24GHz(Dictionary<int, List<double>> data)
    {

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => UpdateChannelAllocationChart24GHz(data));
            return;
        }

        ChannelAllocationSeries24GHz.Clear();

        foreach (var entry in data)
        {
            var channel = entry.Key;
            var signalStrengthsOnChannel = entry.Value;
            string channelToolTip = channel.ToString();

            foreach (var rssi in signalStrengthsOnChannel)
            {
                var lineSeries = new LiveCharts.Wpf.LineSeries
                {
                    Title = $"2.4GHz Channel:",
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
                            new GradientStop(Color.FromArgb(60, 110, 204, 37), 0)        // Green 
                        }
                    },
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
                        return channel.ToString(); 
                    }
                    return ""; 
                };
                ChannelAllocationSeries24GHz.Add(lineSeries);
            }
        }
    }
    private (List<SSIDInfoForCHAllocation24GHz>, List<SSIDInfoForCHAllocation5GHz>) PopulateSSIDInfoList(DataIngestEngine dataIngestEngine)
    {
        var ssidInfoList24Ghz = new List<SSIDInfoForCHAllocation24GHz>();
        var ssidInfoList5Ghz = new List<SSIDInfoForCHAllocation5GHz>();

        for (int i = 0; i < dataIngestEngine.SSID_LIST.Count; i++)
        {
            if (dataIngestEngine.BAND_LIST[i] == "2.4 GHz")
            {
                ssidInfoList24Ghz.Add(new SSIDInfoForCHAllocation24GHz
                {
                    SSID = dataIngestEngine.SSID_LIST[i],
                    Channel = dataIngestEngine.CHANNEL_LIST[i],
                    SignalStrength = dataIngestEngine.SIGNAL_STRENGTH_LIST[i]
                });
            }
            else if (dataIngestEngine.BAND_LIST[i] == "5 GHz")
            {
                ssidInfoList5Ghz.Add(new SSIDInfoForCHAllocation5GHz
                {
                    SSID = dataIngestEngine.SSID_LIST[i],
                    Channel = dataIngestEngine.CHANNEL_LIST[i],
                    SignalStrength = dataIngestEngine.SIGNAL_STRENGTH_LIST[i]
                });
            }
        }

        return (ssidInfoList24Ghz, ssidInfoList5Ghz);
    }

    public void Dispose()
    {

    }

    public class SSIDInfoForCHAllocation24GHz
    {
        public string SSID { get; set; }
        public int Channel { get; set; }
        public double SignalStrength { get; set; }
    }
    public class SSIDInfoForCHAllocation5GHz
    {
        public string SSID { get; set; }
        public int Channel { get; set; }
        public double SignalStrength { get; set; }
    }
}

