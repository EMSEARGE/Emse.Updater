using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Emse.Updater.Helper
{
    public class UserInterfaceHandler
    {
        public static void StartUserInterface()
        {
            try
            {
                string url = @System.AppDomain.CurrentDomain.BaseDirectory + "/updater.html ";

                string chromePath = @"C:\Chrome\PortableChrome.exe";

                if (!File.Exists(chromePath))
                {
                    chromePath = @"chrome";
                }

                try
                {
                    Process chrome = new Process
                    {
                        StartInfo =
                        {
                            FileName = chromePath,
                            Arguments = url +
                                        " --user-data-dir=" + System.AppDomain.CurrentDomain.BaseDirectory +
                                        "//Chrome-UserData// " +
                                        " --no-first-run" +
                                        " --fast --fast-start" +
                                        " --unsafely-treat-insecure-origin-as-secure=" + url +
                                        " --allow-running-insecure-content" +
                                        " --check-for-update-interval=604800" +
                                        " --extensions-update-frequency=604800" +
                                        " --new-window" +
                                        " --disable-notifications" +
                                        " --enable-media-stream" +
                                        " --no-default-browser-check" +
                                        " --aggressive-cache-discard" +
                                        " --disk-cache-size=1" +
                                        " --disable-cache" +
                                        " --disk-cache-dir=\"z:\\\"" +
                                        " --media-cache-dir=\"z:\\\"" +
                                        " --disable-gpu-program-cache" +
                                        " --disable-gpu-shader-disk-cache" +
                                        " --disable-lru-snapshot-cache" +
                                        " --disable-application-cache" +
                                        " --use-fake-ui-for-media-stream" +
                                        " --ignore-certificate-errors" +
                                        " --test-type" +
                                        " --disable-infobars" +
                                        " --disable-session-crashed-bubble" +
                                        " --kiosk --incognito --disable-pinch --overscroll-history-navigation=0 " //kiosk mode
                        }
                    };

                    chrome.Start();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("Exception - StartUserInterface - (App.Chrome = new Process()) - " + ex.Message);
                }

                LogHelper.WriteLog("StartUserInterface - Success");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Exception - StartUserInterface: " + ex.Message);
            }
        }

        public static void StartUserInterfaceAsUser()
        {
            try
            {
                string url = @System.AppDomain.CurrentDomain.BaseDirectory + "updater.html ";
                string kioskMode = url +
                                   " --no-first-run" +
                                   " --fast --fast-start" +
                                   " --unsafely-treat-insecure-origin-as-secure=" + url +
                                   " --allow-running-insecure-content" +
                                   " --check-for-update-interval=604800" +
                                   " --extensions-update-frequency=604800" +
                                   " --new-window" +
                                   " --disable-notifications" +
                                   " --enable-media-stream" +
                                   " --no-default-browser-check" +
                                   " --aggressive-cache-discard" +
                                   " --disk-cache-size=1" +
                                   " --disable-cache" +
                                   " --disk-cache-dir=\"z:\\\"" +
                                   " --media-cache-dir=\"z:\\\"" +
                                   " --disable-gpu-program-cache" +
                                   " --disable-gpu-shader-disk-cache" +
                                   " --disable-lru-snapshot-cache" +
                                   " --disable-application-cache" +
                                   " --use-fake-ui-for-media-stream" +
                                   " --ignore-certificate-errors" +
                                   " --test-type" +
                                   " --disable-infobars" +
                                   " --disable-session-crashed-bubble" +
                                   " --kiosk --incognito --disable-pinch --overscroll-history-navigation=0 ";
                ProcessExtensions.StartProcessAsCurrentUser(null, @"C:\Chrome\PortableChrome.exe" + " " + kioskMode);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Exception - StartUserInterface: " + ex.Message);
            }
        }

        public static void StopUserInterface()
        {
            try
            {
                try
                {
                    IEnumerable<Process> chromeDriverProcesses = Process.GetProcesses()
                        .Where(pr => pr.ProcessName == "PortableChrome" || pr.ProcessName == "chrome");

                    foreach (Process process in chromeDriverProcesses)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                LogHelper.WriteLog("StopUserInterface - Success");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Exception - StopUserInterface: " + ex.Message);
            }

            //Helper.PurgeChromeUserData();
        }
    }
}