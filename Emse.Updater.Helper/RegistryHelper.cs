using System;
using System.IO;
using System.Net;
using System.Text;
using Emse.Updater.DTO;
using Microsoft.Win32;

namespace Emse.Updater.Helper
{
    public class RegistryHelper
    {
        public static int GetValue(string path)
        {
            RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey(path, true);

            if (softwareKey != null)
            {
                return  (int) softwareKey.GetValue("EnableLUA");
            }
            else
            {
                return -1;
            }
        }

        public static void SetValue(string path, string arg, int value)
        {
            Registry.SetValue(path, arg, value);
        }
    }
}