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
        public static bool DownloadingVersionZip;
        public static Tuple<DateTime, long, long> DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.MinValue, 0, 0);
        private static readonly WebClient Wc = new WebClient();
        public static Process Program { get; set; }
        public InstallWindowsService()
        {
            InitializeComponent();
            GridWindowsInstallerWellcome.Visibility = Visibility.Visible;
        }
        private void BtnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GridWindowsInstallerWellcome.Visibility = Visibility.Hidden;
            GridInstallProgress.Visibility = Visibility.Visible;
            InstallWebService();
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
                string realPath = Helper.PathHelper.GetRealPath();
                string tempPath = Helper.PathHelper.GetTempPath();
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
                Version latestVersion = Helper.VersionHelper.GetLatestVersion();
                string latestversionURL = Helper.VersionHelper.GetVersionZipURL(latestVersion);
                
                Directory.CreateDirectory(tempPath);
                StatusInfoInvoker("Temp folder has been created. " + tempPath);
                
                Directory.CreateDirectory(tempForFilesWithRandom);
                StatusInfoInvoker("Temp with random folder has been created.");
                Thread.Sleep(2000);
                StatusInfoInvoker(" Downloading Emse Updater from url: " + latestversionURL);
                Thread.Sleep(2000);
                bool downloadFail = false;
                DownloadVersionZip(latestversionURL, tempPathForZipWithRandom);

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
                Helper.PathHelper.Empty(new DirectoryInfo(realPath), new List<DirectoryInfo>() { new DirectoryInfo(tempPath) });

                StatusInfoInvoker("Moving files from temp path to " + realPath);
                
                Thread.Sleep(2000);
                Helper.PathHelper.CopyDir(tempForFilesWithRandom, realPath);

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
#if !DEBUG
                    bool windowsRegistry = Helper.WindowsServiceHelper.RegisterWindowsService("EmseUpdater");
                    if (windowsRegistry)
                    {
                        StatusInfoInvoker("Emse Updater has been added to Services ");
                    }
                    else
                    {
                        StatusInfoInvoker("Emse Updater is already installed");
                    }
#endif

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
            Program = StartProcess(Helper.JsonHelper.JsonReader().Path + "\\" + Helper.JsonHelper.JsonReader().AppName + ".exe");
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
    }
}
