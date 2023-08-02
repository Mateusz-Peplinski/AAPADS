using AAPADS;
using System.Collections.Generic;
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
            });

            TOTAL_DETECTED_AP = dataIngestEngine.SSID_LIST.Count;
            // update TOTAL_SECURE_AP and TOTAL_VULNERABLE_AP in a similar way
        }
    }
}

