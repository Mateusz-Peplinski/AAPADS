using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AAPADS
{
    public class WifiScanner
    {
        [DllImport("Wlanapi.dll")]
        public static extern int WlanOpenHandle(
            [In] UInt32 clientVersion,
            [In, Out] IntPtr pReserved,
            [Out] out UInt32 negotiatedVersion,
            [Out] out IntPtr clientHandle
        );

        [DllImport("Wlanapi.dll")]
        public static extern int WlanCloseHandle(
            [In] IntPtr clientHandle,
            [In, Out] IntPtr pReserved
        );

        [DllImport("Wlanapi.dll")]
        public static extern int WlanScan(
            [In] IntPtr clientHandle,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
            [In, Out] IntPtr pDot11Ssid,
            [In, Out] IntPtr pIeData,
            [In, Out] IntPtr pReserved
        );

        [DllImport("Wlanapi.dll")]
        public static extern int WlanEnumInterfaces(
            [In] IntPtr clientHandle,
            [In, Out] IntPtr pReserved,
            [Out] out IntPtr ppInterfaceList
        );

        [DllImport("Wlanapi.dll")]
        public static extern void WlanFreeMemory(IntPtr pMemory);

        public enum WLAN_INTERFACE_STATE
        {
            wlan_interface_state_not_ready,
            wlan_interface_state_connected,
            wlan_interface_state_ad_hoc_network_formed,
            wlan_interface_state_disconnecting,
            wlan_interface_state_disconnected,
            wlan_interface_state_associating,
            wlan_interface_state_discovering,
            wlan_interface_state_authenticating
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WLAN_INTERFACE_INFO
        {
            public Guid InterfaceGuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strInterfaceDescription;
            public WLAN_INTERFACE_STATE isState;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_INTERFACE_INFO_LIST
        {
            public uint dwNumberOfItems;
            public uint dwIndex;
            public WLAN_INTERFACE_INFO[] InterfaceInfo;

            public WLAN_INTERFACE_INFO_LIST(IntPtr pList)
            {
                dwNumberOfItems = (uint)Marshal.ReadInt32(pList, 0);
                dwIndex = (uint)Marshal.ReadInt32(pList, 4);
                InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    IntPtr pItemList = new IntPtr(pList.ToInt64() + i * Marshal.SizeOf(typeof(WLAN_INTERFACE_INFO)) + 8);
                    InterfaceInfo[i] = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(pItemList);
                }
            }
        }


        public static void PerformWifiScan()
        {
            UInt32 negotiatedVersion;
            IntPtr clientHandle;
            WlanOpenHandle(2, IntPtr.Zero, out negotiatedVersion, out clientHandle);

            IntPtr ppInterfaceList;
            WlanEnumInterfaces(clientHandle, IntPtr.Zero, out ppInterfaceList);

            WLAN_INTERFACE_INFO_LIST interfaceList = new WLAN_INTERFACE_INFO_LIST(ppInterfaceList);

            for (int i = 0; i < (int)interfaceList.dwNumberOfItems; i++)
            {
                WlanScan(clientHandle, interfaceList.InterfaceInfo[i].InterfaceGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            WlanFreeMemory(ppInterfaceList);
            WlanCloseHandle(clientHandle, IntPtr.Zero);
        }
    }


}
