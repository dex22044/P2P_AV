using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace P2P_AV
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    /// 

    public partial class SettingsWindow : Window
    {
        BrushConverter brushConverter = new BrushConverter();
        MainWindow parent;

        public SettingsWindow(bool isDarkTheme, MainWindow win)
        {
            InitializeComponent();
            parent = win;
            if (isDarkTheme)
            {
                Common_DarkThemeRadioButton.IsChecked = true;
                setDarkTheme();
            }
            else
            {
                Common_LightThemeRadioButton.IsChecked = true;
                setLightTheme();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Common_ChangedTheme(object sender, RoutedEventArgs e)
        {
            if (Common_LightThemeRadioButton.IsChecked == true)
            {
                setLightTheme();
            }
            else
            {
                setDarkTheme();
            }
        }

        void setDarkTheme()
        {
            Background = brushConverter.ConvertFrom("#565656") as Brush;
            MainTabControl.Background = brushConverter.ConvertFrom("#353535") as Brush;

            parent.Background = brushConverter.ConvertFrom("#565656") as Brush;
            parent.AudioIP.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;
            parent.AudioIP.Foreground = brushConverter.ConvertFrom("#D0D0D0") as Brush;

            parent.AudioType_Client_RadioButton.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;
            parent.AudioType_Server_RadioButton.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;

            parent.SoundStartBtn.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;
            parent.SoundStartBtn.Foreground = brushConverter.ConvertFrom("#D0D0D0") as Brush;

            parent.HideBtn.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;
            parent.HideBtn.Foreground = brushConverter.ConvertFrom("#D0D0D0") as Brush;

            parent.EnterFullscreenBtn.Background = brushConverter.ConvertFrom("#2E2E2E") as Brush;
            parent.EnterFullscreenBtn.Foreground = brushConverter.ConvertFrom("#D0D0D0") as Brush;
        }

        void setLightTheme()
        {
            Background = brushConverter.ConvertFrom("#ECECEC") as Brush;
            MainTabControl.Background = brushConverter.ConvertFrom("#D3D3D3") as Brush;

            parent.Background = brushConverter.ConvertFrom("#ECECEC") as Brush;
            parent.AudioIP.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;
            parent.AudioIP.Foreground = brushConverter.ConvertFrom("#2E2E2E") as Brush;

            parent.AudioType_Client_RadioButton.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;
            parent.AudioType_Server_RadioButton.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;

            parent.SoundStartBtn.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;
            parent.SoundStartBtn.Foreground = brushConverter.ConvertFrom("#2E2E2E") as Brush;

            parent.HideBtn.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;
            parent.HideBtn.Foreground = brushConverter.ConvertFrom("#2E2E2E") as Brush;

            parent.EnterFullscreenBtn.Background = brushConverter.ConvertFrom("#D0D0D0") as Brush;
            parent.EnterFullscreenBtn.Foreground = brushConverter.ConvertFrom("#2E2E2E") as Brush;
        }
    }
}
