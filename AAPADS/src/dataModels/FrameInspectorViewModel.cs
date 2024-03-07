using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AAPADS
{
    public class FrameInspectorViewModel : INotifyPropertyChanged
    {
        // The timer object used to keep track of how many frames are collected each second
        private readonly DispatcherTimer _timer;

        // The counter that is incremented everytime a frame is collected then reset every 1 second
        private int _framesThisSecond = 0;

        // The propertry change event handeler to update GUI 
        public event PropertyChangedEventHandler PropertyChanged;

        // The start frame capture command
        public ICommand StartCaptureCommand { get; }

        // The stop frame capture command
        public ICommand StopCaptureCommand { get; }

        // The series for frames collected per second
        public SeriesCollection SeriesCollection { get; set; }

        // y = frames
        public AxesCollection AxisYCollection { get; set; }

        // x = time (sec)
        public AxesCollection AxisXCollection { get; set; }

        // The collected of frames used to display frame data on the data grid
        public ObservableCollection<FrameInfo> Frames { get; } = new ObservableCollection<FrameInfo>();

        // The total frames captured count
        // Incremented eachtime a frame is captured
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

        public FrameInspectorViewModel()
        {
            // Bind the Start Capture Command to the StartCapture function
            StartCaptureCommand = new RelayCommand(StartCapture);

            // Bind the Stop Capture Command to the StopCapture function
            StopCaptureCommand = new RelayCommand(StopCapture);

            // Construct the time series graph
            FrameCountGraph();

            // Create the timer 
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
        }
        private void FrameCountGraph()
        {
            SeriesCollection = new SeriesCollection{
                new LineSeries
                {
                    Values = new ChartValues<int>(),
                    PointGeometry = null,
                    Stroke = new SolidColorBrush(Color.FromRgb(110, 204, 37)), // Green colour
                    LineSmoothness = 0,
                    PointGeometrySize = 0,
                    PointForeground =  new SolidColorBrush(Color.FromRgb(110, 204, 37)), // Green colour
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
        }
        private void TimerOnTick(object sender, EventArgs e)
        {
            // Every 1 second update the graph
            UpdateGraph(_framesThisSecond);

            // Reset the frame count every one second
            _framesThisSecond = 0;
        }
        public void OnFrameCaptured()
        {
            // increment the frames captured this second
            _framesThisSecond++;

            // Increment the total frames captured
            FrameCount++;
        }

        public void UpdateGraph(int newFramesCount)
        {
            // Invoke the GUI thread and update the graph
            Application.Current.Dispatcher.Invoke(() =>
            {
                var series = SeriesCollection[0];
                series.Values.Add(newFramesCount);

                // keep the full graph to display the past 1 min but keeping the value at 60 (60 seconds)
                if (series.Values.Count > 60)
                {
                    series.Values.RemoveAt(0);
                }

                OnPropertyChanged(nameof(SeriesCollection));
            });
        }

        private void StartCapture()
        {
            // Start the timer
            _timer.Start();

            // Call the capture thread in DataIngestEngineDot11Frames to start
            DataIngestEngineDot11Frames.Instance?.StartCaptureAsync();
        }

        private void StopCapture()
        {
            // Stop the timer
            _timer.Stop();

            // Call the capture thread in DataIngestEngineDot11Frames to stop
            DataIngestEngineDot11Frames.Instance?.StopCapture();
        }

        
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
