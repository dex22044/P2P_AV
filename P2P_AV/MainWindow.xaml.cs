using MediaFoundation;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2P_AV
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int arole;
        int vrole;
        public static MainWindow current;
        static SettingsWindow settingsWin;

        Thread vidThread;
        Thread audThread;

        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public bool isFullscreen;
        public System.Windows.Forms.Screen currentScreen;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hookID = InterceptKeys.SetHook(_proc);
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)vkCode;

                if (MainWindow.current.isFullscreen)
                {
                    if (key == System.Windows.Forms.Keys.Scroll)
                    {
                        MainWindow.current.ExitFullscreenMode(null, null);
                    }
                }

                {
                    if (key == System.Windows.Forms.Keys.LWin
                     || key == System.Windows.Forms.Keys.RWin
                     || key == System.Windows.Forms.Keys.Scroll
                     || key == System.Windows.Forms.Keys.LShiftKey
                     || key == System.Windows.Forms.Keys.RShiftKey)
                    {
                        ControlsStreamer.keyboardButtonPressed(key);
                        return (IntPtr)1; // Handled.
                    }
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public MainWindow()
        {
            InitializeComponent();
            current = this;

            settingsWin = new SettingsWindow(true, this);

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach(NetworkInterface @interface in interfaces)
            {
                foreach (UnicastIPAddressInformation ip in @interface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        TextBlock block = new TextBlock
                        {
                            Text = ip.Address.ToString(),
                            Margin = new Thickness(3)
                        };
                        IPs.Children.Add(block);
                    }
                }
            }
        }

        private void SetAudioType(object sender, RoutedEventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            if ((string)btn.Content == "Server") arole = 1;
            if ((string)btn.Content == "Client") arole = 0;
        }

        private void StartAudio(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            //VideoStartBtn.IsEnabled = false;
            string aip = AudioIP.Text;
            string aport = settingsWin.Connection_AudioPort.Text;
            vidThread = new Thread(new ThreadStart(() =>
            {
                AudioStreamer.MainAsync(arole, 8192, $"{aip}:{aport}", Convert.ToInt32(aport));
            }));
            vidThread.IsBackground = false;
            vidThread.Priority = ThreadPriority.Highest;
            vidThread.Start();

            string vip = AudioIP.Text;
            string vport = settingsWin.Connection_VideoPort.Text;
            VideoStreamer.width = Convert.ToInt32(settingsWin.Compression_ImageWidth.Text);
            VideoStreamer.height = Convert.ToInt32(settingsWin.Compression_ImageHeight.Text);
            audThread = new Thread(new ThreadStart(() =>
            {
                VideoStreamer.MainAsync(arole, vip, Convert.ToInt32(vport));
            }));
            audThread.IsBackground = false;
            audThread.Priority = ThreadPriority.Highest;
            audThread.Start();

            string cport = settingsWin.Connection_ControlsPort.Text;
            ControlsStreamer.enabled = true;
            new Thread(new ThreadStart(() =>
            {
                ControlsStreamer.MainAsync(arole, vip, Convert.ToInt32(cport));
            })).Start();

            AudioIP.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _ = ((string)null).Length;
        }

        private void HideShowMenu(object sender, RoutedEventArgs e)
        {
            Button s = (Button)sender;
            if ((string)s.Content == "Hide")
            {
                s.Content = "Show";

                IPs.Height = 0;
                StackPanelMenu1.Height = 0;
                //StackPanelMenu2.Height = 0;
                //StackPanelMenu3.Height = 0;
            }
            else
            {
                s.Content = "Hide";

                IPs.Height = double.NaN;
                StackPanelMenu1.Height = double.NaN;
                //StackPanelMenu2.Height = double.NaN;
                //StackPanelMenu3.Height = double.NaN;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioStreamer.WaveOut.Volume = (float)e.NewValue;
        }

        private void Slider_ValueChanged2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VideoStreamer.encodeQuality = (long)e.NewValue;
        }

        private void VideoObject_KeyDown(object sender, KeyEventArgs e)
        {
            //KeyboardData.Text = e.Key.ToString();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            settingsWin.Show();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point global = e.GetPosition(VideoObject);
            ControlsStreamer.mousePositionChanged(global.X / VideoObject.ActualWidth, global.Y / VideoObject.ActualHeight);
        }

        private void VideoObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ControlsStreamer.mouseButtonPressed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.Space || e.Key == Key.Enter ||
                e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt || e.SystemKey == Key.Space || e.SystemKey == Key.Enter)
            {
                e.Handled = true;
                ControlsStreamer.keyboardButtonPressed(e);
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private void VideoObject_KeyDown_1(object sender, KeyEventArgs e)
        {
            ControlsStreamer.keyboardButtonPressed(e);
        }

        private void VideoObject_KeyUp(object sender, KeyEventArgs e)
        {
            ControlsStreamer.keyboardButtonReleased(e);
        }

        private void CheckBoxControlsEnable_Checked(object sender, RoutedEventArgs e)
        {
            ControlsStreamer.enabled = ((CheckBox)sender).IsChecked == true;
        }

        private void EnterFullscreenMode(object sender, RoutedEventArgs e)
        {
            BottomPanel.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Collapsed;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Topmost = true;
            isFullscreen = true;
        }

        public void ExitFullscreenMode(object sender, RoutedEventArgs e)
        {
            BottomPanel.Visibility = Visibility.Visible;
            TopPanel.Visibility = Visibility.Visible;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized;
            Topmost = false;
            isFullscreen = false;
        }
    }
}
