using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace AAPADS
{
    /// <summary>
    /// Interaction logic for loadingScreen.xaml
    /// </summary>
    public partial class loadingScreen : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        public loadingScreen()
        {
            InitializeComponent();
            AllocConsole(); // The debug console is created now to give the console window time to initialize before calling the Console.Writeline();
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
