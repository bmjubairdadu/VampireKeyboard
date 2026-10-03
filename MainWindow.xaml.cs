using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using VampireKeyboard.Services;

namespace VampireKeyboard;
public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private LanguageDef? _chosen;
    private int _step;

    public MainWindow()
    {
        InitializeComponent();
        _settings = AppSettings.Load();

        if (_settings.SetupCompleted)
        {
            FinishSetup(openSettingsWindow: false);
            return;
        }

        ShowStep(0);
    }

    private void ShowStep(int step)
    {
        _step = step;
        WelcomePanel.Visibility = step == 0 ? Visibility.Visible : Visibility.Collapsed;
        LanguagePanel.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
        DonePanel.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
        BackBtn.Visibility = step is 1 or 2 ? Visibility.Visible : Visibility.Collapsed;
        NextBtn.Content = step switch
        {
            0 => "Next",
            _ => "Install & Finish",
        };
        NextBtn.Visibility = step == 2 ? Visibility.Collapsed : Visibility.Visible;
        StartBtn.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;

        if (step == 1)
            LoadLanguages();
    }

    private void LoadLanguages()
    {
        LanguageList.Items.Clear();
        foreach (var lang in LanguageDef.All)
        {
            var item = new ListBoxItem
            {
                Content = $"{lang.Flag}  {lang.DisplayName}",
                Tag = lang,
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 15,
            };
            if (lang.Id == _settings.SelectedLanguage) item.IsSelected = true;
            LanguageList.Items.Add(item);
        }
    }

    private void LanguageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageList.SelectedItem is ListBoxItem li && li.Tag is LanguageDef lang)
            _chosen = lang;
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_step > 0) ShowStep(_step - 1);
    }

    private void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_step == 0)
        {
            ShowStep(1);
            return;
        }

        if (_step == 1)
        {
            if (LanguageList.SelectedItem is ListBoxItem li && li.Tag is LanguageDef lang)
                _chosen = lang;

            if (_chosen == null)
            {
                MessageBox.Show("Please choose a language first.", "Vampire Keyboard",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _settings.SelectedLanguage = _chosen.Id;
            _settings.SetupCompleted = true;
            _settings.StartWithWindows = EnableAutoStartBox.IsChecked == true;
            _settings.Save();

            if (_settings.StartWithWindows)
                RegisterAutoStart();

            ShortcutCreator.CreateDesktopShortcut("Vampire Keyboard");

            ShowStep(2);
        }
    }

    private void StartBtn_Click(object sender, RoutedEventArgs e)
    {
        FinishSetup(openSettingsWindow: true);
    }

    private void FinishSetup(bool openSettingsWindow)
    {
        var app = (App)Application.Current;
        app.StartRunning(_settings);

        if (openSettingsWindow)
            TrayIconService.OpenSettings();
        Hide();
    }

    private void RegisterAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
            key?.SetValue("VampireKeyboard", Process.GetCurrentProcess().MainModule!.FileName!);
        }
        catch { }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (Application.Current.Properties["Running"] is true)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnClosing(e);
    }
}