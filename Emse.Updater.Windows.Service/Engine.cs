using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Emse.Updater.DTO;
using Emse.Updater.Helper;
using System.IO.Compression;
using Microsoft.Win32;

namespace Emse.Updater.Windows.Service
{
    public class Engine
    {
        public static bool DownloadingVersionZip;
        public static Tuple<DateTime, long, long> DownloadingVersionZipProgress = new Tuple<DateTime, long, long>(DateTime.MinValue, 0, 0);
        private static readonly WebClient Wc = new WebClient();
        public static Process Program { get; set; }
        public static bool UpdateStatus = true;
        public static string AppDir;
        public static void Execute()
        {
            //Console.WriteLine("while (true): " + DateTime.Now.ToString("hh:mm:ss"));
            Engine e = new Engine();
            e.Updater();
        }
        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                Exception e = (Exception)args.ExceptionObject;
                LoggerAdapter.Instance.Debug("Exception - GlobalExceptionHandler: " + e.Message);
                LoggerAdapter.Instance.Debug("Runtime terminating:" + args.IsTerminating);
            }
            catch (Exception ex)
            {
                LoggerAdapter.Instance.Debug("Exception - GlobalExceptionHandler - " + ex.Message);
            }
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
                LoggerAdapter.Instance.Debug(ex.Message);
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
                    LoggerAdapter.Instance.Debug("WerFault killed.");
                }
                catch (Exception ex)
                {
                    LoggerAdapter.Instance.Debug("KillWerFaultProcesses: " + ex.Message);
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
                LoggerAdapter.Instance.Debug(ex.Message);
            }

            while (true)
            {
                if (Process.GetProcessesByName("explorer").Length == 0) //Wait for desktop
                {
                    Thread.Sleep(1000);
                    LoggerAdapter.Instance.Debug("Waiting for explorer");
                    continue;
                }

                break;
            }

            while (true)
            {
                KillWerFaultProcesses();

                try
                {
                    //if (RegistryHelper.GetValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA") == 1) //Set EnableLUA
                    //{
                    //    RegistryHelper.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0);
                    //    Thread.Sleep(2000);
                    //    WindowsHelper.ExitWindows(WindowsHelper.ExitWindowsType.ForceRestart, WindowsHelper.ShutdownReason.FlagPlanned, true);
                    //}

                    if (!UpdateStatus)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    string realPath = Helper.PathHelper.GetRealPath();

                    setting = Helper.JsonHelper.JsonReader();
                    LoggerAdapter.Instance.Debug("JSON file has been read");

                    Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);
                    if (processOfEmse.Length != 0)
                    {
                        Process mainProcess = processOfEmse[0];

                        //if (!mainProcess.Responding)
                        //{
                        //    LoggerAdapter.Instance.Debug(setting.ExeName + " not responding.");
                        //    Helper.ProcessHelper.KillProcess();
                        //    LoggerAdapter.Instance.Debug(setting.ExeName + " Process killed.");
                        //}
                    }

                    if (processOfEmse.Length == 0)
                    {
                        if (File.Exists(realPath + "\\" + setting.ExeName + ".exe"))
                        {
                            //Program = StartProcess(realPath + "\\" + setting.ExeName + ".exe");
                            AppDir = realPath + "\\" + setting.ExeName + ".exe";

                            RegistryKey ly = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                            if (ly == null)
                            {
                                RegistryKey acf = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags", true);
                                ly = acf.CreateSubKey(@"Layers");
                            }
                            if(ly.GetValue(AppDir) == null)
                                ly.SetValue(AppDir, "~RUNASADMIN");


                            try
                            {
                                Emse.Updater.Helper.ProcessExtensions.StartProcessAsCurrentUser(AppDir);
                                LoggerAdapter.Instance.Debug("Process has been started.");
                            }
                            catch (Exception ex)
                            {
                                LoggerAdapter.Instance.Debug("Create Process As User failed: " + ex);
                            }
                        }
                        else
                            LoggerAdapter.Instance.Debug("exe file not found");

                    }
                    else
                        LoggerAdapter.Instance.Debug("process's already running");

                    LoggerAdapter.Instance.Debug("Version.txt will be read.");
                    Version latestVersion = Helper.VersionHelper.GetLatestVersion();

                    if (latestVersion == null)
                    {
                        LoggerAdapter.Instance.Debug("Version.txt Read Timeout - 10 Seconds");

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
                                LoggerAdapter.Instance.Debug(ex.Message);
                            }
                        }

                        string latestversionURL = Helper.VersionHelper.GetVersionZipURL(latestVersion);

                        Directory.CreateDirectory(tempPath);
                        LoggerAdapter.Instance.Debug("Temp folder has been created. " + tempPath);
                        Directory.CreateDirectory(tempForFilesWithRandom);
                        LoggerAdapter.Instance.Debug("Temp with random folder has been created.");
                        LoggerAdapter.Instance.Debug(" Downloading " + setting.AppName + " , from url: " + latestversionURL);
                        bool downloadFail = false;
                        DownloadVersionZip(latestversionURL, tempPathForZipWithRandom);

                        while (true)
                        {
                            if (!DownloadingVersionZip || !UpdateStatus)
                            {
                                Wc.CancelAsync();

                                break;
                            }

                            if ((DateTime.Now - DownloadingVersionZipProgress.Item1).TotalMinutes > 10)
                            {
                                LoggerAdapter.Instance.Debug("Downloading Timeout - 10 Minutes");
                                downloadFail = true;
                                break;
                            }

                            if (DownloadingVersionZipProgress.Item2 != 0 && DownloadingVersionZipProgress.Item2 != 0)
                            {
                                LoggerAdapter.Instance.Debug("Downloading: " + ((DownloadingVersionZipProgress.Item3 * 100) / DownloadingVersionZipProgress.Item2) + "% - " + DownloadingVersionZipProgress.Item2 + " / " + DownloadingVersionZipProgress.Item3);
                            }
                            else
                            {
                                LoggerAdapter.Instance.Debug("Getting ready to download version zip.");
                            }

                            Thread.Sleep(3000);
                        }

                        if (!UpdateStatus || downloadFail)
                        {
                            ToSleep(setting);
                            LoggerAdapter.Instance.Debug("Download fail.");
                            continue;
                        }

                        //UserInterfaceHandler.StartUserInterfaceAsUser();
                        LoggerAdapter.Instance.Debug(latestversionURL + " has been downloaded.");
                        ZipFile.ExtractToDirectory(tempPathForZipWithRandom, tempForFilesWithRandom);
                        LoggerAdapter.Instance.Debug("File has been unzipped");
                        if (setting.ConsoleMode)
                        {
                            Helper.ProcessHelper.CloseProcess();
                            Thread.Sleep(3000);

                            //Helper.ProcessHelper.SoftClose(realPath, setting.ExeName);
                            //LoggerAdapter.Instance.Debug(setting.ExeName + " soft closed.");
                        }
                        else
                        {
                            Helper.ProcessHelper.CloseProcess();
                            Thread.Sleep(1000);
                            Helper.ProcessHelper.KillProcess();
                            LoggerAdapter.Instance.Debug(setting.ExeName + " Process killed.");
                            Thread.Sleep(5000);
                        }
                        System.IO.Directory.CreateDirectory(realPath);

                        Helper.PathHelper.Empty(new DirectoryInfo(realPath), new List<DirectoryInfo>() { new DirectoryInfo(tempPath) }, SetFilesToKeep(setting));
                        LoggerAdapter.Instance.Debug("Moving files from temp path to " + realPath);

                        Helper.PathHelper.CopyDir(tempForFilesWithRandom, realPath);
                        LoggerAdapter.Instance.Debug("Copied to " + realPath);

                        setting.CurrentVersion = latestVersion.ToString();
                        Helper.JsonHelper.JsonWriter(setting);
                        LoggerAdapter.Instance.Debug("Settings has been saved");

                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                            LoggerAdapter.Instance.Debug(tempPath + " has been deleted.");
                        }

                        //UserInterfaceHandler.StopUserInterface();
                        AppDir = realPath + "\\" + setting.ExeName + ".exe";

                        try
                        {
                            ProcessExtensions.StartProcessAsCurrentUser(AppDir);
                            LoggerAdapter.Instance.Debug("Process has been started.");
                        }
                        catch (Exception ex)
                        {
                            LoggerAdapter.Instance.Debug("CreateProcessAsUser failed: " + ex);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LoggerAdapter.Instance.Debug(ex.Message);
                }

                ToSleep(setting);
                ClearConsole();
            }
        }

        DateTime clearTime = DateTime.Now;
        private void ClearConsole()
        {
            if (DateTime.Now.Subtract(clearTime) > TimeSpan.FromMinutes(30))
            {
                Console.Clear();
                clearTime = DateTime.Now;
            }
        }

        private static List<FileInfo> SetFilesToKeep(SettingDto setting)
        {
            LoggerAdapter.Instance.Debug("filestoKeep:"+setting.FilesToKeep);

            List<FileInfo> filesToKeep = null;
            string[] fnsToKeep = setting.FilesToKeep.Split(';');

            LoggerAdapter.Instance.Debug(fnsToKeep.Length + " files found to keep");

            foreach (var fnToKeep in fnsToKeep)
            {
                if (filesToKeep == null)
                    filesToKeep = new List<FileInfo>();
                try
                {
                    string lastSlash = setting.Path.EndsWith("\\") ? "" : "\\";
                    filesToKeep.Add(new FileInfo(setting.Path+ lastSlash +fnToKeep));

                }
                catch (Exception ex)
                {
                    LoggerAdapter.Instance.Debug("FilesToKeep failed:"+ fnToKeep +","+ ex.Message);

                }
            }
            return filesToKeep;
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
    }
}
