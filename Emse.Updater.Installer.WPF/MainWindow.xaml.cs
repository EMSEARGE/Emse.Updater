using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Emse.Updater.Installer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Main = this;
            Main._NavigationFrame.Navigate(new InstallWindowsService());
        }
        public ImageSource ApplicationIcon => BitmapFrame.Create(new Uri("EmseUpdaterICo.ico"));
    }
}
