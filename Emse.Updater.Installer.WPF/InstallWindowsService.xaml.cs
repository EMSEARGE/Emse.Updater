using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Emse.Updater.Installer.WPF
{
    public partial class InstallWindowsService : Page
    {
        public static string GlobalUrl = "http://191.235.157.215";
        public static string Url;
        public static bool DownloadingVersionZip;
        public static Tuple<DateTime, long, long> DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.MinValue, 0, 0);
        private static readonly WebClient Wc = new WebClient();
        public static Process Program { get; set; }
        public InstallWindowsService()
        {
            InitializeComponent();
            GridWindowsInstallerWellcome.Visibility = Visibility.Visible;
            RadioButtonGlobal.IsChecked = true;
            TextBoxCustomUrl.Visibility = Visibility.Hidden;
        }
        private void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (RadioButtonLocal.IsChecked == true)
            {
                Uri uriResult;
                bool result = Uri.TryCreate(TextBoxCustomUrl.Text, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    GridWindowsInstallerWellcome.Visibility = Visibility.Hidden;
                    GridInstallProgress.Visibility = Visibility.Visible;
                    Url = TextBoxCustomUrl.Text;
                    InstallWebService();
                }
                else
                {
                    MessageBox.Show("Invalid URL");
                }
            }
            if (RadioButtonGlobal.IsChecked == true)
            {
                GridWindowsInstallerWellcome.Visibility = Visibility.Hidden;
                GridInstallProgress.Visibility = Visibility.Visible;
                Url = GlobalUrl;
                InstallWebService();
            }
        }
        private void InstallWebService()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                DownloadUpdaterDeploy();
            });
        }
        private void DownloadUpdaterDeploy()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LabelCurrentStatusContent.Content = "Downloading Files";
            }));
            while (true)
            {
                Guid randomGuid = Guid.NewGuid();
                string realPath = "C:\\Emse.Updater";
                string tempPath = "C:\\Temp.Emse.Updater";
                string tempForFilesWithRandom = tempPath + "\\" + randomGuid;
                string tempPathForZipWithRandom = tempPath + "\\" + randomGuid + "." + "zip";

                if (Directory.Exists(tempPath))
                {
                    try
                    {
                        Directory.Delete(tempPath, true);
                    }
                    catch (Exception ex)
                    {
                        StatusInfoInvoker(ex.Message);
                    }
                }
                
                Directory.CreateDirectory(tempPath);
                StatusInfoInvoker("Temp folder has been created. " + tempPath);
                
                Directory.CreateDirectory(tempForFilesWithRandom);
                StatusInfoInvoker("Temp with random folder has been created.");
                Thread.Sleep(2000);
                bool downloadFail = false;
                DownloadVersionZip( Url + "/Emse.Updater.zip", tempPathForZipWithRandom);

                while (true)
                {
                    if (!DownloadingVersionZip)
                    {
                        Wc.CancelAsync();
                        break;
                    }

                    if ((DateTime.Now - DownloadingVersionZipProgress.Item1).TotalMinutes > 10)
                    {
                        StatusInfoInvoker("Downloading Timeout - 10 Minutes");
                        downloadFail = true;
                        break;
                    }

                    if (DownloadingVersionZipProgress.Item2 != 0 && DownloadingVersionZipProgress.Item2 != 0)
                    {
                        StatusInfoInvoker("Downloading: " + ((DownloadingVersionZipProgress.Item3 * 100) / DownloadingVersionZipProgress.Item2) + "% - " + DownloadingVersionZipProgress.Item2 + " / " + DownloadingVersionZipProgress.Item3);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        StatusInfoInvoker("Getting ready to download version zip.");
                        Thread.Sleep(2000);
                    }

                    Thread.Sleep(3000);
                }

                if (downloadFail)
                {
                    Thread.Sleep(5000);
                    continue;
                }
                StatusInfoInvoker("Files have been downloaded.");
                Thread.Sleep(2000);
                ZipFile.ExtractToDirectory(tempPathForZipWithRandom, tempForFilesWithRandom);

                StatusInfoInvoker("Files have been unzipped");

                Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LabelCurrentStatusContent.Content = "Installing Files";
                }));

                System.IO.Directory.CreateDirectory(realPath);
                Handler.PathHelper.Empty(new DirectoryInfo(realPath), new List<DirectoryInfo>() { new DirectoryInfo(tempPath) });

                StatusInfoInvoker("Moving files from temp path to " + realPath);
                
                Thread.Sleep(2000);
                Handler.PathHelper.CopyDir(tempForFilesWithRandom, realPath);

                StatusInfoInvoker("Copied to " + realPath);

                Thread.Sleep(2000);

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                    StatusInfoInvoker(tempPath + " has been deleted.") ;
                }
                if (!DownloadingVersionZip)
                {
                    Thread.Sleep(5000);
                    break;
                }
            }
            InstallUpdaterWindowsService();
        }
        private void InstallUpdaterWindowsService()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LabelCurrentStatusContent.Content = "Installing Windows Service";
            }));

            StatusInfoInvoker("Checking OS and services.msc");
            Thread.Sleep(1500);

            bool windowsRegistry = Handler.WindowsServiceHelper.RegisterWindowsService("EmseUpdater");
            if (windowsRegistry)
            {
                StatusInfoInvoker("Emse Updater has been added to Services ");
            }
            else
            {
                StatusInfoInvoker("Emse Updater is already installed");
            }

            StatusInfoInvoker("Done!");
            Thread.Sleep(2000);
            OpenUpdaterSettings();
        }
        private void OpenUpdaterSettings()
        {
            StatusInfoInvoker("");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LabelCurrentStatusContent.Content = "Running Emse Updater Settings";
            }));
            Program = StartProcess("C:\\Emse.Updater\\Emse.Updater.Settings.WPF.exe");
            Thread.Sleep(5000);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LabelCurrentStatusContent.Content = "";
            }));
            Environment.Exit(1);
        }
        private void DownloadVersionZip(string latestversionURL, string tempPathForZipWithRandom)
        {
            try
            {
                DownloadingVersionZip = true;
                DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.Now, 0, 0);
                Wc.DownloadFileCompleted += DownloadFileCompleted;
                Wc.DownloadProgressChanged += DownloadProgressChanged;
                Wc.DownloadFileAsync(new Uri(latestversionURL), tempPathForZipWithRandom);
            }
            catch (Exception ex)
            {
                StatusInfoInvoker(ex.Message);
                DownloadingVersionZip = false;
            }
        }
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.Now, downloadProgressChangedEventArgs.TotalBytesToReceive, downloadProgressChangedEventArgs.BytesReceived);
        }
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            DownloadingVersionZip = false;
            DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.MinValue, 0, 0);
        }
        private void StatusInfoInvoker(string log)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TextBlockCurrentStatusInfo.Text = log;
            }));
        }
        private Process StartProcess(string path)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = path,
                ErrorDialog = false,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            process.StartInfo = startinfo;
            process.Start();

            return process;
        }
        private void RadioButtonLocal_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxCustomUrl.Visibility = Visibility.Visible;
        }
        private void RadioButtonGlobal_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxCustomUrl.Visibility = Visibility.Hidden;
        }
    }
}
