using System;
using System.Windows;
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
            START_AAPADS();
        }

        private DispatcherTimer _timer;
        private MainWindow _mainWindow;

        private void START_AAPADS()
        {

            _mainWindow = new MainWindow();

            //currently a time --> needs to map to an event in data ingest engine to say the scan has been preformed so the program does not load blank data
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            _mainWindow.Show();

            this.Close();
        }

    }
}
