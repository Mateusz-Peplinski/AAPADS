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
    }


}



