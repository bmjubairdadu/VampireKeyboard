# 🧛 Vampire Keyboard

<p align="center">
  <img src="Assets/dracula.png" width="128" alt="Vampire Keyboard Logo"/>
</p>

**Type Banglish, get বাংলা — instantly, in any Windows application.**

Vampire Keyboard is a multilingual keyboard helper for Windows built for Bangladeshi developers, writers, and everyday users. Type Romanized Bangla (Banglish) anywhere — MS Office, Chrome, VS Code, Notepad — and each word converts to proper Bangla the moment you hit Space. It also supports direct Bangla ⇄ English translation, a Bijoy/Avro compatible mode for official work, and 20+ world languages.

---

## ✨ Features

- ⚡ **Live Banglish → বাংলা conversion** — works system-wide in every app, fully **offline**
- 🧠 **Smart phonetic engine** — 200+ word dictionary, hard letters (ট/ড/ণ), hasanta conjuncts (`sKul` → স্কুল), case-sensitive rules
- 🏛 **Bijoy/Avro Compatible Mode** — for official documents and legacy editors
- 🌍 **20+ languages** — Hindi, Urdu, Arabic, Spanish, French, Chinese, Japanese and more
- 🔁 **Direct translator** — Google Translate with automatic offline fallback
- 📴 **Works offline** — transliteration never needs internet; translations cache locally
- 🖱 **Draggable top bar** — click to switch language, toggle typing mode, ON/OFF
- 🛡 **Runs as Administrator** — reliable hooks even in elevated apps
- 🚀 **Auto-start with Windows**, desktop shortcut, professional installer

## 📦 Installation

### Installer (recommended)

1. Download **`VampireKeyboard-Setup.exe`** from the [Releases](../../releases) page
2. Run it → accept the UAC prompt (admin required)
3. Choose options (desktop shortcut, start with Windows)
4. Launch → complete the setup wizard → pick your language
5. Start typing Banglish anywhere!

> **System requirements:** Windows 10/11 (x64). No .NET installation needed — the runtime is bundled.

### Manual

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

## ⌨️ Usage Examples

| You type | You get |
|---|---|
| `ami bhalo achi` | আমি ভালো আছি |
| `ami tomake bhalobashi` | আমি তোমাকে ভালোবাসি |
| `taka koto dorkar` | টাকা কত দরকার |
| `dhonnobad vai` | ধন্যবাদ ভাই |
| `Taka` / `sKul` | টাকা / স্কুল (case tricks) |

## 🛠 Building from Source

**Prerequisites:** .NET 10 SDK, Windows

```powershell
# Restore & build
dotnet build -c Release

# Publish self-contained single exe
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

# Build installer (requires Inno Setup 6)
ISCC.exe installer.iss
```

## 📁 Project Structure

```
VampireKeyboard/
├── App.xaml / App.xaml.cs           # Application entry, single-instance, error handling
├── MainWindow.xaml(.cs)             # Setup wizard (language selection, install)
├── TopBarWindow.xaml(.cs)           # Draggable top bar with language menu
├── SettingsWindow.xaml(.cs)         # Settings + direct translator UI
├── Assets/                          # Logo (dracula.png / .ico)
├── Services/
│   ├── BanglishTransliterator.cs    # Banglish → Bangla phonetic engine
│   ├── MultiLangTransliterator.cs   # Hindi / Urdu phonetic support
│   ├── BijoyConverter.cs            # Bijoy/Avro compatible conversion
│   ├── KeyboardHookService.cs       # Global low-level keyboard hook
│   ├── TranslationService.cs        # Google Translate + offline fallback
│   ├── OfflineDictionary.cs         # Persistent offline translation cache
│   ├── AppSettings.cs               # Persisted settings
│   ├── TrayIconService.cs           # System tray icon & menu
│   ├── LanguageDef.cs               # Supported language definitions
│   └── ShortcutCreator.cs           # Desktop shortcut creation
└── installer.iss                    # Inno Setup installer script
```

## 🗺 Roadmap

- [ ] Word suggestions / autocomplete popup
- [ ] Voice input
- [ ] Cloud sync of personal dictionary
- [ ] macOS & Linux support

## 🤝 Contributing

Contributions are welcome! Please open an issue or pull request — especially for new Banglish dictionary words and transliteration fixes.

## 📄 License

This project is licensed under the [MIT License](LICENSE).
