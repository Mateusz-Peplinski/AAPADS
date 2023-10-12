using AAPADS.src.engine;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace AAPADS
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        private readonly DataIngestEngine DATA_INGESTION_ENGINE_OBJECT;
        private readonly DetectionEngine DETECTION_ENGINE_OBJECT;

        private readonly overviewViewDataModel OVERVIEW_VIEW_MODEL_OBJECT;
        private readonly detectionsViewDataModel DETECTION_VIEW_MODEL_OBJECT;
        private readonly detectionSetUpViewDataModel WLAN_NETWORK_ADAPTER_VIEW_MODEL_OBJECT;
        private AccessPointInvestigatorDataModel ACCESS_POINT_INVESTIGATOR_VIEW_MODEL_OBJECT;
        public NetworkCardInfoViewModel NetworkCardInfoVM { get; set; }

        private double _originalWidth;
        private double _originalHeight;
        private double _originalLeft;
        private double _originalTop;
        private bool _wasMaximized = false;

        public MainWindow()
        {
            InitializeComponent();

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            DATA_INGESTION_ENGINE_OBJECT = new DataIngestEngine();
            OVERVIEW_VIEW_MODEL_OBJECT = new overviewViewDataModel();
            DATA_INGESTION_ENGINE_OBJECT.SSIDDataCollected += UpdateOverviewTabUI;
            DATA_INGESTION_ENGINE_OBJECT.START_DATA_INGEST_ENGINE();

            DETECTION_ENGINE_OBJECT = new DetectionEngine();
            DETECTION_VIEW_MODEL_OBJECT = new detectionsViewDataModel();
            DETECTION_ENGINE_OBJECT.DetectionDiscovered += updateDetectionTabUI;
            DETECTION_ENGINE_OBJECT.START_DETECTION_ENGINE();

            WLAN_NETWORK_ADAPTER_VIEW_MODEL_OBJECT = new detectionSetUpViewDataModel();

            NetworkCardInfoVM = new NetworkCardInfoViewModel();
            NetworkCardInfoExpander.DataContext = NetworkCardInfoVM;


            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;

            MaximizeButton.Click += (s, e) =>
            {
                if (!_wasMaximized)
                {

                    _originalWidth = Width;
                    _originalHeight = Height;
                    _originalLeft = Left;
                    _originalTop = Top;

                    OVERVIEW_VIEW_MODEL_OBJECT.SummarySectionHeight = 500;
                    WindowState = WindowState.Normal;
                    Left = 0;
                    Top = 0;
                    Width = SystemParameters.WorkArea.Width;
                    Height = SystemParameters.WorkArea.Height;

                    _wasMaximized = true;
                }
                else
                {
                    OVERVIEW_VIEW_MODEL_OBJECT.SummarySectionHeight = 300;
                    Width = _originalWidth;
                    Height = _originalHeight;
                    Left = _originalLeft;
                    Top = _originalTop;

                    _wasMaximized = false;
                }
            };

            CloseButton.Click += (s, e) => Application.Current.Shutdown();

            DataContext = OVERVIEW_VIEW_MODEL_OBJECT;



        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                    OVERVIEW_VIEW_MODEL_OBJECT.SummarySectionHeight = 500;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    OVERVIEW_VIEW_MODEL_OBJECT.SummarySectionHeight = 300;
                }
            }
            else
            {
                DragMove();
            }
        }


        private void About_Click(object sender, RoutedEventArgs e)
        {
            about AboutPage = new about();
            AboutPage.Show();
        }
        private void AccessPointRadar_Click(object sender, RoutedEventArgs e)
        {
            AccessPointRadarWindow accessPointRadarWindow = new AccessPointRadarWindow(DATA_INGESTION_ENGINE_OBJECT);
            accessPointRadarWindow.Show();
        }
        private void EXIT_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var tab = e.AddedItems[0] as TabItem;

            if (tab == null) return;

            if (tab == OverviewTab)
            {
                DataContext = OVERVIEW_VIEW_MODEL_OBJECT;
            }
            else if (tab == DetectionsTab)
            {
                DataContext = DETECTION_VIEW_MODEL_OBJECT;
            }
            else if (tab == AccessPointInvestigateTab)
            {
                ACCESS_POINT_INVESTIGATOR_VIEW_MODEL_OBJECT = new AccessPointInvestigatorDataModel();
                DataContext = ACCESS_POINT_INVESTIGATOR_VIEW_MODEL_OBJECT;
            }
            else if (tab == DetectionSetupTab)
            {
                DataContext = WLAN_NETWORK_ADAPTER_VIEW_MODEL_OBJECT;
            }
        }
        private async void UpdateOverviewTabUI(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                await OVERVIEW_VIEW_MODEL_OBJECT.UpdateAccessPoints(DATA_INGESTION_ENGINE_OBJECT);
            });

        }
        private void updateDetectionTabUI(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                DETECTION_VIEW_MODEL_OBJECT.updateDetections(DETECTION_ENGINE_OBJECT);
            });
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }
        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb != null)
            {
                Clipboard.SetText(tb.Text);
            }
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();

            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }

            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
