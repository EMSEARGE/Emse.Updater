using MahApps.Metro.Controls;

namespace Emse.Updater.Installer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Main { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Main = this;
            
            Main._NavigationFrame.Navigate(new Dashboard());
        }
    }
}
