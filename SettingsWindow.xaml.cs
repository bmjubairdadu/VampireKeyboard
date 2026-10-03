using System.Windows;
using System.Windows.Controls;
using VampireKeyboard.Services;

namespace VampireKeyboard;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly TranslationService _translator = new();
    private bool _loading = true;

    public SettingsWindow()
    {
        InitializeComponent();
        _settings = ((App)Application.Current).Settings;

        foreach (var lang in LanguageDef.All)
        {
            LanguageBox.Items.Add($"{lang.Flag}  {lang.DisplayName}");
            FromBox.Items.Add(lang.Flag + " " + lang.DisplayName.Split('(')[0].Trim());
            ToBox.Items.Add(lang.Flag + " " + lang.DisplayName.Split('(')[0].Trim());
        }

        int idx = Array.FindIndex(LanguageDef.All, l => l.Id == _settings.SelectedLanguage);
        LanguageBox.SelectedIndex = idx >= 0 ? idx : 0;
        FromBox.SelectedIndex = 0;
        ToBox.SelectedIndex = 2;

        EnableHookBox.IsChecked = _settings.AutoTranslateEnabled;
        _loading = false;
    }

    private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || LanguageBox.SelectedIndex < 0) return;
        _settings.SelectedLanguage = LanguageDef.All[LanguageBox.SelectedIndex].Id;
        _settings.Save();
        ((App)Application.Current).Hook.ToLanguage = _settings.SelectedLanguage;
    }

    private async void TranslateBtn_Click(object sender, RoutedEventArgs e)
    {
        var from = LanguageDef.All[Math.Max(0, FromBox.SelectedIndex)];
        var to = LanguageDef.All[Math.Max(0, ToBox.SelectedIndex)];

        var text = SourceBox.Text;
        if (string.IsNullOrWhiteSpace(text)) return;
        if (from.Id == "banglish-bangla")
        {
            text = BanglishTransliterator.Transliterate(text);
            from = LanguageDef.All[2];
        }

        ResultBox.Text = "Translating...";
        var result = await _translator.TranslateAsync(text, MapApiCode(from.Id), MapApiCode(to.Id));
        ResultBox.Text = result ?? "Translation failed (offline or rate limited).";
    }

    private static string MapApiCode(string id) => id switch
    {
        "banglish-bangla" or "bangla" or "bijoy" => "bn",
        "banglish-english" or "english" => "en",
        "hindi" => "hi",
        "urdu" => "ur",
        "arabic" => "ar",
        "spanish" => "es",
        "french" => "fr",
        "german" => "de",
        "portuguese" => "pt",
        "russian" => "ru",
        "chinese" => "zh",
        "japanese" => "ja",
        "korean" => "ko",
        "indonesian" => "id",
        "malay" => "ms",
        "turkish" => "tr",
        "thai" => "th",
        "vietnamese" => "vi",
        _ => "en",
    };

    private void CopyBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ResultBox.Text))
            Clipboard.SetText(ResultBox.Text);
    }

    private void EnableHook_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _settings.AutoTranslateEnabled = EnableHookBox.IsChecked == true;
        _settings.Save();
        ((App)Application.Current).Hook.Enabled = _settings.AutoTranslateEnabled;
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Hide();
}
