using System;
using System.Linq;
using System.ServiceProcess;

namespace Emse.Updater.Installer.WPF.Handler
{
    public class WindowsServiceHelper
    {
        #region RegisterWindowsService
        public static bool RegisterWindowsService(string serviceName)
        {
            bool result = false;
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                if (!IsServiceInstalled(serviceName))
                {
                    //# 32 Bit Operation System
                    if (IntPtr.Size == 4)
                    {
                        startInfo.Arguments = @"/C C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe C:\\Emse.Updater\\Emse.Updater.Windows.Service.exe";
                    }
                    //# 64 Bit Operation System
                    else if (IntPtr.Size == 8)
                    {
                        startInfo.Arguments = @"/C C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe C:\\Emse.Updater\\Emse.Updater.Windows.Service.exe";
                    }
                    startInfo.FileName = "cmd.exe";
                    startInfo.Verb = "runas";
                    process.StartInfo = startInfo;
                    process.Start();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                // ignored
            }

            return result;
        }
        public static bool IsServiceInstalled(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.Any(service => service.ServiceName == serviceName);
        }
    }
    #endregion
}

