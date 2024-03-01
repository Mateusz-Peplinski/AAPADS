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
        private static CaptureFileWriterDevice captureFileWriter;


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

            // Print all the devices that sharpcap detects
            // showAllDevices();

            String MonitorModeAdapterName = FetchMonitorModeWNICDescription();

            var device = devices.FirstOrDefault(d => d.Description.Contains(MonitorModeAdapterName));
            if (device == null)
            {
                Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Device '{MonitorModeAdapterName}' not found on this machine");
                return;
            }

            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;

            //IntPtr deviceHandle; // You would need to obtain this from the SharpPcap device handle
            //int monitorMode = 1; // 1 to enable monitor mode, 0 to disable
            //int result = PcapNativeMethods.pcap_set_rfmon(deviceHandle, monitorMode);
            //if (result != 0)
            //{
            //    throw new Exception("Failed to set monitor mode");
            //}

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


            device.Open(configuration);
            device.Filter = "";

            Console.WriteLine($"[ 802.11 CAPTURE ENGINE ] Listening on {device.Description}, hit 'ctrl-c' to stop...");

            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            string capFile = "80211Capture";

            // create the output file
            captureFileWriter = new CaptureFileWriterDevice(capFile);



            captureFileWriter.Open(device);

            device.StartCapture();

            while (!_stopCapturingFlag)
            {
                // Keep the application running until Ctrl+C is pressed
                Task.Delay(1000).Wait();
            }

            device.StopCapture();
            device.Close();
            Console.WriteLine("[ 802.11 CAPTURE ENGINE ] Capture stopped");
        }

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

            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            //Console.WriteLine($"{packet.PayloadPacket}");
            // Directly check for MacFrame if Ieee80211RadioPacket is not recognized

            var wifiPacket = packet.PayloadPacket as PacketDotNet.Ieee80211.MacFrame;

            if (wifiPacket != null)
            {
                Console.WriteLine($"802.11: [{wifiPacket.FrameControl.SubType}]");
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
            Console.WriteLine("Stopping capture");
            _stopCapturingFlag = true;
            e.Cancel = true;
        }
    }

}
