using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using PacketDotNet.Utils;
using SharpPcap.LibPcap;

namespace AAPADS
{
    public class DataIngestEngineDot11Frames
    {
        private bool _stopCapturing;

        public DataIngestEngineDot11Frames()
        {
            StartCaptureAsync();
        }
        //ISSUES: 
        // Need to convert friendly name to adpatername
        // the thread locks the program unless ctrl-c is pressed
        // does not seem to capture packets


        public void StartCaptureAsync()
        {
            Task.Run(() => StartCapture());
        }

        private void StartCapture()
        {
            Console.CancelKeyPress += HandleCancelKeyPress;

            // Retrieve the device list
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ 802.11 CAPTURE ENGINE ] No devices were found on this machine");
                return;
            }
            int i = 0;

            //Print all the devices that sharpcap detects
            //foreach (var dev in devices)
            //{
            //    Console.WriteLine($"{i} - {dev}");
            //    i++;
            //}

            String MonitorModeAdapterName = FetchMonitorModeWNICDescription();

            var device = devices.FirstOrDefault(d => d.Description.Contains(MonitorModeAdapterName));
            if (device == null)
            {
                Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Device '{MonitorModeAdapterName}' not found on this machine");
                return;
            }

            // Open the device for capturing
            device.Open(DeviceModes.None, 1000); // Open device with a read timeout of 1000 milliseconds

            Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Listening on {device.Description}, hit 'ctrl-c' to stop...");

            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            device.StartCapture();

            while (!_stopCapturing)
            {
                // Keep the application running until Ctrl+C is pressed
                Task.Delay(1000).Wait();
            }

            device.StopCapture();
            device.Close();
            Console.WriteLine("[ 802.11 CAPTURE ENGINE ] Capture stopped");
        }


        private static void Device_OnPacketArrival(object s, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            Console.WriteLine("packet");
            // Assuming the base packet is an 802.11 packet since we are in monitor mode
            var wifiPacket = packet as PacketDotNet.Ieee80211.MacFrame;

            if (wifiPacket != null)
            {
                // Process all the 802.11 packets here
                switch (wifiPacket.FrameControl.SubType)
                {
                    case PacketDotNet.Ieee80211.FrameControlField.FrameSubTypes.ManagementBeacon:
                        // Process Beacon Frame
                        break;
                    case PacketDotNet.Ieee80211.FrameControlField.FrameSubTypes.ManagementAssociationRequest:
                        // Process Association Request Frame
                        break;
                    case PacketDotNet.Ieee80211.FrameControlField.FrameSubTypes.ManagementAssociationResponse:
                        // Process Association Response Frame
                        break;
                      
                }
            }
        }
        private string FetchMonitorModeWNICDescription()
        {
            string defaultMonitorModeWNICName = "MONITOR MODE ADAPTER NOT SET"; // Default value if the setting does not exist
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Attempt to fetch the setting from the database
                string monitorModeWNICName = db.GetSetting("MonitorModeWNICDescription");

                // If 'monitorModeWNICName' is null, the setting does not exist in the database
                if (string.IsNullOrEmpty(monitorModeWNICName))
                {
                    // Setting does not exist or is empty; use the default value
                    monitorModeWNICName = defaultMonitorModeWNICName;
                }

                // Return the fetched value or the default if not found
                return monitorModeWNICName;
            }
        }

        private void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("-- Stopping capture");
            _stopCapturing = true;
            e.Cancel = true;
        }
    }
}
