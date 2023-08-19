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

namespace AAPADS
{
    public class AccessPointRadarViewModel : INotifyPropertyChanged
    {
        private double _rotationAngle;
        private CancellationTokenSource _cts;
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

        public ObservableCollection<UIElement> AccessPoints { get; set; } = new ObservableCollection<UIElement>();

        public void AddAccessPoint(string accessPointId, int RSSI)
        {
            double normalizedRSSI = 1 - (RSSI + 100) / 100.0; // This will give 1 for -100 and 0 for 0
            double r = normalizedRSSI * 200;

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
            Canvas.SetTop(ellipse, y - 2.5);  // Adjusting for ellipse size
            Canvas.SetZIndex(ellipse, 1);     // Ensure the ellipse is on top

            AccessPoints.Add(ellipse);
        }


        public void RemoveAccessPoint(string accessPointId)
        {
            // Assuming you have a way to identify which ellipse corresponds to which access point
            // For this example, I'm just removing the first ellipse
            if (AccessPoints.Count > 0)
            {
                AccessPoints.RemoveAt(0);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public AccessPointRadarViewModel()
        {
            StartRadarRotation();
            AddAccessPoint("SSID 1", -50);
            AddAccessPoint("SSID 2", -100);
            AddAccessPoint("SSID 3", -80);
            AddAccessPoint("SSID 4", -30);
            AddAccessPoint("SSID 4", -10);
            AddAccessPoint("SSID 4", -20);
            AddAccessPoint("SSID 4", -30);
            AddAccessPoint("SSID 4", -40);
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
