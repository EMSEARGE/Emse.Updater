using System.Diagnostics;

namespace Emse.Updater.Helper
{
    public class ProcessHelper
    {
        // DRY = Dont Repeat Yourself
        public static void KillProcess()
        {
            DTO.SettingDto setting = Emse.Updater.Helper.JsonHelper.JsonReader();
            Process[] processOfEmse = Process.GetProcessesByName(setting.ExeName);
            foreach (Process process in processOfEmse)
            {
                process.CloseMainWindow();
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