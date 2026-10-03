using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VampireKeyboard.Services;
public class KeyboardHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _proc;
    private readonly List<char> _buffer = new();
    private readonly object _lock = new();

    public bool Enabled { get; set; } = true;
    public string CurrentLanguage { get; set; } = "banglish-bangla";
    public bool BijoyMode { get; set; }
    public event Action<string, string>? WordConverted;

    public void Install()
    {
        _proc = HookProc;
        using var curProc = Process.GetCurrentProcess();
        using var mod = curProc.MainModule!;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(mod.ModuleName), 0);
    }

    public void Uninstall()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && Enabled &&
            (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            var hook = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var vk = (int)hook.vkCode;
            bool isModifier = vk is 0x10 or 0x11 or 0x12 or 0xA0 or 0xA1;
            if (!isModifier && (ModifierKeysDown() || vk is < 0x30 and not 0x20 and not 0x0D and not 0x08))
            {
                lock (_lock) _buffer.Clear();
                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            if (vk == 0x20)
            {
                string typed = FlushBuffer();
                if (!string.IsNullOrEmpty(typed))
                {
                    string? converted = CurrentLanguage switch
                    {
                        "banglish-bangla" => ConvertBanglishWord(typed),
                        "hindi" => MultiLangTransliterator.TryTransliterate("hindi", typed),
                        "urdu" => MultiLangTransliterator.TryTransliterate("urdu", typed),
                        "bijoy" => BijoyConverter.TryConvert(typed),
                        _ => null,
                    };
                    if (converted != null && converted != typed)
                    {
                        ReplaceLastChars(typed.Length, converted + " ");
                        WordConverted?.Invoke(typed, converted);
                        return CallNextHookEx(_hookId, nCode, wParam, lParam);
                    }
                }
            }
            else if (vk == 0x0D)
            {
                lock (_lock) _buffer.Clear();
            }
            else if (vk == 0x08)
            {
                lock (_lock)
                {
                    if (_buffer.Count > 0) _buffer.RemoveAt(_buffer.Count - 1);
                }
            }
            else if (vk >= 0x30 && vk <= 0x5A)
            {
                bool shift = (GetKeyState(0x10) & 0x8000) != 0;
                char c = vk switch
                {
                    >= 0x41 and <= 0x5A => (char)(shift ? vk : vk + 0x20),
                    _ => (char)vk,
                };
                lock (_lock)
                {
                    if (_buffer.Count < 64) _buffer.Add(c);
                }
            }
            else
            {
                lock (_lock) _buffer.Clear();
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private static bool ModifierKeysDown() =>
        (GetKeyState(0x11) & 0x8000) != 0 || (GetKeyState(0x12) & 0x8000) != 0;

    private static string? ConvertBanglishWord(string word)
    {
        if (SmartWordHandler.LooksLikeTechnicalTerm(word))
            return null;

        if (SmartWordHandler.IsCommonEnglishWord(word))
            return null;

        return BanglishTransliterator.TransliterateWord(word);
    }

    private string FlushBuffer()
    {
        lock (_lock)
        {
            var s = new string(_buffer.ToArray());
            _buffer.Clear();
            return s;
        }
    }
    private static void ReplaceLastChars(int count, string replacement)
    {
        var inputs = new List<INPUT>();
        for (int i = 0; i < count; i++)
        {
            inputs.Add(KeyInput(0x08, true));
            inputs.Add(KeyInput(0x08, false));
        }
        foreach (var ch in replacement)
        {
            inputs.Add(CharInput(ch, true));
            inputs.Add(CharInput(ch, false));
        }

        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
    }

    private static INPUT KeyInput(ushort vk, bool up) => new()
    {
        type = 1,
        U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, dwFlags = up ? 2u : 0u } },
    };

    private static INPUT CharInput(char c, bool up) => new()
    {
        type = 1,
        U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = c, dwFlags = (up ? 2u : 0u) | 0x0004 } },
    };

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public void Dispose() => Uninstall();
}
