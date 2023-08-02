using AAPADS;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class overviewViewDataModel : baseDataModel, INotifyPropertyChanged
{
    public ObservableCollection<dataModelStructure> AccessPoints { get; }

    private int _totalDetectedAp;
    public int TOTAL_DETECTED_AP
    {
        get { return _totalDetectedAp; }
        set
        {
            if (_totalDetectedAp != value)
            {
                _totalDetectedAp = value;
                OnPropertyChanged(nameof(TOTAL_DETECTED_AP));
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

            TOTAL_DETECTED_AP = dataIngestEngine.SSID_LIST.Count;
            // update TOTAL_SECURE_AP and TOTAL_VULNERABLE_AP in a similar way
        }
    }
}

