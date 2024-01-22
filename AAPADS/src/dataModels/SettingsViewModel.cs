using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AAPADS
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
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
                    SaveSettings();
                }
            }
        }

        public void LoadSettings()
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

            // Based on the boolean value, show or hide the console.
            if (IsDebugConsoleEnabled)
                ShowConsole();
            else
                HideConsole();
        }

        public void SaveSettings()
        {
            using (var db = new SettingsDatabaseAccess("wireless_profile.db"))
            {
                // Convert the boolean to a string representation and save it.
                db.SaveSetting("DebugConsoleEnabled", IsDebugConsoleEnabled ? "true" : "false");
            }
        }

        public void ShowConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);
        }

        public void HideConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

    }

}
