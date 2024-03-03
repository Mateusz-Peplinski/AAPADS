using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AAPADS
{
    public class FrameInspectorViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private int _framesThisSecond = 0;
        public SeriesCollection SeriesCollection { get; set; }
        public AxesCollection AxisYCollection { get; set; }
        public AxesCollection AxisXCollection { get; set; }
        public ObservableCollection<FrameInfo> Frames { get; } = new ObservableCollection<FrameInfo>();

        private int _frameCount;
        public int FrameCount
        {
            get => _frameCount;
            set
            {
                _frameCount = value;
                OnPropertyChanged(nameof(FrameCount));
            }
        }


        public ICommand StartCaptureCommand { get; }
        public ICommand StopCaptureCommand { get; }

        public FrameInspectorViewModel()
        {
            StartCaptureCommand = new RelayCommand(StartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture);
            SeriesCollection = new SeriesCollection{
                new LineSeries
                {
                    Values = new ChartValues<int>(),
                    PointGeometry = null,
                    Stroke = new SolidColorBrush(Color.FromRgb(110, 204, 37)),    
                    LineSmoothness = 0,
                    PointGeometrySize = 0,
                    PointForeground =  new SolidColorBrush(Color.FromRgb(110, 204, 37)),
                    Fill = Brushes.Transparent
                }
            };

            AxisYCollection = new AxesCollection
            {
                new Axis
                {
                    Title = "Frames per Second",
                    MinValue = 0 
                }
            };

            AxisXCollection = new AxesCollection
            {
                new Axis
                {
                    Title = "Time (Seconds)",
                    MinValue = 0 
                }
            };

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
        }
        private void TimerOnTick(object sender, EventArgs e)
        {
            UpdateGraph(_framesThisSecond);
            _framesThisSecond = 0; 
        }
        public void OnFrameCaptured()
        {
            _framesThisSecond++;
            
            FrameCount++;
        }
        public void UpdateGraph(int newFramesCount)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var series = SeriesCollection[0];
                series.Values.Add(newFramesCount);

                if (series.Values.Count > 60)
                {
                    series.Values.RemoveAt(0);
                }

                OnPropertyChanged(nameof(SeriesCollection));
            });
        }


        private IList<string> GenerateLabels(int count)
        {
            return Enumerable.Range(1, count).Select(x => x.ToString()).ToList();
        }

        private void StartCapture()
        {
            _timer.Start();
            DataIngestEngineDot11Frames.Instance?.StartCaptureAsync();
        }

        private void StopCapture()
        {
            _timer.Stop();
            DataIngestEngineDot11Frames.Instance?.StopCapture();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand : ICommand
    {
        private Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }


}
