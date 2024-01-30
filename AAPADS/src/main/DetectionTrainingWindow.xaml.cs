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
using System.Windows.Threading;

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

        // The Cancel button click event that will abort detection traning
        private void AbortDetectionTraning_Click(object sender, RoutedEventArgs e)
        {
            // Confirm with the user they really want to exit
            var result = MessageBox.Show("Are you sure you want to cancel Detection Traning", "AAPADS - CONFIRMATION",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            switch (result)
            {
                case MessageBoxResult.Yes:

                    SetDetectionTrainingFlag(false); // Setting the TrainingFlag to false will globally abort the traning phase

                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        // Minimise the Window
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Maximise the Window
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        
        //Close the window
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Set the Training Flag status
        private void SetDetectionTrainingFlag(bool FlagStatus)
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Convert the boolean to a string representation and save it.
                db.SaveSetting("TrainingFlag", FlagStatus.ToString());
            }
        }
    }
}
