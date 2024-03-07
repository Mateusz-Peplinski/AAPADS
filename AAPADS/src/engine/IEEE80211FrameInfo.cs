using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class FrameInfo
    {
        public string FrameIndex { get; set; } // The count of the frame since collection started
        public string FrameType { get; set; } // The IEEE802.11 Frame Type
        public string Source { get; set; } // The source MAC address of the frame
        public string Destination { get; set; } // The destination MAC address of the frame
        public string FrameSize { get; set; } // The size of the 802.11 Frame in bytes
        public string SequenceControlSequenceNumber { get; set; } // The sequence number is used to keep track of each frame that is sent between a transmitter and receiver 
        public string SequenceControlFragmentNumber { get; set; } // This is a 4-bit field that is used when a large packet is broken down into smaller fragments for transmission. 
    }
}
