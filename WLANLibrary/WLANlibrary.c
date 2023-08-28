#include <Windows.h>
#include <wlanapi.h>
#include <stdio.h>
#include <windot11.h>

#pragma comment(lib, "wlanapi.lib")

const int MAX_STRING_LENGTH = 32;

typedef struct {
    char ssids[3200]; // 100 SSIDs * 32 characters each = 3200
    int count;
} SSIDList;

#define MAX_BSSIDS_PER_SSID 5

typedef struct {
    char ssids[3200]; //  100 SSIDs * 32 characters each = 3200
    char authMethods[3200]; // 100 * 32
    char encryptionTypes[3200]; // 100 * 32
    char bssids[8500]; // 100 SSIDs * 5 BSSIDs * 17 chars
    int channels[100][MAX_BSSIDS_PER_SSID];
    int count;
} ExtendedSSIDList;

__declspec(dllexport) BOOL PerformWifiScan() {
    DWORD negotiatedVersion;
    HANDLE clientHandle = NULL;
    WLAN_INTERFACE_INFO_LIST* pInterfaceList = NULL;
    DWORD dwResult;
   
    dwResult = WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);
    if (dwResult != ERROR_SUCCESS) {
        fprintf(stderr, "WlanOpenHandle failed with error: %u\n", dwResult);
        return FALSE; 
    }
    
    dwResult = WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);
    if (dwResult != ERROR_SUCCESS) {
        fprintf(stderr, "WlanEnumInterfaces failed with error: %u\n", dwResult);
        WlanCloseHandle(clientHandle, NULL);
        return FALSE;
    }
    
    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        dwResult = WlanScan(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, NULL, NULL, NULL);
        if (dwResult != ERROR_SUCCESS) {
            fprintf(stderr, "WlanScan on interface %u failed with error: %u\n", i, dwResult);
        }
    }
    
    if (pInterfaceList) {
        WlanFreeMemory(pInterfaceList);
    }
    if (clientHandle) {
        WlanCloseHandle(clientHandle, NULL);
    }

    return TRUE; 
}

__declspec(dllexport) void GetAvailableSSIDs_Extended(ExtendedSSIDList* pResult) {
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
            // SSID
            char* currentSSIDStart = pResult->ssids + j * MAX_STRING_LENGTH;
            strncpy_s(currentSSIDStart, MAX_STRING_LENGTH, pAvailableNetworkList->Network[j].dot11Ssid.ucSSID, _TRUNCATE);
            currentSSIDStart[MAX_STRING_LENGTH - 1] = '\0';  // ensure null termination

            // Authentication method
            char* currentAuthStart = pResult->authMethods + j * MAX_STRING_LENGTH;
            switch (pAvailableNetworkList->Network[j].dot11DefaultAuthAlgorithm) {
            case DOT11_AUTH_ALGO_WPA:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "WPA");
                break;
            case DOT11_AUTH_ALGO_WPA_PSK:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "WPA-PSK");
                break;
            case DOT11_AUTH_ALGO_WPA_NONE:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "WPA-NONE");
                break;
            case DOT11_AUTH_ALGO_WPA3:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "WPA3");
                break;
            case DOT11_AUTH_ALGO_80211_SHARED_KEY:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "SHARED KEY");
                break;
            case DOT11_AUTH_ALGO_80211_OPEN:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "OPEN");
                break;
            default:
                strcpy_s(currentAuthStart, MAX_STRING_LENGTH, "UNKNOWN");
                break;
            }

            // Encryption type
            char* currentEncStart = pResult->encryptionTypes + j * MAX_STRING_LENGTH;
            switch (pAvailableNetworkList->Network[j].dot11DefaultCipherAlgorithm) {
            case DOT11_CIPHER_ALGO_WEP40:
            case DOT11_CIPHER_ALGO_WEP:
            case DOT11_CIPHER_ALGO_WEP104:
                strcpy_s(currentEncStart, MAX_STRING_LENGTH, "WEP");
                break;
            case DOT11_CIPHER_ALGO_CCMP:
                strcpy_s(currentEncStart, MAX_STRING_LENGTH, "AES");
                break;
            case DOT11_CIPHER_ALGO_TKIP:
                strcpy_s(currentEncStart, MAX_STRING_LENGTH, "TKIP");
                break;
            case DOT11_CIPHER_ALGO_NONE:
                strcpy_s(currentEncStart, MAX_STRING_LENGTH, "NONE");
                break;
            default:
                strcpy_s(currentEncStart, MAX_STRING_LENGTH, "UNKNOWN");
                break;
            }

            // BSSID (MAC address)
            WLAN_BSS_LIST* pBssList;
            WlanGetNetworkBssList(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, &pAvailableNetworkList->Network[j].dot11Ssid, pAvailableNetworkList->Network[j].dot11BssType, FALSE, NULL, &pBssList);
            for (DWORD bssidIndex = 0; bssidIndex < pBssList->dwNumberOfItems && bssidIndex < MAX_BSSIDS_PER_SSID; bssidIndex++) {
                char* currentBSSIDStart = pResult->bssids + (j * MAX_BSSIDS_PER_SSID + bssidIndex) * 17; 
                sprintf_s(currentBSSIDStart, 17, "%02X:%02X:%02X:%02X:%02X:%02X",
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[0],
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[1],
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[2],
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[3],
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[4],
                    pBssList->wlanBssEntries[bssidIndex].dot11Bssid[5]);
                int channel = ConvertFrequencyToChannel(pBssList->wlanBssEntries[bssidIndex].ulChCenterFrequency);
                pResult->channels[j][bssidIndex] = channel;
            }
            WlanFreeMemory(pBssList);
           
        }

        WlanFreeMemory(pAvailableNetworkList);
    }

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);
}

int ConvertFrequencyToChannel(ULONG freq) {
    if (freq >= 2412000 && freq <= 2483000) {
        return (freq - 2412000) / 5000 + 1;
    }
    else if (freq >= 5170000 && freq <= 5825000) {
        return (freq - 5170000) / 5000 + 34;
    }
    else {
        return -1;  
    }
}

typedef struct {
    char ssid[32];
    char bssid[18]; // 17 characters + null terminator
    char authMethod[32];
    char encryptionType[32];
    int channel;
} NetworkInfo;

__declspec(dllexport) int GetVisibleNetworks(NetworkInfo* networks, int maxNetworks) {
    HANDLE hClient = NULL;
    DWORD dwMaxClient = 2;    // Assume a max of two wireless interfaces
    DWORD dwCurVersion = 0;
    DWORD dwResult = 0;
    int count = 0;

    dwResult = WlanOpenHandle(dwMaxClient, NULL, &dwCurVersion, &hClient);
    if (dwResult != ERROR_SUCCESS) {
        return -1; // Error handling
    }

    PWLAN_INTERFACE_INFO_LIST pIfList = NULL;
    dwResult = WlanEnumInterfaces(hClient, NULL, &pIfList);
    if (dwResult != ERROR_SUCCESS || pIfList == NULL) {
        WlanCloseHandle(hClient, NULL);
        return -1; // Error handling
    }

    for (int i = 0; i < (int)pIfList->dwNumberOfItems && count < maxNetworks; i++) {
        PWLAN_AVAILABLE_NETWORK_LIST pNetworkList = NULL;
        dwResult = WlanGetAvailableNetworkList(hClient, &pIfList->InterfaceInfo[i].InterfaceGuid, 0, NULL, &pNetworkList);
        if (dwResult != ERROR_SUCCESS || pNetworkList == NULL) {
            continue; // Move to the next interface
        }

        for (int j = 0; j < pNetworkList->dwNumberOfItems && count < maxNetworks; j++) {
            PWLAN_AVAILABLE_NETWORK pNetwork = &pNetworkList->Network[j];

            // SSID
            for (int k = 0; k < pNetwork->dot11Ssid.uSSIDLength; k++) {
                networks[count].ssid[k] = pNetwork->dot11Ssid.ucSSID[k];
            }
            networks[count].ssid[pNetwork->dot11Ssid.uSSIDLength] = '\0';

            PWLAN_BSS_LIST pBssList = NULL;

            // BSSID
            WlanGetNetworkBssList(hClient, &pIfList->InterfaceInfo[i].InterfaceGuid, &pNetwork->dot11Ssid, pNetwork->dot11BssType, TRUE, NULL, &pBssList);
            if (pBssList != NULL && pBssList->dwNumberOfItems > 0) {
                PWLAN_BSS_ENTRY pBssEntry = &pBssList->wlanBssEntries[0];
                sprintf_s(networks[count].bssid, sizeof(networks[count].bssid), "%02X:%02X:%02X:%02X:%02X:%02X",
                    pBssEntry->dot11Bssid[0], pBssEntry->dot11Bssid[1], pBssEntry->dot11Bssid[2],
                    pBssEntry->dot11Bssid[3], pBssEntry->dot11Bssid[4], pBssEntry->dot11Bssid[5]);

                networks[count].channel = ConvertFrequencyToChannel(pBssEntry->ulChCenterFrequency);

            }
            if (pBssList) {
                WlanFreeMemory(pBssList);
            }

            // Auth Method
            switch (pNetwork->dot11DefaultAuthAlgorithm) {
            case DOT11_AUTH_ALGO_80211_OPEN:
                strcpy_s(networks[count].authMethod, 32, "802.11 OPEN");
                break;
            case DOT11_AUTH_ALGO_80211_SHARED_KEY:
                strcpy_s(networks[count].authMethod, 32, "802.11 SHARED KEY");
                break;
            case DOT11_AUTH_ALGO_WPA:
                strcpy_s(networks[count].authMethod, 32, "WPA");
                break;
            case DOT11_AUTH_ALGO_WPA_PSK:
                strcpy_s(networks[count].authMethod, 32, "WPA PSK");
                break;
            case DOT11_AUTH_ALGO_WPA_NONE:
                strcpy_s(networks[count].authMethod, 32, "WPA NONE");
                break;
            case DOT11_AUTH_ALGO_RSNA:
                strcpy_s(networks[count].authMethod, 32, "RSNA");
                break;
            case DOT11_AUTH_ALGO_RSNA_PSK:
                strcpy_s(networks[count].authMethod, 32, "RSNA PSK");
                break;
            case DOT11_AUTH_ALGO_WPA3:
                strcpy_s(networks[count].authMethod, 32, "WPA3");
                break;
            case DOT11_AUTH_ALGO_WPA3_SAE:
                strcpy_s(networks[count].authMethod, 32, "WPA3 SAE");
                break;
            case DOT11_AUTH_ALGO_OWE:
                strcpy_s(networks[count].authMethod, 32, "OWE");
                break;
            case DOT11_AUTH_ALGO_WPA3_ENT:
                strcpy_s(networks[count].authMethod, 32, "WPA3 ENT");
                break;
            default:
                strcpy_s(networks[count].authMethod, 32, "UNKNOWN");
                break;
            }


            // Encryption Type
            switch (pNetwork->dot11DefaultCipherAlgorithm) {
            case DOT11_CIPHER_ALGO_NONE:
                strcpy_s(networks[count].encryptionType, 32, "NONE");
                break;
            case DOT11_CIPHER_ALGO_WEP40:
            case DOT11_CIPHER_ALGO_WEP:
            case DOT11_CIPHER_ALGO_WEP104:
                strcpy_s(networks[count].encryptionType, 32, "WEP");
                break;
            case DOT11_CIPHER_ALGO_TKIP:
                strcpy_s(networks[count].encryptionType, 32, "TKIP");
                break;
            case DOT11_CIPHER_ALGO_CCMP:
                strcpy_s(networks[count].encryptionType, 32, "CCMP");
                break;
            case DOT11_CIPHER_ALGO_WPA_USE_GROUP:
                strcpy_s(networks[count].encryptionType, 32, "WPA USE GROUP");
                break;
            default:
                strcpy_s(networks[count].encryptionType, 32, "UNKNOWN");
                break;
            }

            count++;
            if (count >= maxNetworks) {
                break;
            }


        }
        WlanFreeMemory(pNetworkList);
    }
    WlanFreeMemory(pIfList);
    WlanCloseHandle(hClient, NULL);
    return count;
}



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
            char* currentSSIDStart = pResult->ssids + j * MAX_STRING_LENGTH;
            strncpy_s(currentSSIDStart, MAX_STRING_LENGTH, pAvailableNetworkList->Network[j].dot11Ssid.ucSSID, _TRUNCATE);
            currentSSIDStart[MAX_STRING_LENGTH - 1] = '\0';  
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
