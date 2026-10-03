using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VampireKeyboard.Services;

public static class ShortcutCreator
{
    [DllImport("ole32.dll")]
    private static extern int CoInitialize(IntPtr pvReserved);

    [DllImport("ole32.dll")]
    private static extern void CoUninitialize();

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(ref Guid clsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, out IntPtr ppv);

    public static void CreateDesktopShortcut(string appName)
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return;

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string linkPath = Path.Combine(desktop, $"{appName}.lnk");
            if (File.Exists(linkPath)) return;

            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "dracula.ico");
            if (!File.Exists(iconPath)) iconPath = exePath;

            CreateShortcut(linkPath, exePath, iconPath, appName, "Vampire Keyboard — Type Banglish anywhere");
        }
        catch { }
    }

    private static void CreateShortcut(string linkPath, string targetPath, string iconPath, string description, string tooltip)
    {
        Type? t = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"));
        if (t == null) return;

        dynamic? shell = null;
        try
        {
            shell = Activator.CreateInstance(t);
            if (shell == null) return;
            var shortcut = shell.CreateShortcut(linkPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath) ?? "";
            shortcut.IconLocation = iconPath;
            shortcut.Description = tooltip;
            shortcut.Save();
        }
        catch { }
        finally
        {
            if (shell != null) Marshal.ReleaseComObject(shell);
        }
    }
}
