using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emse.Updater.Helper
{
    public class PathHelper
    {
        public static string ReplacePath(string path)
        {
            path = path.Replace("{Desktop}", Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory));
            return path;
        }

        public static string GetTempPath()
        {
            return ReplacePath(Emse.Updater.Helper.JsonHelper.JsonReader().TempPath);
        }

        public static string GetRealPath()
        {
            return ReplacePath(Emse.Updater.Helper.JsonHelper.JsonReader().Path);
        }

        public static void Empty(System.IO.DirectoryInfo directory, List<System.IO.DirectoryInfo> excludePathList = null)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
            {
                if (excludePathList == null || excludePathList.FirstOrDefault(a => a.FullName.Contains(subDirectory.FullName)) == null)
                {
                    try
                    {
                        subDirectory.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
            }
        }

        public static void CopyDir(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            // Get Files & Copy
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);

                // ADD Unique File Name Check to Below!!!!
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }

            // Get dirs recursively and copy files
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyDir(folder, dest);
            }
        }
    }
}
