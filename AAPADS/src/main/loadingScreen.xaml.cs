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

            _mainWindow = new MainWindow();

            
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
