using AAPADS;
using AAPADS.src.engine;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class detectionsViewDataModel : baseDataModel, INotifyPropertyChanged
{
    public ObservableCollection<dataModelStructure> DETECTIONS { get; }

    public detectionsViewDataModel()
    {
        DETECTIONS = new ObservableCollection<dataModelStructure>();
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public void updateDetections(DetectionEngine detectionEngine)
    {
        DETECTIONS.Clear();


        for (int i = 0; i < detectionEngine.CRITICALITY_LEVEL.Count; i++)
        {
            DETECTIONS.Add(new dataModelStructure()
            {
                CRITICAILITY_LEVEL = detectionEngine.CRITICALITY_LEVEL[i],
        } );
            
        }

        //for (int i = 0; i < dataIngestEngine.SSID_LIST.Count; i++)
        //{
        //    AccessPoints.Add(new dataModelStructure
        //    {
        //        SSID = dataIngestEngine.SSID_LIST[i],
        //        BSSID = dataIngestEngine.BSSID_LIST[i],
        //        RSSI = dataIngestEngine.SIGNAL_STRENGTH_LIST[i],
        //        WLAN_STANDARD = dataIngestEngine.WIFI_STANDARD_LIST[i],
        //        WIFI_CHANNEL = dataIngestEngine.CHANNEL_LIST[i],
        //        FREQ_BAND = dataIngestEngine.BAND_LIST[i],
        //        ENCRYPTION_USED = dataIngestEngine.ENCRYPTION_TYPE_LIST[i],
        //        FREQUENCY = dataIngestEngine.FREQUENCY_LIST[i],
        //        AUTHENTICATION = dataIngestEngine.AUTH_LIST[i],
        //    });

        //    TOTAL_DETECTED_AP = dataIngestEngine.SSID_LIST.Count;
        //    // update TOTAL_SECURE_AP and TOTAL_VULNERABLE_AP in a similar way
        //}
    }


}



