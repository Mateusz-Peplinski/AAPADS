using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AAPADS
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        private bool _isDebugConsoleEnabled;

        public bool IsDebugConsoleEnabled
        {
            get { return _isDebugConsoleEnabled; }
            set
            {
                if (_isDebugConsoleEnabled != value)
                {
                    _isDebugConsoleEnabled = value;

                    if (_isDebugConsoleEnabled)
                        ShowConsole();
                    else
                        HideConsole();

                    OnPropertyChanged(nameof(IsDebugConsoleEnabled));
                    SaveIsDebugConsoleEnabled();
                }
            }
        }

        public void LoadSettings()
        {
            LoadIsDebugConsoleEnabled();
        }
        private void LoadIsDebugConsoleEnabled()
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // GetSetting should return the string representation of the setting.
                var settingValue = db.GetSetting("DebugConsoleEnabled");

                // Parse the returned value to a boolean.
                if (settingValue != null)
                {
                    IsDebugConsoleEnabled = settingValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }

            if (IsDebugConsoleEnabled)
                ShowConsole();
            else
                HideConsole();
        }

        private void SaveIsDebugConsoleEnabled()
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Convert the boolean to a string representation and save it.
                db.SaveSetting("DebugConsoleEnabled", IsDebugConsoleEnabled ? "true" : "false");
            }
        }

        private void ShowConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);
        }

        private void HideConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

    }

}
