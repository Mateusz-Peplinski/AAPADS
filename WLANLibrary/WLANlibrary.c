#include <Windows.h>
#include <wlanapi.h>
#include <stdio.h>

#pragma comment(lib, "wlanapi.lib")

#include <Windows.h>
#include <wlanapi.h>
#include <stdio.h>

#pragma comment(lib, "wlanapi.lib")

__declspec(dllexport) void PerformWifiScan() {
    DWORD negotiatedVersion;
    HANDLE clientHandle;
    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        WlanScan(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, NULL, NULL, NULL);
    }

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);
}

