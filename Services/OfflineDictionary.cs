using System.IO;
using System.Text.Json;

namespace VampireKeyboard.Services;

public static class OfflineDictionary
{
    private class Entry
    {
        public Dictionary<string, string> Map { get; set; } = new();
    }

    private static readonly Dictionary<string, Dictionary<string, string>> Cache = new();
    private static readonly object Lock = new();

    private static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VampireKeyboard");

    private static string FilePath(string pair) => Path.Combine(Dir, $"offline_{pair}.json");

    public static Dictionary<string, string> Load(string pair)
    {
        lock (Lock)
        {
            if (Cache.TryGetValue(pair, out var cached)) return cached;
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(FilePath(pair)))
                {
                    var entry = JsonSerializer.Deserialize<Entry>(File.ReadAllText(FilePath(pair)));
                    if (entry?.Map != null)
                        foreach (var kv in entry.Map)
                            map[kv.Key] = kv.Value;
                }
            }
            catch { }
            Cache[pair] = map;
            return map;
        }
    }

    public static void Save(string pair, Dictionary<string, string> additions)
    {
        lock (Lock)
        {
            var map = Load(pair);
            foreach (var kv in additions)
                map[kv.Key] = kv.Value;
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath(pair),
                    JsonSerializer.Serialize(new Entry { Map = map }, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }

    public static string? Lookup(string pair, string text)
    {
        var map = Load(pair);
        var key = text.Trim().ToLowerInvariant();
        if (map.Count == 0) return null;
        if (map.TryGetValue(key, out var exact)) return exact;
        return null;
    }

    public static string? LookupPhrase(string pair, string text)
    {
        var map = Load(pair);
        if (map.Count == 0) return null;
        var words = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sb = new System.Text.StringBuilder();
        bool any = false;
        foreach (var w in words)
        {
            if (map.TryGetValue(w.ToLowerInvariant(), out var v))
            {
                sb.Append(v).Append(' ');
                any = true;
            }
            else
            {
                sb.Append(w).Append(' ');
            }
        }
        return any ? sb.ToString().TrimEnd() : null;
    }
}

public static class OfflineBanglish
{
    public static string? Translate(string text)
    {
        var trimmed = text.Trim();
        var direct = OfflineDictionary.Lookup("en-bn", trimmed);
        if (direct != null) return direct;

        if (BanglishTransliterator.HasBanglishLetters(trimmed))
        {
            var bangla = BanglishTransliterator.Transliterate(trimmed);
            if (bangla != trimmed) return bangla;
            return null;
        }

        return OfflineDictionary.LookupPhrase("en-bn", trimmed);
    }
}
