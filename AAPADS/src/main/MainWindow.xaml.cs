using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AAPADS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DataIngestEngine engine;
        private readonly overviewViewDataModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            engine = new DataIngestEngine();
            viewModel = new overviewViewDataModel();
            engine.SSIDDataCollected += UpdateUI;
            engine.Start();

            DataContext = viewModel;

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            about aboutWindow = new about();
            aboutWindow.Show();
        }

        private void EXIT_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void overviewTab_Click(object sender, RoutedEventArgs e)
        {
            DataContext = viewModel;
        }

        private void detectionsTab_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new detectionsViewDataModel();
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Here's where you update your UI. 
            // Make sure to do it on the UI thread
            this.Dispatcher.Invoke(() =>
            {
                viewModel.UpdateAccessPoints(engine);
            });
        }

    }
}
