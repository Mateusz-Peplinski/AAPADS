using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AAPADS
{
    /// <summary>
    /// Interaction logic for DetectionTrainingWindow.xaml
    /// </summary>
    public partial class DetectionTrainingWindow : Window
    {
        public DetectionTrainingWindow()
        {
            InitializeComponent();
        }

        // Method to update the timer display
        public void UpdateTimerDisplay(TimeSpan time)
        {
            TimerTextBox.Text = time.ToString(@"hh\:mm\:ss");
        }
    }
}
