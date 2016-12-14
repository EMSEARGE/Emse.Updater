using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emse.Updater.Helper
{
    public class LogHelper
    {
        static readonly object LogHelperLocker = new object();



        public static void WriteLog(string message)
        {
            try
            {
                lock (LogHelperLocker)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "-Log.log"; //Teşekkürler Uğur

                    if (!File.Exists(path))
                    {
                        File.Create(path).Dispose();

                        using (TextWriter tw = new StreamWriter(path, true))
                        {
                            tw.WriteLine("------------ " + DateTime.Now);
                            tw.Close();
                        }

                        List<string> logFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.log", SearchOption.AllDirectories).OrderByDescending(s => s).ToList();

                        IEnumerable<string> toDeleteLogFiles = logFiles.Skip(7);
                        foreach (string toDeleteLogFile in toDeleteLogFiles)
                        {
                            try
                            {
                                File.Delete(toDeleteLogFile);
                            }
                            catch (Exception ex)
                            {
                                // ignored
                            }
                        }
                    }

                    using (TextWriter tw = new StreamWriter(path, true))
                    {
                        tw.WriteLine(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + ": " + message);
                        tw.Close();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static List<string> ReadLog()
        {
            List<string> result = null;

            try
            {
                lock (LogHelperLocker)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.Year.ToString("0000") +  DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "-Log.log";

                    if (File.Exists(path))
                    {
                        result = File.ReadLines(path).TakeLast(30).Reverse().ToList();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }
    }
}