using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VampireKeyboard.Services;

namespace VampireKeyboard;
public partial class TopBarWindow : Window
{
    private readonly AppSettings _settings;
    private bool _loadingLangs;

    public TopBarWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        PositionTopRight();
        LoadLanguages();
        UpdateLabel();
        EnableBox.IsChecked = _settings.AutoTranslateEnabled;
        if (_settings.SelectedLanguage == "bijoy") ModeBijoy.IsChecked = true;
        else if (_settings.AutoTranslateEnabled) ModeConvert.IsChecked = true;
        else ModeNormal.IsChecked = true;
    }

    private void PositionTopRight()
    {
        Left = SystemParameters.WorkArea.Right - Width - 12;
        Top = SystemParameters.WorkArea.Top + 8;
    }

    private void LoadLanguages()
    {
        _loadingLangs = true;
        LangList.Items.Clear();
        foreach (var lang in LanguageDef.All)
        {
            var item = new ListBoxItem
            {
                Content = $"{lang.Flag}  {lang.DisplayName}",
                Tag = lang,
                Padding = new Thickness(10, 7, 10, 7),
                FontSize = 13,
            };
            if (lang.Id == _settings.SelectedLanguage) item.IsSelected = true;
            LangList.Items.Add(item);
        }
        _loadingLangs = false;
    }

    private void UpdateLabel()
    {
        var lang = LanguageDef.All.FirstOrDefault(l => l.Id == _settings.SelectedLanguage)
                   ?? LanguageDef.All[0];
        string shortName = lang.Id switch
        {
            "banglish-bangla" => "BN",
            "banglish-english" => "BN-EN",
            "bijoy" => "Bijoy",
            "bangla" => "Bangla",
            "english" => "EN",
            _ => lang.Flag,
        };
        StatusText.Text = $"{shortName} v";
        OnOffText.Text = _settings.AutoTranslateEnabled ? "[ON]" : "[OFF]";
    }

    private void Bar_MouseDown(object sender, MouseButtonEventArgs e)
    {
    }

    private void Lang_Click(object sender, MouseButtonEventArgs e)
    {
        LangPopup.IsOpen = !LangPopup.IsOpen;
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        LangPopup.IsOpen = false;
    }

    private void LangList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingLangs) return;
        if (LangList.SelectedItem is ListBoxItem li && li.Tag is LanguageDef lang)
        {
            _settings.SelectedLanguage = lang.Id;
            _settings.Save();

            var app = (App)Application.Current;
            app.Hook.CurrentLanguage = lang.Id;

            UpdateLabel();
        }
    }

    private void Mode_Changed(object sender, RoutedEventArgs e)
    {
        if (EnableBox == null) return;
        var app = (App)Application.Current;

        if (ModeBijoy.IsChecked == true)
        {
            _settings.SelectedLanguage = "bijoy";
            _settings.Save();
            app.Hook.CurrentLanguage = "bijoy";
            app.Hook.Enabled = true;
            EnableBox.IsChecked = true;
            LoadLanguages();
        }
        else
        {
            bool convert = ModeConvert.IsChecked == true;
            _settings.AutoTranslateEnabled = convert;
            _settings.Save();
            app.Hook.Enabled = convert;
            EnableBox.IsChecked = convert;
        }
        UpdateLabel();
    }

    private void Enable_Changed(object sender, RoutedEventArgs e)
    {
        if (ModeConvert == null) return;
        bool on = EnableBox.IsChecked == true;
        _settings.AutoTranslateEnabled = on;
        _settings.Save();

        var app = (App)Application.Current;
        app.Hook.Enabled = on;
        if (on && _settings.SelectedLanguage != "bijoy") ModeConvert.IsChecked = true;
        else if (!on) ModeNormal.IsChecked = true;
        UpdateLabel();
    }

    private void More_Click(object sender, RoutedEventArgs e)
    {
        LangPopup.IsOpen = false;
        TrayIconService.OpenSettings();
    }

    private void Logo_Click(object sender, MouseButtonEventArgs e)
    {
        TrayIconService.OpenSettings();
    }
}