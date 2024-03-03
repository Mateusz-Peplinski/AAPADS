using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AAPADS
{
    public class FrameInspectorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FrameInfo> Frames { get; } = new ObservableCollection<FrameInfo>();


        public ICommand StartCaptureCommand { get; }
        public ICommand StopCaptureCommand { get; }

        public FrameInspectorViewModel()
        {
            StartCaptureCommand = new RelayCommand(StartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture);
        }

        private void StartCapture()
        {
            // Logic to start capture
            DataIngestEngineDot11Frames.Instance?.StartCaptureAsync();
        }

        private void StopCapture()
        {
            // Logic to stop capture
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
