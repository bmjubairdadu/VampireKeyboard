using System.Collections.Generic;

namespace VampireKeyboard.Services;

public static class BijoyConverter
{
    private static readonly Dictionary<string, string> AsciiToBangla = new()
    {
        ["k"] = "ক", ["K"] = "ক্ষ", ["g"] = "গ", ["G"] = "ঘ", ["\u00f1"] = "ঙ",
        ["c"] = "চ", ["C"] = "ছ", ["j"] = "জ", ["J"] = "ঝ", ["\\"] = "ঞ",
        ["T"] = "ট", ["™"] = "ঠ", ["D"] = "ড", [".]"] = "ঢ", ["N"] = "ণ",
        ["t"] = "ত", ["‹"] = "থ", ["d"] = "দ", ["-"] = "ধ", ["n"] = "ন",
        ["p"] = "প", ["=F"] = "ফ", ["b"] = "ব", ["v"] = "ভ", ["m"] = "ম",
        ["Z"] = "য", ["R"] = "র", ["\\'"] = "ড়", ["l"] = "ল", ["S"] = "শ",
        ["s"] = "স", ["$"] = "ষ", ["h"] = "হ", ["@"] = "ড়",
        ["yy"] = "য্য", ["y"] = "য়",
        ["a"] = "া", ["i"] = "ি", ["\\."] = "ী", ["u"] = "ু", ["\\U"] = "ূ",
        ["\\["] = "ৃ", ["e"] = "ে", ["o"] = "ো", ["O"] = "ৌ",
        ["A"] = "আ", ["B"] = "ই", ["C"] = "উ", ["E"] = "এ", ["F"] = "ও",
        ["w"] = "্", ["W"] = "ঁ", ["^"] = "ৎ",
        ["0"] = "০", ["1"] = "১", ["2"] = "২", ["3"] = "৩", ["4"] = "৪",
        ["5"] = "৫", ["6"] = "৬", ["7"] = "৭", ["8"] = "৮", ["9"] = "৯",
    };

    public static string? TryConvert(string typed)
    {
        if (string.IsNullOrEmpty(typed)) return null;
        if (typed.Any(c => c > 0x7F && c < 0x980)) return null;
        if (typed.All(c => !char.IsLetter(c))) return null;

        var result = new System.Text.StringBuilder();
        bool anyConverted = false;
        int i = 0;

        while (i < typed.Length)
        {
            bool converted = false;
            foreach (var len in new[] { 2, 1 })
            {
                if (i + len <= typed.Length &&
                    AsciiToBangla.TryGetValue(typed.Substring(i, len), out var bn))
                {
                    result.Append(bn);
                    i += len;
                    converted = true;
                    anyConverted = true;
                    break;
                }
            }
            if (!converted)
            {
                result.Append(typed[i]);
                i++;
            }
        }

        return anyConverted ? result.ToString() : null;
    }
}
