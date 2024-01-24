using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace AAPADS
{
    public class detectionSetUpViewDataModel : baseDataModel, INotifyPropertyChanged
    {
        public ObservableCollection<NETWORK_ADAPTER_INFO> NETWORK_80211_ADAPTERS { get; set; }

        private int _detectionTrainingTime;
        public int DetectionTrainingTime
        {
            get { return _detectionTrainingTime; }
            set
            {
                if (_detectionTrainingTime != value)
                {
                    _detectionTrainingTime = value;
                    OnPropertyChanged(nameof(DetectionTrainingTime));
                    SaveDetectionTrainingTimeToDatabase(value);
                }
            }
        }

        public detectionSetUpViewDataModel()
        {
            NETWORK_80211_ADAPTERS = new ObservableCollection<NETWORK_ADAPTER_INFO>();
            NETWORK_ADAPTER_INFO.AdapterCollection = NETWORK_80211_ADAPTERS;
            LoadAdapters();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadAdapters()
        {
            var adapters = GetNetworkAdapters();
            foreach (var adapter in adapters)
            {
                NETWORK_80211_ADAPTERS.Add(adapter);
            }
        }
        private void SaveDetectionTrainingTimeToDatabase(int hours)
        {
            string timeValue = $"{hours:00}:00:00";
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                db.SaveSetting("DetectionTrainingTime", timeValue);
            }
        }

        public List<NETWORK_ADAPTER_INFO> GetNetworkAdapters()
        {
            List<NETWORK_ADAPTER_INFO> adapterList = new List<NETWORK_ADAPTER_INFO>();

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    adapterList.Add(new NETWORK_ADAPTER_INFO
                    {
                        NETWORK_ADAPTER_NAME = adapter.Name,
                        NETWORK_ADAPTER_ID = adapter.Id,
                        NETWORK_ADAPTER_DESCRIPTION = adapter.Description,
                        NETWORK_ADAPTER_STATUS = adapter.OperationalStatus.ToString(),
                        NETWORK_ADAPTER_SPEED_BYTES = adapter.Speed,
                        NETWORK_ADAPTER_MAC_ADDRESS = adapter.GetPhysicalAddress().ToString()
                    });
                }
            }

            return adapterList;
        }
    }
    public class NETWORK_ADAPTER_INFO : INotifyPropertyChanged
    {
        public string NETWORK_ADAPTER_NAME { get; set; }
        public string NETWORK_ADAPTER_ID { get; set; }
        public string NETWORK_ADAPTER_DESCRIPTION { get; set; }
        public string NETWORK_ADAPTER_STATUS { get; set; }
        public long NETWORK_ADAPTER_SPEED_BYTES { get; set; }
        public string NETWORK_ADAPTER_MAC_ADDRESS { get; set; }

        private bool _isAdapterActive;
        public bool IsAdapterActive
        {
            get { return _isAdapterActive; }
            set
            {
                if (_isAdapterActive != value)
                {
                    _isAdapterActive = value;
                    OnPropertyChanged("IsAdapterActive");

                    if (value)
                    {
                        SaveSelectedAdapterSetting(NETWORK_ADAPTER_DESCRIPTION);
                        // Deactivate other adapters in the collection
                        if (AdapterCollection != null)
                        {
                            foreach (var adapter in AdapterCollection)
                            {
                                if (adapter != this)
                                    adapter.IsAdapterActive = false;
                            }
                        }
                    }
                }
            }
        }
        private void SaveSelectedAdapterSetting(string adapterName)
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                db.SaveSetting("DefaultWNICName", adapterName);
            }
        }


        public static ObservableCollection<NETWORK_ADAPTER_INFO> AdapterCollection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
