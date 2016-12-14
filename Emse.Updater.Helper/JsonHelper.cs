using System.IO;
using Emse.Updater.DTO;
using Newtonsoft.Json;

namespace Emse.Updater.Helper
{
    public class JsonHelper
    {
        public static SettingDto JsonReader()
        {
            string pathToJson = System.AppDomain.CurrentDomain.BaseDirectory + "Settings.json";
            string jsonContent = File.ReadAllText(pathToJson);
            SettingDto jDto = JsonConvert.DeserializeObject<SettingDto>(jsonContent);
            return jDto;
        }

        public static void JsonWriter(SettingDto settings)
        {
            string pathToJson = System.AppDomain.CurrentDomain.BaseDirectory + "Settings.json";
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(pathToJson, json);
        }
    }
}