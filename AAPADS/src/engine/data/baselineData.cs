using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    internal class baselineData
    {

        public string[] BSSID = new string[12] { "C3:D3:9C:C4:00:B9", "A5:CC:C0:C8:8F:E1", "1E:5B:88:9C:A0:E7", "6C:54:30:E3:76:D5", "E4:A9:5B:1D:40:D4", "28:FC:73:7B:46:25", "C3:D3:9C:C4:00:B9", "A5:CC:C0:C8:8F:E1", "1E:5B:88:9C:A0:E7", "6C:54:30:E3:76:D5", "E4:A9:5B:1D:40:D4", "28:FC:73:7B:46:25" };
        public string[] SSID = new string[12] { "DLINK-55433", "TALKTALK-7765", "BT-8787", "CISCO-ALPHA", "CISCO-BETA", "AT&T_0900", "DLINK-55433", "TALKTALK-7765", "BT-8787", "CISCO-ALPHA", "CISCO-BETA", "AT&T_0900" };
        public int[] SIGNAL_STRENGTH_RSSI_dBm = new int[12] { 30, 30, 0, 86, 60, 90, 38, 10, 0, 80, 67, 90 };
        public string[] SIGNAL_STRENGTH_RANGE = new string[12] { "EXCELLENT", "GOOD", "EXCELLENT", "POOR", "FAIR", "VERY POOR", "EXCELLENT", "EXCELLENT", "EXCELLENT", "POOR", "FAIR", "VERY POOR" };
        public string[] WIFI_CHANNEL = new string[12] { "1", "6", "11", "1", "6", "11", "1", "6", "11", "1", "6", "11" };
        public string[] FREQUENCY = new string[12] { "2.412 GHz ", "2.437 GHz ", "2.462 GHz ", "2.412 GHz ", "2.437 GHz ", "2.462 GHz ", "2.412 GHz ", "2.437 GHz ", "2.462 GHz ", "2.412 GHz ", "2.437 GHz ", "2.462 GHz " };
        public string[] ENCRYPTION_METHOD = new string[12] { "WEP", "WPA", "WPA2", "AES", "TKIP", "PSK", "EAP", "CCMP", "WPS", "LEAP", "PEAP", "TTLS" };
    }
}
