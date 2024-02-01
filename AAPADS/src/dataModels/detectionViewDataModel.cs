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
    public void updateDetections()
    {
        DETECTIONS.Clear();

        using (var databaseAccess = new DetectionEngineDatabaseAccess("wireless_profile.db"))
        {
            var detectionEvents = databaseAccess.FetchAllDetectionData();
            foreach (var detectionEvent in detectionEvents)
            {
                DETECTIONS.Add(new dataModelStructure()
                {
                    CRITICAILITY_LEVEL = detectionEvent.CriticalityLevel,
                    RISK_LEVEL = detectionEvent.RiskLevel,
                    DETECTION_STATUS = detectionEvent.DetectionStatus,
                    DETECTION_TIME = detectionEvent.DetectionTime,
                    DETECTION_TITLE = detectionEvent.DetectionTitle,
                    DETECTION_DESCRIPTION = detectionEvent.DetectionDescription,
                    DETECTION_REMEDIATION = detectionEvent.DetectionRemediation,
                    DETECTION_ACCESS_POINT_SSID = detectionEvent.DetectionAccessPointSsid,
                    DETECTION_ACCESS_POINT_MAC_ADDRESS = detectionEvent.DetectionAccessPointMacAddress,
                    DETECTION_ACCESS_POINT_SIGNAL_STRENGTH = detectionEvent.DetectionAccessPointSignalStrength,
                    DETECTION_ACCESS_POINT_OPEN_CHANNEL = detectionEvent.DetectionAccessPointOpenChannel,
                    DETECTION_ACCESS_POINT_FREQUENCY = detectionEvent.DetectionAccessPointFrequency,
                    DETECTION_ACCESS_POINT_IS_STILL_ACTIVE = detectionEvent.DetectionAccessPointIsStillActive,
                    DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED = detectionEvent.DetectionAccessPointTimeFirstDetected,
                    DETECTION_ACCESS_POINT_ENCRYPTION = detectionEvent.DetectionAccessPointEncryption,
                    DETECTION_ACCESS_POINT_CONNECTED_CLIENTS = detectionEvent.DetectionAccessPointConnectedClients,
                });
            }
        }
    }


}



