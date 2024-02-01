using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class DetectionEvent
    {
        public int ID { get; set; } // Auto-incremented by the database
        public string CriticalityLevel { get; set; }
        public int RiskLevel { get; set; }
        public string DetectionStatus { get; set; }
        public string DetectionTime { get; set; }
        public string DetectionTitle { get; set; }
        public string DetectionDescription { get; set; }
        public string DetectionRemediation { get; set; }
        public string DetectionAccessPointSsid { get; set; }
        public string DetectionAccessPointMacAddress { get; set; }
        public string DetectionAccessPointSignalStrength { get; set; }
        public string DetectionAccessPointOpenChannel { get; set; }
        public string DetectionAccessPointFrequency { get; set; }
        public string DetectionAccessPointIsStillActive { get; set; }
        public string DetectionAccessPointTimeFirstDetected { get; set; } 
        public string DetectionAccessPointEncryption { get; set; }
        public string DetectionAccessPointConnectedClients { get; set; }

    }
}
