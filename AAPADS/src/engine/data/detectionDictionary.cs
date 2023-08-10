using System.Collections.Generic;

namespace AAPADS
{
    public class WirelessAttackDatabase
    {
        public Dictionary<string, WirelessAttackDetails> wirelessAttacks = new Dictionary<string, WirelessAttackDetails>
        {
            {"Eavesdropping (Sniffing)",
                new WirelessAttackDetails
        {
            Description = "An attacker passively intercepts and logs wireless traffic, usually to capture sensitive information like passwords or session tokens. This is possible because radio frequencies (RF) can be easily intercepted.",
            Remediation = "Disconnect sensitive systems from the wireless network. Switch to a wired connection for critical activities. Review and upgrade network encryption protocols, such as transitioning from WPA2 to WPA3. Educate users to avoid transmitting sensitive information until the issue is fully addressed"
        }
             },
            {"Man-in-the-Middle Attack (MitM)",
                new WirelessAttackDetails
        {
            Description = "The attacker intercepts communications between two parties and relays messages between them, making them believe they're talking directly to each other. This allows the attacker to manipulate the conversation or gather sensitive data.",
            Remediation = "Disconnect affected clients and force them to re-authenticate. Invalidate active sessions on critical systems. Confirm that network infrastructure devices, such as routers and gateways, are legitimate and have not been replaced or manipulated by attackers. Rotate credentials and ensure HTTPS is enforced on all internal web platforms."
        }
            },
            {"Rogue Access Point",
                new WirelessAttackDetails
        {
            Description = "An attacker sets up an unauthorized wireless access point (often with a similar SSID to a legitimate network) to lure users into connecting. Once users are connected, their data can be intercepted or they can be subjected to MitM attacks.",
            Remediation = "Immediate Remediation: Using a network scanning tool, locate and physically disconnect the rogue AP. Reauthenticate all devices on the network. Inform users to forget the rogue network SSID from their devices and change their passwords, especially if they had connected to the rogue AP."
        }
            },
            {"Deauthentication Attack",
                new WirelessAttackDetails
        {
            Description = "This is an attack against the Wi-Fi Protected Access (WPA) and WPA2 protocols. An attacker sends deauthentication frames in a Wi-Fi network, disconnecting the victim from their trusted AP. This can be part of a larger attack or simply a way to cause disruption.",
            Remediation = "Monitor wireless traffic for the source of deauthentication frames and block or isolate malicious sources. Temporarily reduce the range of the Wi-Fi network to minimize external interference, if feasible. Consider changing the Wi-Fi channel or frequency."
        }
            },
            {"WPS (Wi-Fi Protected Setup) PIN Brute Force Attack",
                new WirelessAttackDetails
        {
            Description = "Wi-Fi Protected Setup (WPS) was designed to simplify the setup of wireless networks. However, its PIN method is vulnerable to brute-force attacks. Attackers can use tools like Reaver to guess the WPS PIN, which then provides them with the WPA/WPA2 passphrase of the wireless network.",
            Remediation = "Immediately disable WPS on all access points. Reset and change the WPS PIN, even if you plan to keep it disabled. Inform users to change their device passwords if they have used WPS to connect."
        }
            },

        };
    }


    public class WirelessAttackDetails
    {
        public string Description { get; set; }
        public string Remediation { get; set; }

        

    }



}
