using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;

namespace AAPADS
{
    /// <summary>
    /// Interaction logic for MonitorModeConfigurationWindow.xaml
    /// </summary>
    public partial class MonitorModeConfigurationWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<MONITOR_MODE_NETWORK_ADAPTER_INFO> MONITOR_MODE_NETWORK_ADAPTERS { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private MONITOR_MODE_NETWORK_ADAPTER_INFO _selectedAdapter;
        private string _defaultWLANName;
        public MONITOR_MODE_NETWORK_ADAPTER_INFO SelectedAdapter
        {
            get { return _selectedAdapter; }
            set
            {
                if (_selectedAdapter != value)
                {
                    _selectedAdapter = value;
                    OnPropertyChanged(nameof(SelectedAdapter));

                    // When the selection changes, update DefaultWNICName
                    SELECTED_NETWORK_ADAPTER = value?.MONITOR_MODE_NETWORK_ADAPTER_NAME;
                }
            }
        }
        public string SELECTED_NETWORK_ADAPTER
        {
            get { return _defaultWLANName; }
            set
            {
                if (_defaultWLANName != value)
                {
                    _defaultWLANName = value;
                    OnPropertyChanged(nameof(SELECTED_NETWORK_ADAPTER));
                }
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MonitorModeConfigurationWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            MONITOR_MODE_NETWORK_ADAPTERS = new ObservableCollection<MONITOR_MODE_NETWORK_ADAPTER_INFO>();
            MONITOR_MODE_NETWORK_ADAPTER_INFO.AdapterCollection = MONITOR_MODE_NETWORK_ADAPTERS;
            LoadAdapters();
        }
        private void LoadAdapters()
        {
            var adapters = GetNetworkAdapters();
            foreach (var adapter in adapters)
            {
                MONITOR_MODE_NETWORK_ADAPTERS.Add(adapter);
            }
        }
        private void SaveSelectedMonitorModeAdapterSetting(string adapterName)
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                db.SaveSetting("MonitorModeWNICName", adapterName);
            }
        }
        private void SetMonitorMode_Click(object sender, RoutedEventArgs e)
        {
            SetMonitorMode(SELECTED_NETWORK_ADAPTER);
        }
        private void SetManagedMode_Click(object sender, RoutedEventArgs e)
        {
            SetManagedMode(SELECTED_NETWORK_ADAPTER);
        }
        public List<MONITOR_MODE_NETWORK_ADAPTER_INFO> GetNetworkAdapters()
        {
            List<MONITOR_MODE_NETWORK_ADAPTER_INFO> adapterList = new List<MONITOR_MODE_NETWORK_ADAPTER_INFO>();

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    adapterList.Add(new MONITOR_MODE_NETWORK_ADAPTER_INFO
                    {
                        MONITOR_MODE_NETWORK_ADAPTER_NAME = adapter.Name,
                        MONITOR_MODE_NETWORK_ADAPTER_ID = adapter.Id,
                        MONITOR_MODE_NETWORK_ADAPTER_DESCRIPTION = adapter.Description,
                        MONITOR_MODE_NETWORK_ADAPTER_STATUS = adapter.OperationalStatus.ToString(),
                        MONITOR_MODE_NETWORK_ADAPTER_SPEED_BYTES = adapter.Speed,
                        MONITOR_MODE_NETWORK_ADAPTER_MAC_ADDRESS = adapter.GetPhysicalAddress().ToString()
                    });
                }
            }

            return adapterList;
        }
        // Minimise the Window
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Maximise the Window
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        //Close the window
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public bool SetMonitorMode(string adapterName)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "WlanHelper.exe",
                    Arguments = $"\"{adapterName}\" mode monitor",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    // Read the output to ensure the command was successful
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Check if the output contains the success message
                    if (output.Contains("Success"))
                    {
                        SaveSelectedMonitorModeAdapterSetting(adapterName);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("MONITOR MODE SET SUCCESSFULLY.");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FAILED TO SET MONITOR MODE. ERROR: " + output);
                        Console.WriteLine("TRY RUNNING PROGRAM WITH ADMINISTRATOR PRIVILEGES");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR SETTING MONITOR MODE: {ex.Message}");
                return false;
            }
        }
        public bool SetManagedMode(string adapterName)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "WlanHelper.exe",
                    Arguments = $"\"{adapterName}\" mode managed",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    // Read the output to ensure the command was successful
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Check if the output contains the success message
                    if (output.Contains("Success"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("MANAGED MODE SET SUCCESSFULLY.");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FAILED TO SET MANAGED MODE. ERROR: " + output);
                        Console.WriteLine("TRY RUNNING PROGRAM WITH ADMINISTRATOR PRIVILEGES");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR SETTING MONITOR MODE: {ex.Message}");
                return false;
            }
        }
    }
    public class MONITOR_MODE_NETWORK_ADAPTER_INFO : INotifyPropertyChanged
    {
        public string MONITOR_MODE_NETWORK_ADAPTER_NAME { get; set; }
        public string MONITOR_MODE_NETWORK_ADAPTER_ID { get; set; }
        public string MONITOR_MODE_NETWORK_ADAPTER_DESCRIPTION { get; set; }
        public string MONITOR_MODE_NETWORK_ADAPTER_STATUS { get; set; }
        public long MONITOR_MODE_NETWORK_ADAPTER_SPEED_BYTES { get; set; }
        public string MONITOR_MODE_NETWORK_ADAPTER_MAC_ADDRESS { get; set; }

        public static ObservableCollection<MONITOR_MODE_NETWORK_ADAPTER_INFO> AdapterCollection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


