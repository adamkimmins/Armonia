using System;
using System.IO;
using System.Text.Json;

namespace Armonia.App.Services
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Rustic";
        public double MasterVolume { get; set; } = 50;
        public bool ShowStartupScreens { get; set; } = true;
    }

    public static class SettingsService
    {
        private static readonly string _configDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Armonia");
        private static readonly string _configFile = Path.Combine(_configDir, "appsettings.json");

        private static AppSettings? _cachedSettings;

        public static AppSettings Load()
        {
            try
            {
                if (_cachedSettings != null)
                    return _cachedSettings;

                if (!Directory.Exists(_configDir))
                    Directory.CreateDirectory(_configDir);

                if (!File.Exists(_configFile))
                {
                    _cachedSettings = new AppSettings();
                    Save(_cachedSettings);
                    return _cachedSettings;
                }

                var json = File.ReadAllText(_configFile);
                _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                return _cachedSettings;
            }
            catch
            {
                _cachedSettings = new AppSettings();
                return _cachedSettings;
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                if (!Directory.Exists(_configDir))
                    Directory.CreateDirectory(_configDir);

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFile, json);
                _cachedSettings = settings;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Error");
            }
        }
    }
}
