using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using VampireKeyboard.Services;

namespace VampireKeyboard;
public partial class MainWindow : Window
{
    private readonly AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();
        _settings = AppSettings.Load();

        if (_settings.SetupCompleted)
        {
            FinishSetup();
            return;
        }
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        _settings.SetupCompleted = true;
        _settings.StartWithWindows = EnableAutoStartBox.IsChecked == true;
        _settings.Save();

        if (_settings.StartWithWindows)
            RegisterAutoStart();

        FinishSetup();
    }

    private void FinishSetup()
    {
        var app = (App)Application.Current;
        app.StartRunning(_settings);
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
