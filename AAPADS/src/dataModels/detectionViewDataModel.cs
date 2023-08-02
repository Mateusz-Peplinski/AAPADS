using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;


namespace AAPADS
{
    public class detectionsViewDataModel : baseDataModel
    {
        public detectionsViewDataModel()
        {

            DataIngestEngine DIE = new DataIngestEngine();

            //DETECTIONS = CreateData(DIE.BSSID);


            foreach (var model in DETECTIONS)
            {
                model.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(dataModelStructure.IsSelected))
                        OnPropertyChanged(nameof(IsAllItems1Selected));
                };
            }

        }
        public bool? IsAllItems1Selected
        {
            get
            {
                var selected = DETECTIONS.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    SelectAll(value.Value, DETECTIONS);
                    OnPropertyChanged();
                }
            }
        }

        private static void SelectAll(bool select, IEnumerable<dataModelStructure> models)
        {
            foreach (var model in models)
            {
                model.IsSelected = select;
            }
        }
        private static ObservableCollection<dataModelStructure> CreateData(string[] BSSID)
        {
            ObservableCollection<dataModelStructure> data = new ObservableCollection<dataModelStructure>();

            for (int i = 0; i < BSSID.Length; i++)
            {
                dataModelStructure model = new dataModelStructure
                {
                    BSSID = BSSID[i],

                };
                
                data.Add(model);
            }

            return data;
        }
        public ObservableCollection<dataModelStructure> DETECTIONS { get; }

    }

    
}
