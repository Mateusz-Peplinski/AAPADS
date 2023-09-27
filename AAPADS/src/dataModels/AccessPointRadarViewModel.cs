using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace AAPADS
{
    public class AccessPointRadarViewModel : INotifyPropertyChanged
    {
        private double _rotationAngle;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _radarCts;

        public event PropertyChangedEventHandler PropertyChanged;
        public double RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                _rotationAngle = value;
                OnPropertyChanged("RotationAngle");
            }
        }
        private Dictionary<string, Ellipse> accessPoints = new Dictionary<string, Ellipse>();
        private Random rand = new Random();

        public ObservableCollection<UIElement> ACCESS_POINTS { get; set; } = new ObservableCollection<UIElement>();

        public void AddAccessPoint(string accessPointId, int RSSI)
        {
            double normalizedRSSI = RSSI; 
            double r = normalizedRSSI;

            double theta = rand.NextDouble() * 2 * Math.PI;
            double x = 200 + r * Math.Cos(theta);
            double y = 200 + r * Math.Sin(theta);

            Ellipse ellipse = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Red, 0.0),
                        new GradientStop(Colors.Transparent, 1.0)
                    }
                },
                ToolTip = $"{accessPointId}\nRSSI: {RSSI}"
            };
            
            Canvas.SetLeft(ellipse, x - 2.5); // Adjusting for ellipse size
            Canvas.SetTop(ellipse, y - 2.5);  
            //Canvas.SetZIndex(ellipse, 1);     // ellipse is on top

            ACCESS_POINTS.Add(ellipse);
        }


        public void RemoveAccessPoint()
        {
            // Assuming you have a way to identify which ellipse corresponds to which access point
            // For this example, I'm just removing the first ellipse
            ACCESS_POINTS.Clear();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public AccessPointRadarViewModel()
        {
            StartRadarRotation();
            
        }

        public async Task PopulateRadar(DataIngestEngine dataIngestEngine)
        {
            _radarCts = new CancellationTokenSource();
            while (!_radarCts.Token.IsCancellationRequested)
            {
                var ssidListCopy = dataIngestEngine.SSID_LIST.ToList();

                for (int i = 0; i < ssidListCopy.Count; i++)
                {
                    AddAccessPoint(ssidListCopy[i], (dataIngestEngine.SIGNAL_STRENGTH_LIST[i] / 2) - 100);
                }

                await Task.Delay(5000); // Delay for 1 second or adjust as needed
                //RemoveAccessPoint();
            }
        }
        public void StopRadarPopulation()
        {
            _radarCts?.Cancel();
        }

        public void StartRadarRotation()
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RotationAngle += 5;
                        if (RotationAngle >= 360) RotationAngle = 0;
                    });

                    await Task.Delay(50);
                }
            });
        }

        public void StopRadarRotation()
        {
            _cts?.Cancel();
        }
    }
}
