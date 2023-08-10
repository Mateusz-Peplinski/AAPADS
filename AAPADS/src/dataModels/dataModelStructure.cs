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
        private int _TOTAL_SECURE_AP;    
        private int _TOTAL_2_4_GHz_AP;    
        private int _TOTAL_5_GHz_AP;    
        private string _CRITICAILITY_LEVEL;   
        private int _RISK_LEVEL;
        private string _DETECTION_STATUS;
        private string _DETECTION_TIME;
        private string _DETECTION_TITLE;
        private string _DETECTION_DESCRIPTION;
        private string _DETECTION_REMEDIATION;
        private string _DETECTION_ACCESS_POINT_SSID;
        private string _DETECTION_ACCESS_POINT_MAC_ADDRESS;
        private string _DETECTION_ACCESS_POINT_SIGNAL_STRENGTH;
        private string _DETECTION_ACCESS_POINT_OPEN_CHANNEL;
        private string _DETECTION_ACCESS_POINT_FREQUENCY;
        private string _DETECTION_ACCESS_POINT_IS_STILL_ACTIVE;
        private string _DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED;
        private string _DETECTION_ACCESS_POINT_ENCRYPTION;
        private string _DETECTION_ACCESS_POINT_CONNECTED_CLIENTS;

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
        public int TOTAL_SECURE_AP
        {
            get => _TOTAL_SECURE_AP;
            set => SetProperty(ref _TOTAL_SECURE_AP, value);
        }
        public int TOTAL_2_4_GHz_AP
        {
            get => _TOTAL_2_4_GHz_AP;
            set => SetProperty(ref _TOTAL_2_4_GHz_AP, value);
        }
        public int TOTAL_5_GHz_AP
        {
            get => _TOTAL_5_GHz_AP;
            set => SetProperty(ref _TOTAL_5_GHz_AP, value);
        }
        public string CRITICAILITY_LEVEL
        {
            get => _CRITICAILITY_LEVEL;
            set => SetProperty(ref _CRITICAILITY_LEVEL, value);
        }
        public int RISK_LEVEL
        {
            get => _RISK_LEVEL;
            set => SetProperty(ref _RISK_LEVEL, value);
        }
        public string DETECTION_STATUS
        {
            get => _DETECTION_STATUS;
            set => SetProperty(ref _DETECTION_STATUS, value);
        }
        public string DETECTION_TIME
        {
            get => _DETECTION_TIME;
            set => SetProperty(ref _DETECTION_TIME, value);
        }

        public string DETECTION_TITLE
        {
            get => _DETECTION_TITLE;
            set => SetProperty(ref _DETECTION_TITLE, value);
        }

        public string DETECTION_DESCRIPTION
        {
            get => _DETECTION_DESCRIPTION;
            set => SetProperty(ref _DETECTION_DESCRIPTION, value);
        }

        public string DETECTION_REMEDIATION
        {
            get => _DETECTION_REMEDIATION;
            set => SetProperty(ref _DETECTION_REMEDIATION, value);
        }

        public string DETECTION_ACCESS_POINT_SSID
        {
            get => _DETECTION_ACCESS_POINT_SSID;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_SSID, value);
        }

        public string DETECTION_ACCESS_POINT_MAC_ADDRESS
        {
            get => _DETECTION_ACCESS_POINT_MAC_ADDRESS;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_MAC_ADDRESS, value);
        }

        public string DETECTION_ACCESS_POINT_SIGNAL_STRENGTH
        {
            get => _DETECTION_ACCESS_POINT_SIGNAL_STRENGTH;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_SIGNAL_STRENGTH, value);
        }

        public string DETECTION_ACCESS_POINT_OPEN_CHANNEL
        {
            get => _DETECTION_ACCESS_POINT_OPEN_CHANNEL;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_OPEN_CHANNEL, value);
        }

        public string DETECTION_ACCESS_POINT_FREQUENCY
        {
            get => _DETECTION_ACCESS_POINT_FREQUENCY;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_FREQUENCY, value);
        }

        public string DETECTION_ACCESS_POINT_IS_STILL_ACTIVE
        {
            get => _DETECTION_ACCESS_POINT_IS_STILL_ACTIVE;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_IS_STILL_ACTIVE, value);
        }

        public string DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED
        {
            get => _DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED, value);
        }

        public string DETECTION_ACCESS_POINT_ENCRYPTION
        {
            get => _DETECTION_ACCESS_POINT_ENCRYPTION;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_ENCRYPTION, value);
        }

        public string DETECTION_ACCESS_POINT_CONNECTED_CLIENTS
        {
            get => _DETECTION_ACCESS_POINT_CONNECTED_CLIENTS;
            set => SetProperty(ref _DETECTION_ACCESS_POINT_CONNECTED_CLIENTS, value);
        }

    }
}
