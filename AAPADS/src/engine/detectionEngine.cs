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
        public event EventHandler DetectionDiscovered;

        public void startdetection()
        {

            populateDetectionViewModelData();
            DetectionDiscovered?.Invoke(this, EventArgs.Empty);
        }

        public void populateDetectionViewModelData()
        {
            CRITICALITY_LEVEL.Add("LEVEL5"); // CIRITICAL (RISK SCORE: 80 - 100)
            CRITICALITY_LEVEL.Add("LEVEL4"); // HIGH (RISK SCORE: 60 - 80)
            CRITICALITY_LEVEL.Add("LEVEL3"); // MEDIUM (RISK SCORE: 40 - 60)
            CRITICALITY_LEVEL.Add("LEVEL2"); // LOW (RISK SCORE: 20 - 40)
            CRITICALITY_LEVEL.Add("LEVEL1"); // INFO (RISK SCORE: 0 - 20)
        }

    }
}
