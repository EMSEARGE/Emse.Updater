using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Emse.Updater.Helper
{
    public class ProcessHelper
    {
        public static void CloseProcess()
        {
            DTO.SettingDto setting = Emse.Updater.Helper.JsonHelper.JsonReader();
            Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);
            foreach (Process process in processOfEmse)
            {
                try
                {
                    process.CloseMainWindow();
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }

        public static void KillProcess()
        {
            DTO.SettingDto setting = Emse.Updater.Helper.JsonHelper.JsonReader();
            Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);
            foreach (Process process in processOfEmse)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }

        public static bool IsEmseHelperRunning()
        {
            string procName = Process.GetCurrentProcess().ProcessName;
            return IsAppRunning(procName);
        }

        private static bool IsAppRunning(string procName)
        {

            // get the list of all processes by the "procName"       
            Process[] processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                return true;
            }

            // Application.Run(...);
            return false;
        }

        public static bool SoftClose(string path, string procName)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path argument to SoftClose is null");

            DateTime t = DateTime.Now;
            try
            {
                File.CreateText(path + "CloseCmd.txt").Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("CloseCmd " + ex.Message);
                return false;
            }

            bool isRunning = true;
            while (isRunning)
            {
                isRunning = Process.GetProcessesByName(procName).Length != 0;
                Thread.Sleep(1000);
                if (DateTime.Now.Subtract(t) > TimeSpan.FromSeconds(10))
                    return false;
            }
            return true;
        }

        public static void KillWerFaultProcesses()
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
    }
}