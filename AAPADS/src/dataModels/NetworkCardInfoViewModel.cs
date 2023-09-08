using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AAPADS
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MyWLANStats
        {
            public ulong TransmittedFrameCount;
            public ulong ReceivedFrameCount;
            public ulong WEPExcludedCount;
            public ulong TKIPLocalMICFailures;
            public ulong TKIPReplays;
            public ulong TKIPICVErrorCount;
            public ulong CCMPReplays;
            public ulong CCMPDecryptErrors;
            public ulong WEPUndecryptableCount;
            public ulong WEPICVErrorCount;
            public ulong DecryptSuccessCount;
            public ulong DecryptFailureCount;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string AdapterName;

            public WLAN_INTERFACE_STATE AdapterStatus;
        }

        public enum WLAN_INTERFACE_STATE
        {
            wlan_interface_state_not_ready = 0,
            wlan_interface_state_connected,
            wlan_interface_state_ad_hoc_network_formed,
            wlan_interface_state_disconnecting,
            wlan_interface_state_disconnected,
            wlan_interface_state_associating,
            wlan_interface_state_discovering,
            wlan_interface_state_authenticating
        }

        [DllImport("WLANLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWLANStatistics(out MyWLANStats stats);
    }

    public class NetworkCardInfoViewModel : INotifyPropertyChanged
    {
        
        private string _networkCardName;
        private string _adapterStatus;
        private ObservableCollection<MacFrameData> _macFrameDataCollection;

        public string NETWORK_CARD_NAME
        {
            get { return _networkCardName; }
            set
            {
                _networkCardName = value;
                OnPropertyChanged(nameof(NETWORK_CARD_NAME));
            }
        }
        public string ADAPTER_STATUS
        {
            get { return _adapterStatus; }
            set
            {
                _adapterStatus = value;
                OnPropertyChanged(nameof(ADAPTER_STATUS));
            }
        }

        public ObservableCollection<MacFrameData> MacFrameDataCollection
        {
            get { return _macFrameDataCollection; }
            set
            {
                _macFrameDataCollection = value;
                OnPropertyChanged(nameof(MacFrameDataCollection));
            }
        }

        public async Task UpdateDataAsync()
        {
            try
            {
                var stats = await GetWlanStatsAsync();
                NETWORK_CARD_NAME = stats.AdapterName;
                ADAPTER_STATUS = mapWLANInterfaceStatus(stats.AdapterStatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async Task<NativeMethods.MyWLANStats> GetWlanStatsAsync()
        {
            return await Task.Run(() =>
            {
                NativeMethods.MyWLANStats stats = new NativeMethods.MyWLANStats();
                NativeMethods.GetWLANStatistics(out stats); // Use ref here
                return stats;
            });
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        internal string mapWLANInterfaceStatus(NativeMethods.WLAN_INTERFACE_STATE status)
        {
            switch (status)
            {
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_connected:
                    return "ACTIVE";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_disconnected:
                    return "DISCONNECTED";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_ad_hoc_network_formed:
                    return "AD-HOC NETWORK FORMED";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_disconnecting:
                    return "DISCONNECTING";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_associating:
                    return "ASSOCIATING";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_discovering:
                    return "DISCOVERING";
                case NativeMethods.WLAN_INTERFACE_STATE.wlan_interface_state_authenticating:
                    return "AUTHENTICATING";
                default:
                    return "UNKNOWN";
            }
        }



    }

    public class MacFrameData
    {
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        // Add other properties as needed
    }

}
