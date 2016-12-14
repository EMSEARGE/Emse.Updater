using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Emse.Updater.DTO;
using Emse.Updater.Helper;


namespace Emse.Updater
{
    public partial class App : Application
    {
        public static bool DownloadingVersionZip = false;
        public static Tuple<DateTime, long, long> DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.MinValue, 0,0); 
        private static readonly WebClient Wc = new WebClient();
        public static Process Program { get; set; }
        public static bool UpdateStatus = true;
        public static bool MainWindowStatus = false;
        private TaskbarIcon _notifyIcon;

        public App()
        {
            try
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Exception - App - " + ex.Message);
            }
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                Exception e = (Exception)args.ExceptionObject;
                LogHelper.WriteLog("Exception - GlobalExceptionHandler: " + e.Message);
                LogHelper.WriteLog("Runtime terminating:" + args.IsTerminating);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Exception - GlobalExceptionHandler - " + ex.Message);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            /*Birden Fazla Exe yi çalıştırmamak için*/
            if (ProcessHelper.IsEmseHelperRunning())
            {
                MessageBox.Show("Emse Updater already running");
                Shutdown();
                return;
            }

            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Updater();
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
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
                Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
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

        private async void Updater()
        {
            string tempForFilesWithRandom = null, tempPathForZipWithRandom = null, tempPath = null, realPath = null;
            SettingDto setting = new SettingDto();

            try
            {
                setting = Helper.JsonHelper.JsonReader();
            }
            catch (Exception ex)
            {
                Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
                LogHelper.WriteLog(ex.Message);
            }

            while (true)
            {
                try
                {
                    if (!UpdateStatus)
                    {
                        Thread.Sleep(1000);

                        continue;
                    }

                    realPath = Helper.PathHelper.GetRealPath();

                    setting = Helper.JsonHelper.JsonReader();
                    LogHelper.WriteLog("JSON file has been read");

                    Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);

                    if (processOfEmse.Length == 0)
                    {
                        if (File.Exists(realPath + "\\" + setting.ExeName + ".exe"))
                        {
                            Process.Start(realPath + "\\" + setting.ExeName + ".exe");
                        }
                    }

                    LogHelper.WriteLog("Version.txt will be read.");
                    Version latestVersion = Helper.VersionHelper.GetLatestVersion();

                    if (latestVersion == null)
                    {
                        LogHelper.WriteLog("Version.txt Read Timeout - 10 Seconds");

                        ToSleep(setting);
                        continue;
                    }

                    if (latestVersion != new Version(setting.CurrentVersion)) //yeni sürüm var
                    {
                        Guid randomGuid = Guid.NewGuid();
                        tempPath = Helper.PathHelper.GetTempPath();
                        tempForFilesWithRandom = tempPath + "\\" + randomGuid;
                        tempPathForZipWithRandom = tempPath + "\\" + randomGuid + "." + setting.FileExtension;

                        if (Directory.Exists(tempPath))
                        {
                            try
                            {
                                Directory.Delete(tempPath, true);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteLog(ex.Message);
                            }
                        }

                        string latestversionURL = Helper.VersionHelper.GetVersionZipURL(latestVersion);

                        Directory.CreateDirectory(tempPath);
                        LogHelper.WriteLog("Temp folder has been created. " + tempPath);
                        Directory.CreateDirectory(tempForFilesWithRandom);
                        LogHelper.WriteLog("Temp with random folder has been created.");
                        LogHelper.WriteLog(" Downloading " + setting.AppName + " , from url: " + latestversionURL);
                        bool downloadFail = false;
                        DownloadVersionZip(latestversionURL, tempPathForZipWithRandom);

                        while (true)
                        {
                            if (!DownloadingVersionZip || !UpdateStatus)
                            {
                                Wc.CancelAsync();

                                break;
                            }

                            if ((DateTime.Now -DownloadingVersionZipProgress.Item1).TotalMinutes > 10)
                            {
                                LogHelper.WriteLog("Downloading Timeout - 10 Minutes");
                                downloadFail = true;
                                break;
                            }

                            if (DownloadingVersionZipProgress.Item2 != 0 && DownloadingVersionZipProgress.Item2 != 0)
                            {
                                LogHelper.WriteLog("Downloading: " + ((DownloadingVersionZipProgress.Item3 * 100) / DownloadingVersionZipProgress.Item2) + "% - " + DownloadingVersionZipProgress.Item2 + " / " + DownloadingVersionZipProgress.Item3);
                            }
                            else
                            {
                                LogHelper.WriteLog("Getting ready to download version zip.");
                            }

                            Thread.Sleep(3000);
                        }

                        if (!UpdateStatus || downloadFail)
                        {
                            ToSleep(setting);
                            continue;
                        }

                        LogHelper.WriteLog(latestversionURL + " has been downloaded.");
                        ZipFile.ExtractToDirectory(tempPathForZipWithRandom, tempForFilesWithRandom);
                        LogHelper.WriteLog("File has been unzipped");
                        Helper.ProcessHelper.KillProcess();
                        LogHelper.WriteLog(setting.ExeName + " Process killed.");
                        Thread.Sleep(3000);
                        Helper.PathHelper.Empty(new DirectoryInfo(realPath),
                            new List<DirectoryInfo>() { new DirectoryInfo(tempPath) });
                        LogHelper.WriteLog("Moving files from temp path to " + realPath);

                        Helper.PathHelper.CopyDir(tempForFilesWithRandom, realPath);
                        LogHelper.WriteLog("Copied to " + realPath);

                        setting.CurrentVersion = latestVersion.ToString();
                        Helper.JsonHelper.JsonWriter(setting);
                        LogHelper.WriteLog("Settings has been saved");
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                            LogHelper.WriteLog(tempPath + " has been deleted.");
                        }


                        Program = Process.Start(realPath + "\\" + setting.ExeName + ".exe");
                        LogHelper.WriteLog("Starting Process " + setting.ExeName);

                    }
                    if (MainWindowStatus)
                    {
                        Emse.Updater.MainWindow.Main.FillFields();
                    }


                }
                catch (Exception ex)
                {
                    Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
                }

                ToSleep(setting);
            }
        }

        private void ToSleep(SettingDto setting)
        {
            int secondBetweenLoops = setting.SecondsBetweenLoop;
            if (secondBetweenLoops < 1)
            {
                secondBetweenLoops = 10;
            }

            Thread.Sleep(secondBetweenLoops * 1000);
        }

        private void WcOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            Helper.LogHelper.WriteLog("Bytes: " + downloadProgressChangedEventArgs.BytesReceived + " of " + downloadProgressChangedEventArgs.TotalBytesToReceive);
        }

        public static void MainWindowThreadReadLog()
        {
            while (true)
            {
                try
                {
                    if (MainWindowStatus)
                    {
                        List<string> logLineList = LogHelper.ReadLog();
                        Emse.Updater.MainWindow.Main.TextBoxLog.Dispatcher.Invoke(() =>
                        {
                            Emse.Updater.MainWindow.Main.WriteLogLineListToMainWindow(logLineList);
                        });

                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
                }
            }
        }
    }
}