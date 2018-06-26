namespace Emse.Updater.DTO
{
    public class SettingDto
    {
        public int SecondsBetweenLoop { get; set; }
        public string CurrentVersion { get; set; }
        public string Domain { get; set; }
        public string UrlZip { get; set; }
        public string FileExtension { get; set; }
        public string Path { get; set; }
        public string TempPath { get; set; }
        public string TempPathFile { get; set; }
        public string ExeName { get; set; }
        public string AppName { get; set; }
        public bool UpdateStatus { get; set; }
        public string FilesToKeep { get; set; }
    }
}