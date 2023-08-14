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

typedef struct {
    char ssids[3200]; // Assuming 100 SSIDs * 32 characters each = 3200
    int count;
} SSIDList;


const int MAX_SSID_LENGTH = 32;  

__declspec(dllexport) void GetAvailableSSIDs(SSIDList* pResult) {
    DWORD negotiatedVersion;
    HANDLE clientHandle;

    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        WLAN_AVAILABLE_NETWORK_LIST* pAvailableNetworkList;
        WlanGetAvailableNetworkList(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, 0, NULL, &pAvailableNetworkList);

        pResult->count = pAvailableNetworkList->dwNumberOfItems;

        for (DWORD j = 0; j < pAvailableNetworkList->dwNumberOfItems; j++) {
            char* currentSSIDStart = pResult->ssids + j * MAX_SSID_LENGTH;
            strncpy_s(currentSSIDStart, MAX_SSID_LENGTH, pAvailableNetworkList->Network[j].dot11Ssid.ucSSID, _TRUNCATE);
            currentSSIDStart[MAX_SSID_LENGTH - 1] = '\0';  // ensure null termination
        }

        WlanFreeMemory(pAvailableNetworkList);
    }

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);
}




__declspec(dllexport) int GetRSSIForSSID(const char* targetSSID) {
    DWORD negotiatedVersion;
    HANDLE clientHandle;
    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    int rssi = 0;
    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        WLAN_AVAILABLE_NETWORK_LIST* pAvailableNetworkList;
        WlanGetAvailableNetworkList(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, 0, NULL, &pAvailableNetworkList);

        for (DWORD j = 0; j < pAvailableNetworkList->dwNumberOfItems; j++) {
            if (strcmp(targetSSID, pAvailableNetworkList->Network[j].dot11Ssid.ucSSID) == 0) {
                rssi = pAvailableNetworkList->Network[j].wlanSignalQuality;
                break;
            }
        }

        WlanFreeMemory(pAvailableNetworkList);
    }

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);

    return rssi;
}
__declspec(dllexport) void FreeSSIDListMemory(SSIDList* pSsidList) {
    
}
