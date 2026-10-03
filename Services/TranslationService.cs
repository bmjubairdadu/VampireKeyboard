using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace VampireKeyboard.Services;

public class TranslationService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public async Task<string?> TranslateAsync(string text, string fromLang, string toLang)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var online = await TranslateOnlineAsync(text, fromLang, toLang);
        if (online != null)
        {
            CacheTranslation(fromLang, toLang, text, online);
            return online;
        }
        return TranslateOffline(text, fromLang, toLang);
    }

    private static string? TranslateOffline(string text, string fromLang, string toLang)
    {
        var pair = $"{fromLang}-{toLang}";
        var trimmed = text.Trim();

        var cached = OfflineDictionary.Lookup(pair, trimmed);
        if (cached != null) return cached;

        if (fromLang == "bn" && toLang == "en")
        {
            var rev = OfflineDictionary.Lookup("en-bn", trimmed);
            if (rev == null)
            {
                foreach (var kv in OfflineDictionary.Load("en-bn"))
                {
                    if (string.Equals(kv.Value, trimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        rev = kv.Key;
                        break;
                    }
                }
            }
            return rev;
        }

        if (toLang == "bn" && (fromLang == "en" || fromLang == "bn"))
            return OfflineBanglish.Translate(trimmed);

        return cached;
    }

    private static void CacheTranslation(string fromLang, string toLang, string source, string result)
    {
        if (source.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 12) return;
        if (toLang == "bn" && (fromLang == "en" || fromLang == "bn"))
            OfflineDictionary.Save("en-bn", new Dictionary<string, string> { [source.Trim().ToLowerInvariant()] = result });
        else if (fromLang == "bn" && toLang == "en")
            OfflineDictionary.Save("bn-en", new Dictionary<string, string> { [source.Trim().ToLowerInvariant()] = result });
    }

    private static async Task<string?> TranslateOnlineAsync(string text, string fromLang, string toLang)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        try
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLang}&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";
            var json = await Http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var sb = new StringBuilder();
            foreach (var seg in doc.RootElement[0].EnumerateArray())
            {
                if (seg.ValueKind == JsonValueKind.Array && seg.GetArrayLength() > 0)
                    sb.Append(seg[0].GetString());
            }
            var result = sb.ToString();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }
}
