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
        


        // public event to trigger when a new detection is found. 
        public event EventHandler DetectionDiscovered;

        public void startdetection()
        {

            populateDetectionViewModelStaticDataTest();

            // trigger view model update in MainWindow.xaml.cs
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

            detectionDictionary detectionDictionaryData = new detectionDictionary();

            foreach (var attack in detectionDictionaryData.wirelessAttacks)
            {
                DETECTION_TITLE.Add($"Title: {attack.Key}");
            }

            foreach (var attack in detectionDictionaryData.wirelessAttacks)
            {
                DETECTION_DESCRIPTION.Add($"Title: {attack.Key}\nDescription: {attack.Value}\n");
            }
            

        }

    }
}
