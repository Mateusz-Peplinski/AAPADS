using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AAPADS
{
    public class dataModelStructure : baseDataModel
    {
        private bool _isSelected;
        private string _SSID;
        private int _RSSI;
        private string _BSSID;
        private string _WLAN_STANDARD;
        private int _WIFI_CHANNEL;
        private string _FREQ_BAND;
        private string _ENCRYPTION_USED; 
        private string _FREQUENCY;  
        private string _AUTHENTICATION_USED;  
        private int _TOTAL_DETECTED_AP;    

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string BSSID
        {
            get => _BSSID;
            set => SetProperty(ref _BSSID, value);
        }

        public string SSID
        {
            get => _SSID;
            set => SetProperty(ref _SSID, value);
        }

        public int RSSI
        {
            get => _RSSI;
            set => SetProperty(ref _RSSI, value);
        }

        public string WLAN_STANDARD
        {
            get => _WLAN_STANDARD;
            set => SetProperty(ref _WLAN_STANDARD, value);
        }

        public int WIFI_CHANNEL
        {
            get => _WIFI_CHANNEL;
            set => SetProperty(ref _WIFI_CHANNEL, value);
        }

        public string FREQ_BAND
        {
            get => _FREQ_BAND;
            set => SetProperty(ref _FREQ_BAND, value);
        }

        public string ENCRYPTION_USED
        {
            get => _ENCRYPTION_USED;
            set => SetProperty(ref _ENCRYPTION_USED, value);
        }

        public string FREQUENCY
        {
            get => _FREQUENCY;
            set => SetProperty(ref _FREQUENCY, value);
        }
        public string AUTHENTICATION
        {
            get => _AUTHENTICATION_USED;
            set => SetProperty(ref _AUTHENTICATION_USED, value);
        }

        public int TOTAL_DETECTED_AP
        {
            get => _TOTAL_DETECTED_AP;
            set => SetProperty(ref _TOTAL_DETECTED_AP, value);
        }   
    }
}
