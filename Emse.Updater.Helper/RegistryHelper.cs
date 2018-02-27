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
        /// <summary>
        /// return registry value from subkey
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static int GetValue(string subKey, string keyName)
        {
            RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey(subKey, true);

            if (softwareKey != null)
            {
                return  (int) softwareKey.GetValue(keyName);
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// set registry value to subkey
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        public static void SetValue(string subKey, string keyName, int value)
        {
            Registry.SetValue(subKey, keyName, value);
        }
    }
}