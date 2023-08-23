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
    /// Interaction logic for AccessPointRadarWindow.xaml
    /// </summary>
    public partial class AccessPointRadarWindow : Window
    {
        private double _originalWidth;
        private double _originalHeight;
        private double _originalLeft;
        private double _originalTop;
        private bool _wasMaximized = false;
        private AccessPointRadarViewModel radarViewModel;
        public AccessPointRadarWindow(DataIngestEngine dataIngestEngine)
        {
            InitializeComponent();
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;

            MaximizeButton.Click += (s, e) =>
            {
                if (!_wasMaximized)
                {

                    _originalWidth = Width;
                    _originalHeight = Height;
                    _originalLeft = Left;
                    _originalTop = Top;

                    WindowState = WindowState.Normal;
                    Left = 0;
                    Top = 0;
                    Width = SystemParameters.WorkArea.Width;
                    Height = SystemParameters.WorkArea.Height;

                    _wasMaximized = true;
                }
                else
                {
                    Width = _originalWidth;
                    Height = _originalHeight;
                    Left = _originalLeft;
                    Top = _originalTop;

                    _wasMaximized = false;
                }
            };

            CloseButton.Click += (s, e) => {
                radarViewModel.StopRadarPopulation();
                radarViewModel.StopRadarRotation();
                this.Close();   
            };

            LoadRadar(dataIngestEngine);
        }

        private void LoadRadar(DataIngestEngine dataIngestEngine)
        {
            radarViewModel = new AccessPointRadarViewModel();
            Task.Run(() => radarViewModel.PopulateRadar(dataIngestEngine));
            DataContext = radarViewModel;


        }

    }
    }

