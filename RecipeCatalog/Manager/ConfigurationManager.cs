using Newtonsoft.Json.Linq;
using RecipeCatalog.Resources.Language;

namespace RecipeCatalog.Manager
{
    public static class ConfigurationManager
    {
        public static string GetDirectoryPath()
        {
            string configDirectoryPath;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                configDirectoryPath = Path.Combine(FileSystem.AppDataDirectory, "DnDRecipes");
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                configDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DnDRecipes");
            }
            else
            {
                throw new PlatformNotSupportedException(AppLanguage.Exception_PlatformNotSupported);
            }
            return configDirectoryPath;
        }
        public static string GetFilePath()
        {
            string configDirectoryPath;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                configDirectoryPath = Path.Combine(FileSystem.AppDataDirectory, "DnDRecipes");
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                configDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DnDRecipes");
            }
            else
            {
                throw new PlatformNotSupportedException(AppLanguage.Exception_PlatformNotSupported);
            }
            return Path.Combine(configDirectoryPath, "userSettings.json");
        }
        public static string? ReadValue(string key)
        {
            try
            {
                JObject jsonObj = JObject.Parse(File.ReadAllText(GetFilePath()));
                return jsonObj.SelectToken(key)?.ToString() ?? null;
            }
            catch //probably, first time user, all good
            {
                return null;
            }
        }

        public static void UpdateValue(string key, string newValue)
        {
            JObject jsonObj = JObject.Parse(File.ReadAllText(GetFilePath()));
            var token = jsonObj.SelectToken(key);
            token?.Replace(newValue);
            File.WriteAllText(GetFilePath(), jsonObj.ToString());
        }
    }
}
