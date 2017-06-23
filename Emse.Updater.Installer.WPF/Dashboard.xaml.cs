using System.Windows;
using System.Windows.Controls;

namespace Emse.Updater.Installer.WPF
{
   
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonWindowsService.IsChecked == true)
            {
                MainWindow.Main._NavigationFrame.Navigate(new InstallWindowsService());
            }
        }
    }
}
