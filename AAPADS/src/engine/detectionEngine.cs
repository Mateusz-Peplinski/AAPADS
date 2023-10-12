using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS.src.engine
{
    public class DetectionEngine
    {
        public List<string> CRITICALITY_LEVEL = new List<string>();
        public List<int> RISK_LEVEL = new List<int>();
        public List<string> DETECTION_STATUS = new List<string>();
        public List<string> DETECTION_TIME = new List<string>();
        public List<string> DETECTION_TITLE = new List<string>();
        public List<string> DETECTION_DESCRIPTION = new List<string>();
        public List<string> DETECTION_REMEDIATION = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_SSID = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_MAC_ADDRESS = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_SIGNAL_STRENGTH = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_OPEN_CHANNEL = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_FREQUENCY = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_IS_STILL_ACTIVE = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_ENCRYPTION = new List<string>();
        public List<string> DETECTION_ACCESS_POINT_CONNECTED_CLIENTS = new List<string>();
        
        public event EventHandler DetectionDiscovered;

        public void START_DETECTION_ENGINE()
        {

            populateDetectionViewModelStaticDataTest();

            // when detection is done invoke event so UI can update            
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);
        }

        private void populateDetectionViewModelStaticDataTest()
        {
            CRITICALITY_LEVEL.Add("LEVEL_5"); // CIRITICAL (RISK SCORE: 80 - 100)
            CRITICALITY_LEVEL.Add("LEVEL_4"); // HIGH (RISK SCORE: 60 - 80)
            CRITICALITY_LEVEL.Add("LEVEL_3"); // MEDIUM (RISK SCORE: 40 - 60)
            CRITICALITY_LEVEL.Add("LEVEL_2"); // LOW (RISK SCORE: 20 - 40)
            CRITICALITY_LEVEL.Add("LEVEL_1"); // INFO (RISK SCORE: 0 - 20)

            RISK_LEVEL.Add(95);
            RISK_LEVEL.Add(70);
            RISK_LEVEL.Add(57);
            RISK_LEVEL.Add(22);
            RISK_LEVEL.Add(15);

            DETECTION_STATUS.Add("NEW");
            DETECTION_STATUS.Add("IN-PROGRESS");
            DETECTION_STATUS.Add("CLOSED");
            DETECTION_STATUS.Add("CLOSED");
            DETECTION_STATUS.Add("CLOSED");

            DETECTION_TIME.Add(DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"));
            DETECTION_TIME.Add(DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"));
            DETECTION_TIME.Add(DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"));
            DETECTION_TIME.Add(DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"));
            DETECTION_TIME.Add(DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"));

            WirelessAttackDatabase detectionDictionaryData = new WirelessAttackDatabase();

            foreach (var attack in detectionDictionaryData.wirelessAttacks)
            {
                DETECTION_TITLE.Add(attack.Key); 
                DETECTION_DESCRIPTION.Add(attack.Value.Description); 
                DETECTION_REMEDIATION.Add(attack.Value.Remediation); 
            }
            for (int i = 0; i < 5; i++)
            {
                DETECTION_ACCESS_POINT_SSID.Add($"SSID-{i}");
            }

            
            DETECTION_ACCESS_POINT_MAC_ADDRESS.Add("00:0a:95:9d:68:16");
            DETECTION_ACCESS_POINT_MAC_ADDRESS.Add("00:0a:95:9d:68:17");
            DETECTION_ACCESS_POINT_MAC_ADDRESS.Add("00:0a:95:9d:68:18");
            DETECTION_ACCESS_POINT_MAC_ADDRESS.Add("00:0a:95:9d:68:19");
            DETECTION_ACCESS_POINT_MAC_ADDRESS.Add("00:0a:95:9d:68:20");

           
            DETECTION_ACCESS_POINT_SIGNAL_STRENGTH.Add("-45 dBm");
            DETECTION_ACCESS_POINT_SIGNAL_STRENGTH.Add("-60 dBm");
            DETECTION_ACCESS_POINT_SIGNAL_STRENGTH.Add("-30 dBm");
            DETECTION_ACCESS_POINT_SIGNAL_STRENGTH.Add("-50 dBm");
            DETECTION_ACCESS_POINT_SIGNAL_STRENGTH.Add("-55 dBm");

            
            DETECTION_ACCESS_POINT_OPEN_CHANNEL.Add("6");
            DETECTION_ACCESS_POINT_OPEN_CHANNEL.Add("11");
            DETECTION_ACCESS_POINT_OPEN_CHANNEL.Add("1");
            DETECTION_ACCESS_POINT_OPEN_CHANNEL.Add("9");
            DETECTION_ACCESS_POINT_OPEN_CHANNEL.Add("3");

            
            DETECTION_ACCESS_POINT_FREQUENCY.Add("2.412 GHz");
            DETECTION_ACCESS_POINT_FREQUENCY.Add("2.462 GHz");
            DETECTION_ACCESS_POINT_FREQUENCY.Add("2.417 GHz");
            DETECTION_ACCESS_POINT_FREQUENCY.Add("2.452 GHz");
            DETECTION_ACCESS_POINT_FREQUENCY.Add("2.422 GHz");

            
            DETECTION_ACCESS_POINT_IS_STILL_ACTIVE.Add("Yes");
            DETECTION_ACCESS_POINT_IS_STILL_ACTIVE.Add("No");
            DETECTION_ACCESS_POINT_IS_STILL_ACTIVE.Add("Yes");
            DETECTION_ACCESS_POINT_IS_STILL_ACTIVE.Add("Yes");
            DETECTION_ACCESS_POINT_IS_STILL_ACTIVE.Add("No");

            
            DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED.Add("14:20:15 [Date: 15th July 2023]");
            DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED.Add("10:25:05 [Date: 15th July 2023]");
            DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED.Add("11:30:45 [Date: 15th July 2023]");
            DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED.Add("16:45:10 [Date: 15th July 2023]");
            DETECTION_ACCESS_POINT_TIME_FIRST_DETECTED.Add("17:10:50 [Date: 15th July 2023]");

            
            DETECTION_ACCESS_POINT_ENCRYPTION.Add("WPA2");
            DETECTION_ACCESS_POINT_ENCRYPTION.Add("WEP");
            DETECTION_ACCESS_POINT_ENCRYPTION.Add("WPA3");
            DETECTION_ACCESS_POINT_ENCRYPTION.Add("Open");
            DETECTION_ACCESS_POINT_ENCRYPTION.Add("WPA2");

            
            DETECTION_ACCESS_POINT_CONNECTED_CLIENTS.Add("5");
            DETECTION_ACCESS_POINT_CONNECTED_CLIENTS.Add("2");
            DETECTION_ACCESS_POINT_CONNECTED_CLIENTS.Add("7");
            DETECTION_ACCESS_POINT_CONNECTED_CLIENTS.Add("3");
            DETECTION_ACCESS_POINT_CONNECTED_CLIENTS.Add("0");

        }

    }
}
