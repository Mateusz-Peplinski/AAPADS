using AAPADS.src.engine;
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
   
    public partial class MainWindow : Window
    {
        private readonly DataIngestEngine dataIngestionEngineObject;
        private readonly DetectionEngine detectionEngineObject;

        private readonly overviewViewDataModel overviewViewDisplay;

        private readonly detectionsViewDataModel detectionsDisplay;

        private readonly networkAdapterInformationDataModel networkAdapterInformationDisplay;

        public MainWindow()
        {
            InitializeComponent();

            dataIngestionEngineObject = new DataIngestEngine();
            overviewViewDisplay = new overviewViewDataModel();
            dataIngestionEngineObject.SSIDDataCollected += UpdateOverviewTabUI;
            dataIngestionEngineObject.Start();

            detectionEngineObject = new DetectionEngine();
            detectionsDisplay = new detectionsViewDataModel();
            detectionEngineObject.DetectionDiscovered += updateDetectionTabUI;
            detectionEngineObject.startdetection();

            DataContext = overviewViewDisplay;

            networkAdapterInformationDisplay = new networkAdapterInformationDataModel();


        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void EXIT_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void overviewTab_Click(object sender, RoutedEventArgs e)
        {
            DataContext = overviewViewDisplay;
        }

        private void detectionsTab_Click(object sender, RoutedEventArgs e)
        {
            DataContext = detectionsDisplay;

            
        }
        private void NetworkAdapter_Click(object sender, RoutedEventArgs e)
        {
            DataContext = networkAdapterInformationDisplay;
            
        }

        private void UpdateOverviewTabUI(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                overviewViewDisplay.UpdateAccessPoints(dataIngestionEngineObject);
            });
        }
        private void updateDetectionTabUI(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                detectionsDisplay.updateDetections(detectionEngineObject);
            });
        }

    }
}
