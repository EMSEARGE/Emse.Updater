﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Emse.Updater.DTO;
using Emse.Updater.Helper;


namespace Emse.Updater
{
    public partial class App
    {
        public static bool DownloadingVersionZip;
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

        private void KillWerFaultProcesses()
        {
            Process[] werFaultProcessList = Process.GetProcessesByName("WerFault");
            foreach (Process werFaultProcess in werFaultProcessList)
            {
                try
                {
                    werFaultProcess.Kill();
                    LogHelper.WriteLog("WerFault killed.");
                }
                catch (Exception ex)
                {
                    Emse.Updater.Helper.LogHelper.WriteLog("KillWerFaultProcesses: " + ex.Message);
                }
            }
        }

        private void Updater()
        {
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
                KillWerFaultProcesses();

                try
                {
                    if (!UpdateStatus)
                    {
                        Thread.Sleep(1000);

                        continue;
                    }

                    string realPath = Helper.PathHelper.GetRealPath();

                    setting = Helper.JsonHelper.JsonReader();
                    LogHelper.WriteLog("JSON file has been read");

                    Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);
                    if (processOfEmse.Length != 0)
                    {
                        Process mainProcess = processOfEmse[0];

                        if (!mainProcess.Responding)
                        {
                            LogHelper.WriteLog(setting.ExeName + " not responding.");
                            Helper.ProcessHelper.KillProcess();
                            LogHelper.WriteLog(setting.ExeName + " Process killed.");
                        }
                    }

                    if (processOfEmse.Length == 0)
                    {
                        if (File.Exists(realPath + "\\" + setting.ExeName + ".exe"))
                        {
                            Program = StartProcess(realPath + "\\" + setting.ExeName + ".exe");
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
                        string tempPath = Helper.PathHelper.GetTempPath();
                        string tempForFilesWithRandom = tempPath + "\\" + randomGuid;
                        string tempPathForZipWithRandom = tempPath + "\\" + randomGuid + "." + setting.FileExtension;

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
                        Helper.ProcessHelper.CloseProcess();
                        Thread.Sleep(1000);
                        Helper.ProcessHelper.KillProcess();
                        LogHelper.WriteLog(setting.ExeName + " Process killed.");
                        Thread.Sleep(3000);
                        System.IO.Directory.CreateDirectory(realPath);
                        Helper.PathHelper.Empty(new DirectoryInfo(realPath), new List<DirectoryInfo>() { new DirectoryInfo(tempPath) });
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

                        Program = StartProcess(realPath + "\\" + setting.ExeName + ".exe");
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