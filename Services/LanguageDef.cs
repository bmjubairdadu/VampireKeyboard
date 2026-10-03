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
        new("banglish-bangla", "Banglish to Bangla (Bangladesh)", "BD"),
        new("banglish-english", "Banglish to English", "EN"),
        new("bijoy", "Bijoy/Avro Compatible Mode", "BJ"),
        new("bangla", "Bangla direct", "BD"),
        new("english", "English", "US"),
        new("hindi", "Hindi (India)", "IN"),
        new("urdu", "Urdu (Pakistan)", "PK"),
        new("arabic", "Arabic", "SA"),
        new("spanish", "Spanish", "ES"),
        new("french", "French", "FR"),
        new("german", "German", "DE"),
        new("portuguese", "Portuguese (Brazil)", "BR"),
        new("russian", "Russian", "RU"),
        new("chinese", "Chinese", "CN"),
        new("japanese", "Japanese", "JP"),
        new("korean", "Korean", "KR"),
        new("indonesian", "Indonesian", "ID"),
        new("malay", "Malay", "MY"),
        new("turkish", "Turkish", "TR"),
        new("thai", "Thai", "TH"),
        new("vietnamese", "Vietnamese", "VN"),
    };
}
