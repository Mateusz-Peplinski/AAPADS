using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class detectionDictionary
    {
        public Dictionary<string, string> wirelessAttacks = new Dictionary<string, string>
        {
            {"Eavesdropping (Sniffing)", "An attacker passively intercepts and logs wireless traffic, usually to capture sensitive information like passwords or session tokens. This is possible because radio frequencies (RF) can be easily intercepted."},
            {"Man-in-the-Middle Attack (MitM)", "The attacker intercepts communications between two parties and relays messages between them, making them believe they're talking directly to each other. This allows the attacker to manipulate the conversation or gather sensitive data."},
            {"Rogue Access Points", "An attacker sets up an unauthorized wireless access point (often with a similar SSID to a legitimate network) to lure users into connecting. Once users are connected, their data can be intercepted or they can be subjected to MitM attacks."},
            {"Deauthentication Attack", "This is an attack against the Wi-Fi Protected Access (WPA) and WPA2 protocols. An attacker sends deauthentication frames in a Wi-Fi network, disconnecting the victim from their trusted AP. This can be part of a larger attack or simply a way to cause disruption."},
            {"WPS (Wi-Fi Protected Setup) PIN Brute Force Attack", "Wi-Fi Protected Setup (WPS) was designed to simplify the setup of wireless networks. However, its PIN method is vulnerable to brute-force attacks. Attackers can use tools like Reaver to guess the WPS PIN, which then provides them with the WPA/WPA2 passphrase of the wireless network."}
        };
    }
}
