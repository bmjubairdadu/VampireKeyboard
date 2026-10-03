using System.IO;
using System.Text.Json;

namespace VampireKeyboard.Services;
public class AppSettings
{
    public string SelectedLanguage { get; set; } = "banglish-bangla";
    public string FromLanguage { get; set; } = "banglish-bangla";
    public string ToLanguage { get; set; } = "banglish-bangla";
    public bool SetupCompleted { get; set; }
    public bool StartWithWindows { get; set; }
    public bool AutoTranslateEnabled { get; set; } = true;

    private static string SettingsDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VampireKeyboard");

    private static string SettingsPath => Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}
