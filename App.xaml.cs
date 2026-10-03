using System.Threading;
using System.Windows;
using VampireKeyboard.Services;

namespace VampireKeyboard;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;
    public AppSettings Settings { get; private set; } = new();
    public KeyboardHookService Hook { get; private set; } = new();
    private TrayIconService? _tray;
    private TopBarWindow? _topBar;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(true, "VampireKeyboard_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Vampire Keyboard is already running.\n\nLook for the Vampire icon in the system tray (near the clock).",
                "Vampire Keyboard", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                "Something went wrong, but your data is safe.\n\n" +
                "Technical details:\n" + args.Exception.Message,
                "Vampire Keyboard", MessageBoxButton.OK, MessageBoxImage.Warning);
            args.Handled = true;
        };

        base.OnStartup(e);
        Settings = AppSettings.Load();
        Hook.CurrentLanguage = Settings.SelectedLanguage;
        Hook.Enabled = Settings.AutoTranslateEnabled;
    }
    public void StartRunning(AppSettings settings)
    {
        Settings = settings;
        Hook.CurrentLanguage = settings.SelectedLanguage;
        Hook.Enabled = settings.AutoTranslateEnabled;
        Hook.Install();

        _tray = new TrayIconService(Hook, settings);
        _tray.Show();

        _topBar = new TopBarWindow(settings);
        _topBar.Show();

        Properties["Running"] = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Hook.Uninstall();
        _tray?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}

