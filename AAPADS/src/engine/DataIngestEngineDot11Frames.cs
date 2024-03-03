using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AAPADS
{
    public class DataIngestEngineDot11Frames
    {
        // The status flag for the capture process
        private bool _stopCapturingFlag;

        private const string PCAP_DLL = "wpcap.dll";

        [DllImport(PCAP_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pcap_set_rfmon(IntPtr p, int rfmon);

        public DataIngestEngineDot11Frames()
        {
            // The monitor mode adapter is set in the settings menu
            string HasMonitorModeAdapterBeenSet = FetchMonitorModeWNICDescription();

            // If a monitor mode adapater has not been set the default value is MONITOR MODE ADAPTER NOT SET
            // Start network capture if a monitor mode adapter has been set
            if (HasMonitorModeAdapterBeenSet != "MONITOR MODE ADAPTER NOT SET")
            {
                StartCaptureAsync();
            }
        }
        //ISSUES: 
        // the thread locks the program unless ctrl-c is pressed
        // does not seem to capture packets

        public void StartCaptureAsync()
        {
            Task.Run(() => StartCapture());
        }

        private void StartCapture()
        {
            // For testing key press would prevent 802.11 frame data collection
            Console.CancelKeyPress += HandleCancelKeyPress;

            // Retrieve the device list
            var devices = CaptureDeviceList.Instance;

            // If there is less then one device throw erroe message and return
            if (devices.Count < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ 802.11 CAPTURE ENGINE ] No devices were found on this machine");
                return;
            }

            // Print all the devices that sharpcap detects (Debugging method)
            // showAllDevices();


            // Get the same of the Monitor Mode adapter 
            String MonitorModeAdapterName = FetchMonitorModeWNICDescription();

            // Set the capture device that which contains the MonitorModeAdapterName
            var device = devices.FirstOrDefault(d => d.Description.Contains(MonitorModeAdapterName));

            // If no device was set it means it was not found it could be that it was unplugged
            if (device == null)
            {
                Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Device '{MonitorModeAdapterName}' not found on this machine");
                return;
            }

            // Set the configuration of the network adapter
            // without this no frames will print in pure Promiscuous with mon mode enabled
            // This was the only was I found to allow monitor mode frame capture with SharpPcap
            var configuration = new DeviceConfiguration
            {
                Snaplen = 65536,
                ReadTimeout = 1000,
                //Promiscuous = DeviceModes.Promiscuous,
                Immediate = true,
                TimestampResolution = TimestampResolution.Microsecond,
                Monitor = MonitorMode.Active, // IMPORTANT needs to be set for 802.11 frames to be captures
                BufferSize = 1 << 16,
                TimestampType = TimestampType.Host
            };

            // Open the divice with the configuration
            device.Open(configuration);

            // Set no filter
            device.Filter = "";

            Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Listening on {device.Description}");

            // bind the OnPacketArrival event subscription to Device_OnPacketArrival
            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            // Begin the capture
            device.StartCapture();

            // Keep thread running untill status flag is changed
            while (!_stopCapturingFlag)
            {
                // Keep the application running until Ctrl+C is pressed
                Task.Delay(1000).Wait();
            }

            //Stop Capture 
            device.StopCapture();

            // Close the deivce
            device.Close();

            Console.WriteLine("[ 802.11 CAPTURE ENGINE ] Capture stopped");
        }

        // Debug method to show all deivces detected by SharpPcap
        private void showAllDevices()
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ 802.11 CAPTURE ENGINE ] No devices were found on this machine");
                return;
            }
            int i = 0;

            foreach (var dev in devices)
            {
                Console.WriteLine($"{i} - {dev}");
                i++;
            }
        }

        private static void Device_OnPacketArrival(object s, PacketCapture e)
         {
            // Get the raw packet data
            var rawPacket = e.GetPacket();

            // define LinkLayerFrame as a LINK-LAYER HEADER TYPE frame. In this case with a WLAN this will be a IEEE 802.11 frame
            var LinkLayerFrame = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            // Print each framw payload
            // Console.WriteLine($"{LinkLayerFrame.PayloadPacket}");

            // Attempt to cast the PayloadPacket property of the LinkLayerFrame to a MacFrame type. 
            var IEEE80211Frame = LinkLayerFrame.PayloadPacket as PacketDotNet.Ieee80211.MacFrame;

            // If the 802.11 Frame is not full then print the type
            // TODO: Print to the GUI
            if (IEEE80211Frame != null)
            {
                Console.WriteLine($"802.11: [{IEEE80211Frame.FrameControl.SubType}]");
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
            Console.WriteLine("[ 802.11 CAPTURE ENGINE ] Stopping capture");
            _stopCapturingFlag = true;
            e.Cancel = true;
        }
    }

}
