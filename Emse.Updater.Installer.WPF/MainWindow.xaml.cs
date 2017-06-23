using System.Windows;
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
    }
}
