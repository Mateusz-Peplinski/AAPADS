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
    /// Interaction logic for loadingScreen.xaml
    /// </summary>
    public partial class loadingScreen : Window
    {
        public loadingScreen()
        {
            InitializeComponent();
            StartLoadingProcess();
        }

        private DispatcherTimer _timer;
        private MainWindow _mainWindow;

        private void StartLoadingProcess()
        {
            // Initialize the main window but don't show it yet
            _mainWindow = new MainWindow();

            // Set up the timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Wait for 5 seconds
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            _timer.Stop();

            // Show the main window
            _mainWindow.Show();

            // Close the loading window
            this.Close();
        }

    }
}
