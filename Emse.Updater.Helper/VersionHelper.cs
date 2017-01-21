using System;
using System.IO;
using System.Net;
using System.Text;
using Emse.Updater.DTO;

namespace Emse.Updater.Helper
{
    public class VersionHelper
    {
        public static Version GetLatestVersion()
        {
            Version result = null;
            SettingDto setting = Emse.Updater.Helper.JsonHelper.JsonReader();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(setting.Domain);
                request.Timeout = 10000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        if (receiveStream != null)
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                StreamReader streamReader = response.CharacterSet == null ? new StreamReader(receiveStream) : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                                string readToEndData = streamReader.ReadToEnd();

                                result = new Version(readToEndData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
            }

            return result;
        }

        public static string GetVersionZipURL(Version version)
        {
            DTO.SettingDto setting = Emse.Updater.Helper.JsonHelper.JsonReader();
            return Emse.Updater.Helper.JsonHelper.JsonReader().UrlZip.Replace("{version}", version + "." + setting.FileExtension);
        }
    }
}