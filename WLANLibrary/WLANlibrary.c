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
        fprintf(stderr, "[ WLAN LIBRARY ] WlanOpenHandle failed with error: %u\n", dwResult);
        return FALSE; 
    }
    
    dwResult = WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);
    if (dwResult != ERROR_SUCCESS) {
        fprintf(stderr, "[ WLAN LIBRARY ] WlanEnumInterfaces failed with error: %u\n", dwResult);
        WlanCloseHandle(clientHandle, NULL);
        return FALSE;
    }
    
    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        dwResult = WlanScan(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, NULL, NULL, NULL);
        if (dwResult != ERROR_SUCCESS) {
            fprintf(stderr, "[ WLAN LIBRARY ] WlanScan on interface %u failed with error: %u\n", i, dwResult);
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

// MAIN NETWORK STRUCT FOR AAPADS AAPI
typedef struct {
    char ssid[32];
    char bssid[18]; // 17 characters + null terminator
    char authMethod[32];
    char encryptionType[32];
    int bssidPhyType;
    int channel;
    int bssType;
    int beaconPeriod;
    ULONG frequency;
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
          
            // BSSID
            PWLAN_BSS_LIST pBssList = NULL;
            WlanGetNetworkBssList(hClient, &pIfList->InterfaceInfo[i].InterfaceGuid, &pNetwork->dot11Ssid, pNetwork->dot11BssType, TRUE, NULL, &pBssList);
            if (pBssList != NULL) {
                for (int bssIndex = 0; bssIndex < pBssList->dwNumberOfItems && count < maxNetworks; bssIndex++) {
                    PWLAN_BSS_ENTRY pBssEntry = &pBssList->wlanBssEntries[bssIndex];
                    sprintf_s(networks[count].bssid, sizeof(networks[count].bssid), "%02X:%02X:%02X:%02X:%02X:%02X",
                        pBssEntry->dot11Bssid[0], pBssEntry->dot11Bssid[1], pBssEntry->dot11Bssid[2],
                        pBssEntry->dot11Bssid[3], pBssEntry->dot11Bssid[4], pBssEntry->dot11Bssid[5]);

                    //Channel
                    networks[count].channel = ConvertFrequencyToChannel(pBssEntry->ulChCenterFrequency);

                    //Freq
                    networks[count].frequency = pBssEntry->ulChCenterFrequency;

                    // BSS Type (int)
                    networks[count].bssType = pBssEntry->dot11BssType;

                    // BSSID PHY TYPE (int)
                    networks[count].bssidPhyType = pBssEntry->dot11BssPhyType;

                    // BECON PERIOD (int)
                    networks[count].beaconPeriod = pBssEntry->usBeaconPeriod;


                    // SSID
                    for (int k = 0; k < pNetwork->dot11Ssid.uSSIDLength; k++) {
                        networks[count].ssid[k] = pNetwork->dot11Ssid.ucSSID[k];
                    }
                    networks[count].ssid[pNetwork->dot11Ssid.uSSIDLength] = '\0';

                    // Auth Method
                    switch (pNetwork->dot11DefaultAuthAlgorithm) {
                    case DOT11_AUTH_ALGO_80211_OPEN:
                        strcpy_s(networks[count].authMethod, 32, "802.11 OPEN");
                        break;
                    case DOT11_AUTH_ALGO_80211_SHARED_KEY:
                        strcpy_s(networks[count].authMethod, 32, "802.11 SHARED KEY");
                        break;
                    case DOT11_AUTH_ALGO_RSNA:
                        strcpy_s(networks[count].authMethod, 32, "WPA2 Enterprise");
                        break;
                    case DOT11_AUTH_ALGO_RSNA_PSK:
                        strcpy_s(networks[count].authMethod, 32, "WPA2 PSK");
                        break;
                    case DOT11_AUTH_ALGO_WPA3:
                        strcpy_s(networks[count].authMethod, 32, "WPA3");
                        break;
                    case DOT11_AUTH_ALGO_WPA3_SAE:
                        strcpy_s(networks[count].authMethod, 32, "WPA3 Personal");
                        break;
                    case DOT11_AUTH_ALGO_WPA3_ENT:
                        strcpy_s(networks[count].authMethod, 32, "WPA3 Enterprise");
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
                    case DOT11_AUTH_ALGO_OWE:
                        strcpy_s(networks[count].authMethod, 32, "OWE");
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
            }

        }
        WlanFreeMemory(pNetworkList);
    }
    WlanFreeMemory(pIfList);
    WlanCloseHandle(hClient, NULL);
    return count;
}

typedef struct {
    ULONGLONG TransmittedFrameCount;
    ULONGLONG ReceivedFrameCount;
    ULONGLONG WEPExcludedCount;
    ULONGLONG TKIPLocalMICFailures;
    ULONGLONG TKIPReplays;
    ULONGLONG TKIPICVErrorCount;
    ULONGLONG CCMPReplays;
    ULONGLONG CCMPDecryptErrors;
    ULONGLONG WEPUndecryptableCount;
    ULONGLONG WEPICVErrorCount;
    ULONGLONG DecryptSuccessCount;
    ULONGLONG DecryptFailureCount;
    char AdapterName[256]; 
    WLAN_INTERFACE_STATE AdapterStatus;
} MyWLANStats;
//// program crashed here.. It tryed to wirte to protected memeory
__declspec(dllexport) int GetWLANStatistics(MyWLANStats* stats) {
    DWORD negotiatedVersion;
    HANDLE clientHandle;

    memset(stats, 0, sizeof(MyWLANStats));


    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    DWORD result;
    result = WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);
    if (result != ERROR_SUCCESS) {
        printf("[ WLAN LIBRARY ] Error Opening Handle for GetWLANStatistics() \n");
    }

    WLAN_STATISTICS wlanStats;
    DWORD dataSize = sizeof(WLAN_STATISTICS);
    WlanQueryInterface(clientHandle, &pInterfaceList->InterfaceInfo[0].InterfaceGuid, wlan_intf_opcode_statistics, NULL, &dataSize, (PVOID*)&wlanStats, NULL);

    WCHAR* interfaceName = pInterfaceList->InterfaceInfo[0].strInterfaceDescription;
    WideCharToMultiByte(CP_UTF8, 0, interfaceName, -1, stats->AdapterName, sizeof(stats->AdapterName), NULL, NULL);

    stats->AdapterStatus = pInterfaceList->InterfaceInfo[0].isState;
    stats->TransmittedFrameCount = wlanStats.MacUcastCounters.ullTransmittedFrameCount;
    stats->ReceivedFrameCount = wlanStats.MacUcastCounters.ullReceivedFrameCount;
    stats->WEPExcludedCount = wlanStats.MacUcastCounters.ullWEPExcludedCount;
    stats->TKIPLocalMICFailures = wlanStats.MacUcastCounters.ullTKIPLocalMICFailures;
    stats->TKIPReplays = wlanStats.MacUcastCounters.ullTKIPReplays;
    stats->TKIPICVErrorCount = wlanStats.MacUcastCounters.ullTKIPICVErrorCount;
    stats->CCMPReplays = wlanStats.MacUcastCounters.ullCCMPReplays;
    stats->CCMPDecryptErrors = wlanStats.MacUcastCounters.ullCCMPDecryptErrors;
    stats->WEPUndecryptableCount = wlanStats.MacUcastCounters.ullWEPUndecryptableCount;
    stats->WEPICVErrorCount = wlanStats.MacUcastCounters.ullWEPICVErrorCount;
    stats->DecryptSuccessCount = wlanStats.MacUcastCounters.ullDecryptSuccessCount;
    stats->DecryptFailureCount = wlanStats.MacUcastCounters.ullDecryptFailureCount;
    printf("%llu\n", wlanStats.MacUcastCounters.ullTransmittedFrameCount);

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);

    return 0; // Return 0 for success or another value for failure.
}

__declspec(dllexport) int GetRSSIForSSID(const char* targetSSID) {
    DWORD negotiatedVersion;
    HANDLE clientHandle;
    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    int rssi = 0;
    for (DWORD i = 0; i < pInterfaceList->dwNumberOfItems; i++) {
        PWLAN_BSS_LIST pBssList;
        WlanGetNetworkBssList(clientHandle, &pInterfaceList->InterfaceInfo[i].InterfaceGuid, NULL, dot11_BSS_type_any, FALSE, NULL, &pBssList);

        for (DWORD j = 0; j < pBssList->dwNumberOfItems; j++) {
            if (strcmp(targetSSID, pBssList->wlanBssEntries[j].dot11Ssid.ucSSID) == 0) {
                rssi = pBssList->wlanBssEntries[j].lRssi;
                break;
            }
        }

        WlanFreeMemory(pBssList);
    }

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);

    return rssi;
}
typedef struct {
    char AdapterName[256]; 
    WLAN_INTERFACE_STATE AdapterStatus;
} WLANAdapterStatus;
//// Net version for windows 11
__declspec(dllexport) int WLANGetAdapterStatus(WLANAdapterStatus* stats) {
    DWORD negotiatedVersion;
    HANDLE clientHandle;

    memset(stats, 0, sizeof(WLANAdapterStatus));


    WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);

    WLAN_INTERFACE_INFO_LIST* pInterfaceList;
    WlanEnumInterfaces(clientHandle, NULL, &pInterfaceList);

    DWORD result;
    result = WlanOpenHandle(2, NULL, &negotiatedVersion, &clientHandle);
    if (result != ERROR_SUCCESS) {
        printf("[ WLAN LIBRARY ] Error Opening Handle for WLANGetAdapterStatus()\n");
    }

    WLAN_STATISTICS wlanStats;
    DWORD dataSize = sizeof(WLAN_STATISTICS);
    WlanQueryInterface(clientHandle, &pInterfaceList->InterfaceInfo[0].InterfaceGuid, wlan_intf_opcode_statistics, NULL, &dataSize, (PVOID*)&wlanStats, NULL);

    WCHAR* interfaceName = pInterfaceList->InterfaceInfo[0].strInterfaceDescription;
    WideCharToMultiByte(CP_UTF8, 0, interfaceName, -1, stats->AdapterName, sizeof(stats->AdapterName), NULL, NULL);

    stats->AdapterStatus = pInterfaceList->InterfaceInfo[0].isState;
   

    WlanFreeMemory(pInterfaceList);
    WlanCloseHandle(clientHandle, NULL);

    return 0;
}


// ###############################################################################
//                                FUNCTION ACHIVE BELOW                      #####
//                           CURRENTLY NOT IN USE IN AAPADS                  #####
// ###############################################################################

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