namespace VampireKeyboard.Services;
public class LanguageDef
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Flag { get; }

    public LanguageDef(string id, string displayName, string flag)
    {
        Id = id;
        DisplayName = displayName;
        Flag = flag;
    }

    public static readonly LanguageDef[] All =
    {
        new("banglish-bangla", "Banglish â†’ à¦¬à¦¾à¦‚à¦²à¦¾ (Bangladesh)", "ðŸ‡§ðŸ‡©"),
        new("banglish-english", "Banglish â†’ English", "ðŸ‡¬ðŸ‡§"),
        new("bijoy", "Bijoy/Avro Compatible Mode", "⌨"),
        new("bangla", "বাংলা (Bangla direct)", "🇧🇩"),
        new("english", "English", "ðŸ‡ºðŸ‡¸"),
        new("hindi", "à¤¹à¤¿à¤¨à¥à¤¦à¥€ (Hindi, India)", "ðŸ‡®ðŸ‡³"),
        new("urdu", "Ø§Ø±Ø¯Ùˆ (Urdu, Pakistan)", "ðŸ‡µðŸ‡°"),
        new("arabic", "Ø§Ù„Ø¹Ø±Ø¨ÙŠØ© (Arabic)", "ðŸ‡¸ðŸ‡¦"),
        new("spanish", "EspaÃ±ol (Spanish)", "ðŸ‡ªðŸ‡¸"),
        new("french", "FranÃ§ais (French)", "ðŸ‡«ðŸ‡·"),
        new("german", "Deutsch (German)", "ðŸ‡©ðŸ‡ª"),
        new("portuguese", "PortuguÃªs (Portuguese, Brazil)", "ðŸ‡§ðŸ‡·"),
        new("russian", "Ð ÑƒÑÑÐºÐ¸Ð¹ (Russian)", "ðŸ‡·ðŸ‡º"),
        new("chinese", "ä¸­æ–‡ (Chinese)", "ðŸ‡¨ðŸ‡³"),
        new("japanese", "æ—¥æœ¬èªž (Japanese)", "ðŸ‡¯ðŸ‡µ"),
        new("korean", "í•œêµ­ì–´ (Korean)", "ðŸ‡°ðŸ‡·"),
        new("indonesian", "Bahasa Indonesia", "ðŸ‡®ðŸ‡©"),
        new("malay", "Bahasa Melayu (Malay)", "ðŸ‡²ðŸ‡¾"),
        new("turkish", "TÃ¼rkÃ§e (Turkish)", "ðŸ‡¹ðŸ‡·"),
        new("thai", "à¹„à¸—à¸¢ (Thai)", "ðŸ‡¹ðŸ‡­"),
        new("vietnamese", "Tiáº¿ng Viá»‡t (Vietnamese)", "ðŸ‡»ðŸ‡³"),
    };
}
