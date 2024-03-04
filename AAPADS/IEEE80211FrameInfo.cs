using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class FrameInfo
    {
        public string FrameIndex { get; set; }
        public string FrameType { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string FrameSize { get; set; }
        public string SequenceControlSequenceNumber { get; set; }
        public string SequenceControlFragmentNumber { get; set; }
    }
}
