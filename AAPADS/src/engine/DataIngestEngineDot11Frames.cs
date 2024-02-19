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
                Console.WriteLine("No devices were found on this machine");
                return;
            }
            int i = 0; 

            //Print all the devices that sharpcap detects
            //foreach (var dev in devices) 
            //{
            //    Console.WriteLine($"{i} - {dev}");
            //    i++;
            //}
            // Find a device that matches the name "WiFi 2"
            var device = devices.FirstOrDefault(d => d.Name.Contains("WiFi 2"));
            if (device == null)
            {
                Console.WriteLine("Device 'WiFi 2' not found on this machine");
                return;
            }

            // Open the device for capturing
            device.Open(DeviceModes.Promiscuous, 1000); // Open device with a read timeout of 1000 milliseconds

            Console.WriteLine($"-- Listening on {device.Description}, hit 'ctrl-c' to stop...");

            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            device.StartCapture();

            while (!_stopCapturing)
            {
                // Keep the application running until Ctrl+C is pressed
                Task.Delay(1000).Wait();
            }

            device.StopCapture();
            device.Close();
            Console.WriteLine("-- Capture stopped");
        }


        private static void Device_OnPacketArrival(object s, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);

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


        private void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("-- Stopping capture");
            _stopCapturing = true;
            e.Cancel = true;
        }
    }
}
