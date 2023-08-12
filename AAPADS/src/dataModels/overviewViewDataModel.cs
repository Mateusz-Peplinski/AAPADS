using AAPADS;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;



public class overviewViewDataModel : baseDataModel, INotifyPropertyChanged
{
    public ObservableCollection<dataModelStructure> AccessPoints { get; }

    public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection
{
    new LineSeries
    {
        Title = "2.4GHz",
        Values = new ChartValues<double>()
    },
    new LineSeries
    {
        Title = "5GHz",
        Values = new ChartValues<double>()
    }
};

    public Func<double, string> DateTimeFormatter { get; set; }

    private int _totalDetectedAP;
    private int _totalSecureAP;
    private int _total24GHzNetworks;
    private int _total5GHzNetworks;
    private bool _isLoading;
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
    private string _liveLog;
    public string LiveLog
    {
        get { return _liveLog; }
        set
        {
            if (_liveLog != value)
            {
                _liveLog = value;
                OnPropertyChanged("LiveLog");
            }
        }
    }
    public overviewViewDataModel()
    {
        AccessPoints = new ObservableCollection<dataModelStructure>();

        SeriesCollection = new SeriesCollection
        {
           
            new LineSeries
            {
                Title = "2.4GHz",
                Values = new ChartValues<double>(),
                PointGeometrySize = 18,
                Stroke = Brushes.YellowGreen,
                Fill = new SolidColorBrush(Color.FromArgb(128, 124,252,0)),
                PointForeground = Brushes.YellowGreen,
                StrokeThickness = 0
            },
            
            new LineSeries
            {
                Title = "5GHz",
                Values = new ChartValues<double>(),
                PointGeometrySize = 18,
                Stroke = Brushes.SkyBlue,
                Fill = new SolidColorBrush(Color.FromArgb(128, 30,144,255)), 
                PointForeground = Brushes.SkyBlue,
                StrokeThickness = 0
            }

        };


        DateTimeFormatter = value => DateTime.Now.ToString("hh:mm:ss");

    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateAccessPoints(DataIngestEngine dataIngestEngine)
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
        if (IS_LOADING == false) { UpdateGraph(TOTAL_2_4_GHz_AP, TOTAL_5_GHz_AP); }


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
    public void AppendToLog(string message)
    {
        if (string.IsNullOrEmpty(LiveLog))
        {
            LiveLog = message;
        }
        else
        {
            LiveLog += "\n" + message;
        }

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

    private void UpdateGraph(double value24GHz, double value5GHz)
    {
        var series24GHz = SeriesCollection[0];
        var series5GHz = SeriesCollection[1];

        RunOnUIThread(() =>
        {
            series24GHz.Values.Add(value24GHz);
            series5GHz.Values.Add(value5GHz);

            int maxPoints = 30; // Assuming 5 minutes/10 seconds = 30 points
            if (series24GHz.Values.Count > maxPoints)
                series24GHz.Values.RemoveAt(0);

            if (series5GHz.Values.Count > maxPoints)
                series5GHz.Values.RemoveAt(0);
        });
    }


}

