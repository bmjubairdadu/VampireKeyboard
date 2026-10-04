using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VampireKeyboard.Services;

namespace VampireKeyboard;
public partial class TopBarWindow : Window
{
    private readonly AppSettings _settings;
    private bool _loading;

    public TopBarWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        PositionTopRight();
        LoadFromTo();
        UpdateLabel();
        EnableBox.IsChecked = _settings.AutoTranslateEnabled;
    }

    private void PositionTopRight()
    {
        Left = SystemParameters.WorkArea.Right - Width - 12;
        Top = SystemParameters.WorkArea.Top + 8;
    }

    private void LoadFromTo()
    {
        _loading = true;

        var fromOptions = new[]
        {
            new KeyValuePair<string, string>("banglish-bangla", "Banglish (Roman)"),
            new KeyValuePair<string, string>("english", "English"),
            new KeyValuePair<string, string>("hindi", "Hindi (Roman)"),
            new KeyValuePair<string, string>("urdu", "Urdu (Roman)"),
        };
        var toOptions = new[]
        {
            new KeyValuePair<string, string>("bangla", "Bangla"),
            new KeyValuePair<string, string>("english", "English"),
            new KeyValuePair<string, string>("hindi", "Hindi"),
            new KeyValuePair<string, string>("urdu", "Urdu"),
        };

        FromBox.Items.Clear();
        foreach (var o in fromOptions)
            FromBox.Items.Add(new ComboBoxItem { Content = o.Value, Tag = o.Key });

        ToBox.Items.Clear();
        foreach (var o in toOptions)
            ToBox.Items.Add(new ComboBoxItem { Content = o.Value, Tag = o.Key });

        int fi = fromOptions.ToList().FindIndex(o => o.Key == _settings.FromLanguage);
        FromBox.SelectedIndex = fi >= 0 ? fi : 0;
        int ti = toOptions.ToList().FindIndex(o => o.Key == _settings.ToLanguage);
        ToBox.SelectedIndex = ti >= 0 ? ti : 0;

        _loading = false;
    }

    private void UpdateLabel()
    {
        string from = _settings.FromLanguage switch
        {
            "banglish-bangla" => "BN-Roman",
            "english" => "EN",
            "hindi" => "HI-Roman",
            "urdu" => "UR-Roman",
            _ => "BN",
        };
        string to = _settings.ToLanguage switch
        {
            "bangla" => "BN",
            "english" => "EN",
            "hindi" => "HI",
            "urdu" => "UR",
            _ => "BN",
        };
        StatusText.Text = $"{from} > {to}";
        OnOffText.Text = _settings.AutoTranslateEnabled ? "[ON]" : "[OFF]";
    }

    private void From_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || FromBox.SelectedItem is not ComboBoxItem fi || fi.Tag is not string from) return;
        _settings.FromLanguage = from;
        _settings.Save();
        ((App)Application.Current).Hook.FromLanguage = from;
        UpdateLabel();
    }

    private void To_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ToBox.SelectedItem is not ComboBoxItem ti || ti.Tag is not string to) return;
        _settings.ToLanguage = to;
        _settings.Save();
        ((App)Application.Current).Hook.ToLanguage = to;
        UpdateLabel();
    }

    private void Enable_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        bool on = EnableBox.IsChecked == true;
        _settings.AutoTranslateEnabled = on;
        _settings.Save();
        ((App)Application.Current).Hook.Enabled = on;
        UpdateLabel();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        LangPopup.IsOpen = false;
    }

    private void Logo_Click(object sender, MouseButtonEventArgs e)
    {
        LangPopup.IsOpen = !LangPopup.IsOpen;
    }
}
