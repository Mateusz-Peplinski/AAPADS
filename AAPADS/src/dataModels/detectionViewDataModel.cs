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
                RISK_LEVEL = detectionEngine.RISK_LEVEL[i],
                DETECTION_STATUS = detectionEngine.DETECTION_STATUS[i], 
                DETECTION_TIME = detectionEngine.DETECTION_TIME[i],
                DETECTION_TITLE = detectionEngine.DETECTION_TITLE[i],
                DETECTION_DESCRIPTION = detectionEngine.DETECTION_DESCRIPTION[i],   
                DETECTION_REMEDIATION = detectionEngine.DETECTION_REMEDIATION[i],   
                DETECTION_ACCESS_POINT_SSID = detectionEngine.DETECTION_ACCESS_POINT_SSID[i],
                DETECTION_ACCESS_POINT_MAC_ADDRESS = detectionEngine.DETECTION_ACCESS_POINT_MAC_ADDRESS[i],
                DETECTION_ACCESS_POINT_SIGNAL_STRENGTH = detectionEngine.DETECTION_ACCESS_POINT_SIGNAL_STRENGTH[i],
                DETECTION_ACCESS_POINT_OPEN_CHANNEL = detectionEngine.DETECTION_ACCESS_POINT_OPEN_CHANNEL[i],
                DETECTION_ACCESS_POINT_FREQUENCY = detectionEngine.DETECTION_ACCESS_POINT_FREQUENCY[i],
                DETECTION_ACCESS_POINT_IS_STILL_ACTIVE = detectionEngine.DETECTION_ACCESS_POINT_IS_STILL_ACTIVE[i],
                DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED = detectionEngine.DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED[i],
                DETECTION_ACCESS_POINT_ENCRYPTION = detectionEngine.DETECTION_ACCESS_POINT_ENCRYPTION[i],
                DETECTION_ACCESS_POINT_CONNECTED_CLIENTS = detectionEngine.DETECTION_ACCESS_POINT_CONNECTED_CLIENTS[i],
        } );
            
        }
    }


}



