using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace VampireKeyboard.Services;
public class TrayIconService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    private WinForms.NotifyIcon? _icon;
    private readonly KeyboardHookService _hook;
    private readonly AppSettings _settings;

    public TrayIconService(KeyboardHookService hook, AppSettings settings)
    {
        _hook = hook;
        _settings = settings;
    }

    public void Show()
    {
        _icon = new WinForms.NotifyIcon
        {
            Text = "Vampire Keyboard",
            Visible = true,
        };

        try
        {
            var uri = new Uri("pack://application:,,,/Assets/dracula.png");
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo != null)
                _icon.Icon = new Icon(streamInfo.Stream);
        }
        catch
        {
        }

        var menu = new WinForms.ContextMenuStrip();

        var headerItem = new WinForms.ToolStripMenuItem("🧛 Vampire Keyboard v1.0")
        {
            Enabled = false,
        };

        var toggle = new WinForms.ToolStripMenuItem("Enable Keyboard Conversion")
        {
            Checked = _hook.Enabled,
            CheckOnClick = true,
        };
        toggle.CheckedChanged += (_, _) =>
        {
            _hook.Enabled = toggle.Checked;
            _settings.AutoTranslateEnabled = toggle.Checked;
            _settings.Save();
            toggle.Text = toggle.Checked ? "Enable Keyboard Conversion" : "Keyboard Conversion is OFF";
        };

        var settingsItem = new WinForms.ToolStripMenuItem("⚙ Settings / Language");
        settingsItem.Click += (_, _) => OpenSettings();

        var helpItem = new WinForms.ToolStripMenuItem("❓ How to Use");
        helpItem.Click += (_, _) => ShowHelp();

        var aboutItem = new WinForms.ToolStripMenuItem("ℹ About");
        aboutItem.Click += (_, _) => ShowAbout();

        var exitItem = new WinForms.ToolStripMenuItem("✖ Exit");
        exitItem.Click += (_, _) =>
        {
            _hook.Uninstall();
            _icon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        };

        menu.Items.Add(headerItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(toggle);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(helpItem);
        menu.Items.Add(aboutItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _icon.ContextMenuStrip = menu;
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                OpenSettings();
        };
    }

    public static void OpenSettings()
    {
        var existing = System.Windows.Application.Current.Windows
            .OfType<System.Windows.Window>()
            .FirstOrDefault(w => w is SettingsWindow);
        if (existing != null)
        {
            existing.Activate();
            return;
        }
        var win = new SettingsWindow();
        win.Show();
        win.Activate();
    }

    private static void ShowHelp()
    {
        MessageBox.Show(
            "How to use Vampire Keyboard:\n\n" +
            "1. Click the language text on the top bar → choose your language\n" +
            "2. Type Banglish in ANY app (Notepad, Word, Chrome, Office...)\n" +
            "3. Press Space after each word → it instantly becomes বাংলা\n\n" +
            "Examples:\n" +
            "   ami bhalo achi  →  আমি ভালো আছি\n" +
            "   taka koto dorkar  →  টাকা কত দরকার\n\n" +
            "Bijoy/Avro users: choose 'Bijoy/Avro Compatible Mode' from the top bar menu for official work.\n\n" +
            "Tips:\n" +
            "• Capital T/D/N means hard letters: Taka → টাকা, Dhaka → ঢাকা\n" +
            "• Top bar can be dragged anywhere on screen\n" +
            "• Right-click the tray icon for more options",
            "Vampire Keyboard — Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void ShowAbout()
    {
        MessageBox.Show(
            "Vampire Keyboard v1.0.0\n\n" +
            "Type in your own language — anywhere in Windows.\n\n" +
            "Made with 🖤 for Bangladeshi developers and users worldwide.\n\n" +
            "Features:\n" +
            "• Banglish → বাংলা live conversion (offline)\n" +
            "• 20+ languages supported\n" +
            "• Bijoy/Avro compatible mode\n" +
            "• Direct translation (Google Translate + offline fallback)\n" +
            "• Works in every application",
            "About Vampire Keyboard", MessageBoxButton.OK, MessageBoxImage.None);
    }

    public void Dispose()
    {
        if (_icon != null)
        {
            _icon.Visible = false;
            _icon.Dispose();
        }
    }
}
