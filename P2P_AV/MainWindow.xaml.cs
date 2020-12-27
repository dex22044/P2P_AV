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
            new Thread(new ThreadStart(() =>
            {
                AudioStreamer.MainAsync(arole, 8192, $"{aip}:{aport}", Convert.ToInt32(aport));
            })).Start();

            string vip = AudioIP.Text;
            string vport = settingsWin.Connection_VideoPort.Text;
            VideoStreamer.width = Convert.ToInt32(settingsWin.Compression_ImageWidth.Text);
            VideoStreamer.height = Convert.ToInt32(settingsWin.Compression_ImageHeight.Text);
            new Thread(new ThreadStart(() =>
            {
                VideoStreamer.MainAsync(arole, vip, Convert.ToInt32(vport));
            })).Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((Button)null).Height = 0; //TODO: Normal exit
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

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(VideoObject);

            //MouseData.Text = $"{position.X} {position.Y} {e.LeftButton} {e.RightButton} {e.MiddleButton}";
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            settingsWin.Show();
        }
    }
}
