using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS.src.engine
{
    public class DetectionEngine
    {
        public List<string> CRITICALITY_LEVEL = new List<string>();
        public List<int> RISK_LEVEL = new List<int>();
        public List<string> DETECTION_STATUS = new List<string>();
        public event EventHandler DetectionDiscovered;

        public void startdetection()
        {

            populateDetectionViewModelData();
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);
        }

        public void populateDetectionViewModelData()
        {
            CRITICALITY_LEVEL.Add("LEVEL_5"); // CIRITICAL (RISK SCORE: 80 - 100)
            CRITICALITY_LEVEL.Add("LEVEL_4"); // HIGH (RISK SCORE: 60 - 80)
            CRITICALITY_LEVEL.Add("LEVEL_3"); // MEDIUM (RISK SCORE: 40 - 60)
            CRITICALITY_LEVEL.Add("LEVEL_2"); // LOW (RISK SCORE: 20 - 40)
            CRITICALITY_LEVEL.Add("LEVEL_1"); // INFO (RISK SCORE: 0 - 20)

            RISK_LEVEL.Add(95);
            RISK_LEVEL.Add(70);
            RISK_LEVEL.Add(57);
            RISK_LEVEL.Add(22);
            RISK_LEVEL.Add(15);

            DETECTION_STATUS.Add("NEW");
            DETECTION_STATUS.Add("IN-PROGRESS");
            DETECTION_STATUS.Add("CLOSED");
            DETECTION_STATUS.Add("CLOSED");
            DETECTION_STATUS.Add("CLOSED");
        }

    }
}
