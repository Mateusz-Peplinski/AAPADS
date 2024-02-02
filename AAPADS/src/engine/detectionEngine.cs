using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public bool IsDetectionComplete { get; private set; } = false;

        private CancellationTokenSource _detectionCts = new CancellationTokenSource();

        public void START_DETECTION_ENGINE()
        {
            // Uncomment if database needs detection data
            // This function will populate the database with sample detection data
            // WriteSQLDataTest();

            IsDetectionComplete = true;
            // when detection is done invoke event so UI can update            
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);

            Task.Run(async () => await StartDetection(_detectionCts.Token));
        }
        private void WriteSQLDataTest()
        {
            var detectionEvent = new DetectionEvent
            {
                CriticalityLevel = "LEVEL_5",
                RiskLevel = 95,
                DetectionStatus = "Active",
                DetectionTime = DateTime.Now.ToString("dd:MMM:yyyy [ HH:mm:ss ]"),
                DetectionTitle = "Unauthorized Access Detected",
                DetectionDescription = "An unauthorized device has been detected attempting to access the network.",
                DetectionRemediation = "Investigate the device and take appropriate security measures.",
                DetectionAccessPointSsid = "ExampleSSID",
                DetectionAccessPointMacAddress = "00:1A:2B:3C:4D:5E",
                DetectionAccessPointSignalStrength = "-50 dBm",
                DetectionAccessPointOpenChannel = "6",
                DetectionAccessPointFrequency = "2.412 GHz",
                DetectionAccessPointIsStillActive = "Yes",
                DetectionAccessPointTimeFirstDetected = "14:20:15 [Date: 01 Jan 2024]",
                DetectionAccessPointEncryption = "WPA2",
                DetectionAccessPointConnectedClients = "12"
            };
            using (var db = new DetectionEngineDatabaseAccess("wireless_profile.db"))
            {
                db.SaveDetectionData(detectionEvent);
            }
        }

        private async Task StartDetection(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[ DETECTION ENGINE ] Hello from inside detection thread");

                    //Delay just to prevent tight loop and high CPU usage
                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DETECTION ENGINE ] Detection engine was cancelled.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ DETECTION ENGINE ] An error occurred: {ex.Message}");
            }
        }
        public void StopDetectionEngine()
        {
            _detectionCts.Cancel();
        }
    }
}
