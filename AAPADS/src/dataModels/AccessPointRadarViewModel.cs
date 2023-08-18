using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public AccessPointRadarViewModel() {
            StartRadarRotation();
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
