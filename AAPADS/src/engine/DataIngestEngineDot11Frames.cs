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
        private static bool _stopCapturing;

        public DataIngestEngineDot11Frames()
        {
            StartCaptureAsync();
        }



        public static void StartCaptureAsync()
        {
            Task.Run(() => StartCapture());
        }

        private static void StartCapture()
        {
            Console.CancelKeyPress += HandleCancelKeyPress;

            // Retrieve the device list
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            // Assuming the first device is the one we want to use
            var device = devices[0];
            device.Open(DeviceModes.Promiscuous, 1000); // Open device with a read timeout of 1000 milliseconds

            Console.WriteLine("-- Listening on {0}, hit 'ctrl-c' to stop...", device.Description);

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
            //var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            //if (packet is IEEE80211Packet)
            //{
            //    var wifiPacket = (IEEE80211Packet)packet;

            //    // Check if this is a management frame
            //    if (wifiPacket.FrameControl.SubType == FrameControlField.FrameSubTypes.ManagementBeacon ||
            //        wifiPacket.FrameControl.SubType == FrameControlField.FrameSubTypes.ManagementAssociationRequest ||
            //        wifiPacket.FrameControl.SubType == FrameControlField.FrameSubTypes.ManagementAssociationResponse 
            //        // Add other management subtypes as needed
            //        )
            //    {
            //        // Process the management frame
            //        Console.WriteLine($"Management Frame captured: {wifiPacket.GetType()}");
            //        
            //        if (wifiPacket is DeauthenticationFrame deauthFrame)
            //        {
            //            Console.WriteLine($"Deauthentication Frame: Reason = {deauthFrame.Reason}");
            //        }
            //    }
            //}
        }


        private static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("-- Stopping capture");
            _stopCapturing = true;
            e.Cancel = true;
        }
    }
}
