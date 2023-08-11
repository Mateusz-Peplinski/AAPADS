using AAPADS;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class overviewViewDataModel : baseDataModel, INotifyPropertyChanged
{
    public ObservableCollection<dataModelStructure> AccessPoints { get; }

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
    public overviewViewDataModel()
    {
        AccessPoints = new ObservableCollection<dataModelStructure>();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateAccessPoints(DataIngestEngine dataIngestEngine)
    {
        AccessPoints.Clear();
        //List<int> signalIndex = new List<int>();
        //bool sorted = false;
        //while (!sorted)
        //{
        //    sorted = true;
        //    for (int i = 0; i < dataIngestEngine.SSID_LIST.Count - 1; i++)
        //    {
        //        if (dataIngestEngine.SIGNAL_STRENGTH_LIST[i] < dataIngestEngine.SIGNAL_STRENGTH_LIST[i+1])
        //        {
        //            signalIndex[i] = i+1;
        //            signalIndex[i + 1] = i;
        //        }
        //    }
        //}
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
}

