using System.Text;

namespace VampireKeyboard.Services;

public static class MultiLangTransliterator
{
    private static readonly (string Roman, string Out)[] HindiMap =
    {
        ("kha", "ख"), ("gha", "घ"), ("cha", "छ"), ("jha", "झ"),
        ("tha", "थ"), ("dha", "ध"), ("pha", "फ"), ("bha", "भ"),
        ("sha", "श"), ("chha", "छ"),
        ("aa", "ा"), ("ee", "ी"), ("oo", "ु"), ("ai", "ै"), ("au", "ौ"),
        ("kh", "ख"), ("gh", "घ"), ("ch", "च"), ("jh", "झ"),
        ("th", "थ"), ("dh", "ध"), ("ph", "फ"), ("bh", "भ"), ("sh", "श"),
        ("k", "क"), ("g", "ग"), ("c", "च"), ("j", "ज"), ("t", "त"),
        ("d", "द"), ("n", "न"), ("p", "प"), ("f", "फ"), ("b", "ब"),
        ("v", "व"), ("m", "म"), ("y", "य"), ("r", "र"), ("l", "ल"),
        ("s", "स"), ("h", "ह"),
        ("a", "ा"), ("i", "ि"), ("u", "ु"), ("e", "े"), ("o", "ो"),
    };

    private static readonly Dictionary<string, string> HindiWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["namaste"] = "नमस्ते", ["dhanyavad"] = "धन्यवाद", ["kaise"] = "कैसे",
        ["ho"] = "हो", ["aap"] = "आप", ["main"] = "मैं", ["hai"] = "है",
        ["pyar"] = "प्यार", ["pani"] = "पानी", ["kya"] = "क्या",
    };

    private static readonly (string Roman, string Out)[] UrduMap =
    {
        ("kha", "کھ"), ("gha", "غ"), ("chha", "چھ"), ("tha", "تھ"),
        ("dha", "دھ"), ("pha", "پھ"), ("bha", "بھ"), ("sha", "ش"),
        ("kh", "خ"), ("gh", "غ"), ("ch", "چ"), ("zh", "ژ"),
        ("th", "تھ"), ("dh", "دھ"), ("ph", "پھ"), ("bh", "بھ"), ("sh", "ش"),
        ("k", "ک"), ("g", "گ"), ("c", "چ"), ("j", "ج"), ("t", "ت"),
        ("d", "د"), ("n", "ن"), ("p", "پ"), ("f", "ف"), ("b", "ب"),
        ("v", "و"), ("m", "م"), ("y", "ی"), ("r", "ر"), ("l", "ل"),
        ("s", "س"), ("h", "ہ"), ("q", "ق"), ("w", "و"), ("z", "ز"),
        ("a", "ا"), ("i", "ی"), ("u", "و"), ("e", "ے"), ("o", "و"),
    };

    private static readonly Dictionary<string, string> UrduWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["salam"] = "سلام", ["shukriya"] = "شکریہ", ["kaise"] = "کیسے",
        ["ho"] = "ہو", ["aap"] = "آپ", ["main"] = "میں", ["hai"] = "ہے",
        ["mohabbat"] = "محبت", ["pani"] = "پانی", ["kya"] = "کیا",
    };

    public static string? TryTransliterate(string langId, string word)
    {
        return langId switch
        {
            "hindi" => Generic(word, HindiMap, HindiWords),
            "urdu" => Generic(word, UrduMap, UrduWords),
            _ => null,
        };
    }

    private static string Generic(string word, (string, string)[] map, Dictionary<string, string> words)
    {
        if (word.Length == 0) return word;
        if (words.TryGetValue(word.ToLowerInvariant(), out var w)) return w;

        var sb = new StringBuilder();
        int i = 0;
        bool lastWasConsonant = false;
        while (i < word.Length)
        {
            string best = ""; string bangla = ""; int len = 0;
            foreach (var (roman, outc) in map)
            {
                if (i + roman.Length <= word.Length &&
                    string.Compare(word, i, roman, 0, roman.Length, StringComparison.OrdinalIgnoreCase) == 0 &&
                    roman.Length > len)
                {
                    best = roman; bangla = outc; len = roman.Length;
                }
            }
            if (best.Length == 0)
            {
                sb.Append(word[i]); i++; lastWasConsonant = false; continue;
            }
            bool isVowel = bangla is "ा" or "ि" or "ु" or "े" or "ो" or "ै" or "ौ" or "ी";
            if (isVowel && !lastWasConsonant)
            {
                sb.Append(bangla switch
                {
                    "ा" => "आ", "ि" or "ी" => "इ", "ु" => "उ",
                    "े" => "ए", "ो" => "ओ", "ै" => "ऐ", "ौ" => "औ",
                    _ => bangla,
                });
            }
            else sb.Append(bangla);
            lastWasConsonant = !isVowel;
            i += len;
        }
        return sb.ToString();
    }
}
