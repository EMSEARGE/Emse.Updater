using System;
using System.Diagnostics;

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
    }
}