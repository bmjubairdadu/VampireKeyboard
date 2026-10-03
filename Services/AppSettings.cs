using System.IO;
using System.Text.Json;

namespace VampireKeyboard.Services;
public class AppSettings
{
    public string SelectedLanguage { get; set; } = "banglish-bangla";
    public string FromLanguage { get; set; } = "banglish-bangla";
    public string ToLanguage { get; set; } = "bangla";
    public bool SetupCompleted { get; set; }
    public bool StartWithWindows { get; set; }
    public bool AutoTranslateEnabled { get; set; } = true;

    /// <summary>Valid "From" (input) language keys.</summary>
    public static readonly string[] FromKeys = { "banglish-bangla", "english", "bijoy", "hindi", "urdu" };

    /// <summary>Valid "To" (output) language keys. Note: "banglish-bangla" is NOT a valid target.</summary>
    public static readonly string[] ToKeys = { "bangla", "english", "hindi", "urdu" };

    /// <summary>
    /// Repairs legacy/invalid persisted values. Older builds saved ToLanguage="banglish-bangla",
    /// which made FromLanguage == ToLanguage and silently disabled ALL conversion.
    /// </summary>
    public AppSettings Normalize()
    {
        if (!FromKeys.Contains(FromLanguage)) FromLanguage = "banglish-bangla";
        if (!ToKeys.Contains(ToLanguage)) ToLanguage = "bangla";
        if (FromLanguage == ToLanguage)
        {
            // Pick any target that is not the input language so conversion can run.
            ToLanguage = ToKeys.FirstOrDefault(k => k != FromLanguage) ?? "bangla";
        }
        return this;
    }

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
                return JsonSerializer.Deserialize<AppSettings>(json)?.Normalize() ?? new AppSettings().Normalize();
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
