using Emse.Updater.DTO;
using Emse.Updater.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emse.Updater.Console
{
    class Program
    {
        static void Main(string[] args)
        {
        }



        static void Run()
        {
            SettingDto setting = new SettingDto();

            try
            {
                setting = Helper.JsonHelper.JsonReader();
            }
            catch (Exception ex)
            {
                LoggerAdapter.Instance.Error(ex, "reading Settings");
            }

            while (true)
            {
                if (Process.GetProcessesByName("explorer").Length == 0) //Wait for desktop
                {
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }


            while (true)
            {

                ProcessHelper.KillWerFaultProcesses();
            }


        }
    }
}
