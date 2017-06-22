using System;
using System.Windows;
using System.Windows.Controls;

namespace Emse.Updater.Installer.WPF
{
    public partial class InstallWindowsService : Page
    {
        public InstallWindowsService()
        {
            InitializeComponent();
            GridWindowsInstallerWellcome.Visibility = Visibility.Visible;
        }
        private void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridWindowsInstallerWellcome.Visibility = Visibility.Hidden;
            InstallWebService();
            
            Environment.Exit(1);
        }
        private void InstallWebService()
        {
            DownloadUpdaterDeploy();
            InstallUpdaterDeploy();
            InstallUpdaterWindowsService();
            OpenUpdaterSettings();
        }
        private void DownloadUpdaterDeploy()
        {
           //TODO Download deploy the temp file
        }
        private void InstallUpdaterDeploy()
        {
          //TODO install deploy to the target dir
        }

        private void InstallUpdaterWindowsService()
        {
          //TODO exec cmd line to install service
        }
        private void OpenUpdaterSettings()
        {
           //TODO start Emse.Updater.Settings.WPF
        }
    }
}
