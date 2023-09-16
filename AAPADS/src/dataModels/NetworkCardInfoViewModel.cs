﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private ulong _TransmittedFrameCount;
        private ulong _ReceivedFrameCount;
        private ulong _WEPExcludedCount;
        private ulong _TKIPLocalMICFailures;
        private ulong _TKIPReplays;
        private ulong _TKIPICVErrorCount;
        private ulong _CCMPReplays;
        private ulong _CCMPDecryptErrors;
        private ulong _WEPUndecryptableCount;
        private ulong _WEPICVErrorCount;
        private ulong _DecryptSuccessCount;
        private ulong _DecryptFailureCount;
        private CancellationTokenSource _cts;

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
        public ulong TRANSMITTED_FRAME_COUNT
        {
            get { return _TransmittedFrameCount; }
            set
            {
                _TransmittedFrameCount = value;
                OnPropertyChanged(nameof(TRANSMITTED_FRAME_COUNT));
            }
        }
        public ulong RECEIVED_FRAME_COUNT
        {
            get { return _ReceivedFrameCount; }
            set
            {
                _ReceivedFrameCount = value;
                OnPropertyChanged(nameof(RECEIVED_FRAME_COUNT));
            }
        }
        public ulong WEP_EXCLUDED_COUNT
        {
            get { return _WEPExcludedCount; }
            set
            {
                _WEPExcludedCount = value;
                OnPropertyChanged(nameof(WEP_EXCLUDED_COUNT));
            }
        }
        public ulong TKIP_LOCAL_MIC_FAILURES
        {
            get { return _TKIPLocalMICFailures; }
            set
            {
                _TKIPLocalMICFailures = value;
                OnPropertyChanged(nameof(TKIP_LOCAL_MIC_FAILURES));
            }
        }
        public ulong TKIP_REPLAYS
        {
            get { return _TKIPReplays; }
            set
            {
                _TKIPReplays = value;
                OnPropertyChanged(nameof(TKIP_REPLAYS));
            }
        }
        public ulong TKIP_ICV_ERROR_COUNT
        {
            get { return _TKIPICVErrorCount; }
            set
            {
                _TKIPICVErrorCount = value;
                OnPropertyChanged(nameof(TKIP_ICV_ERROR_COUNT));
            }
        }
        public ulong CCMP_REPLAYS
        {
            get { return _CCMPReplays; }
            set
            {
                _CCMPReplays = value;
                OnPropertyChanged(nameof(CCMP_REPLAYS));
            }
        }
        public ulong CCMP_DECRYPT_ERRORS
        {
            get { return _CCMPDecryptErrors; }
            set
            {
                _CCMPDecryptErrors = value;
                OnPropertyChanged(nameof(CCMP_DECRYPT_ERRORS));
            }
        }
        public ulong WEP_UNDECRYPTABLE_COUNT
        {
            get { return _WEPUndecryptableCount; }
            set
            {
                _WEPUndecryptableCount = value;
                OnPropertyChanged(nameof(WEP_UNDECRYPTABLE_COUNT));
            }
        }
        public ulong WEP_ICV_ERROR_COUNT
        {
            get { return _WEPICVErrorCount; }
            set
            {
                _WEPICVErrorCount = value;
                OnPropertyChanged(nameof(WEP_ICV_ERROR_COUNT));
            }
        }
        public ulong DECRYPT_SUCCESS_COUNT
        {
            get { return _DecryptSuccessCount; }
            set
            {
                _DecryptSuccessCount = value;
                OnPropertyChanged(nameof(DECRYPT_SUCCESS_COUNT));
            }
        }
        public ulong DECRYPT_FAILURE_COUNT
        {
            get { return _DecryptFailureCount; }
            set
            {
                _DecryptFailureCount = value;
                OnPropertyChanged(nameof(DECRYPT_FAILURE_COUNT));
            }
        }

        public NetworkCardInfoViewModel()
        {
            _cts = new CancellationTokenSource();
            StartDataUpdateLoop(_cts.Token);
        }
        private async void StartDataUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UpdateDataAsync();

                // Wait for 5 seconds before the next iteration
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        public async Task UpdateDataAsync()
        {
            try
            {
                var stats = await GetWlanStatsAsync();
                NETWORK_CARD_NAME = stats.AdapterName;
                ADAPTER_STATUS = mapWLANInterfaceStatus(stats.AdapterStatus);
                TRANSMITTED_FRAME_COUNT = stats.TransmittedFrameCount;
                RECEIVED_FRAME_COUNT = stats.ReceivedFrameCount;
                WEP_EXCLUDED_COUNT = stats.WEPExcludedCount;
                TKIP_LOCAL_MIC_FAILURES = stats.TKIPLocalMICFailures;
                TKIP_REPLAYS = stats.TKIPReplays;
                TKIP_ICV_ERROR_COUNT = stats.TKIPICVErrorCount;
                CCMP_REPLAYS = stats.CCMPReplays;
                CCMP_DECRYPT_ERRORS = stats.CCMPDecryptErrors;
                WEP_UNDECRYPTABLE_COUNT = stats.WEPUndecryptableCount;
                WEP_ICV_ERROR_COUNT = stats.WEPICVErrorCount;
                DECRYPT_SUCCESS_COUNT = stats.DecryptSuccessCount;
                DECRYPT_FAILURE_COUNT = stats.DecryptFailureCount;
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
                NativeMethods.GetWLANStatistics(out stats);
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

}