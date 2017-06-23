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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emse.Updater.Helper;

namespace Emse.Updater.Installer.WPF
{
   
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            string cmdLine = @"/C C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe " + PathHelper.GetRealPath() + JsonHelper.JsonReader().ExeName;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            //TODO Ask for administrator privileges
            if (RadioButtonWindowsService.IsChecked == true)
            {
                MainWindow.Main._NavigationFrame.Navigate(new InstallWindowsService());
            }
        }
    }
}
