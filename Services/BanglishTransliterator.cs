using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VampireKeyboard.Services;

public static class BanglishTransliterator
{
    private const char Hasanta = '্';

    private static readonly (string Roman, string Bangla)[] CaseSensitiveMap =
    {
        ("Sh", "ষ"), ("Rh", "ঢ়"),
        ("T", "ট"), ("D", "ড"), ("N", "ণ"), ("R", "ড়"),
        ("S", "স"), ("Y", "য়"), ("Z", "ঝ"),
    };

    private static readonly (string Roman, string Bangla)[] Map =
    {
        ("kkh", "ক্ষ"), ("chh", "ছ"), ("ssh", "ষ"),
        ("tth", "ঠ"), ("ddh", "ঢ"), ("ngg", "ঙ্গ"), ("nch", "ঞ্চ"),
        ("kh", "খ"), ("gh", "ঘ"), ("ch", "চ"), ("jh", "ঝ"),
        ("th", "থ"), ("dh", "ধ"), ("ph", "ফ"), ("bh", "ভ"),
        ("sh", "শ"), ("ng", "ং"), ("ss", "ষ"), ("rr", "ড়"),
        ("oi", "ৈ"), ("ou", "ৌ"), ("aa", "া"), ("ee", "ী"),
        ("oo", "ু"), ("ri", "ৃ"),
        ("k", "ক"), ("g", "গ"), ("c", "চ"), ("j", "জ"),
        ("t", "ত"), ("d", "দ"), ("n", "ন"), ("p", "প"),
        ("f", "ফ"), ("b", "ব"), ("v", "ভ"), ("m", "ম"),
        ("z", "জ"), ("y", "য"), ("r", "র"), ("l", "ল"),
        ("s", "স"), ("h", "হ"), ("x", "ক্স"), ("q", "ক"),
        ("a", "া"), ("i", "ি"), ("u", "ু"), ("e", "ে"), ("o", "ো"),
    };

    private static readonly Dictionary<string, string> VowelFull = new()
    {
        ["a"] = "আ", ["aa"] = "আ", ["i"] = "ই", ["ee"] = "ই", ["u"] = "উ",
        ["oo"] = "উ", ["e"] = "এ", ["o"] = "ও", ["oi"] = "ঐ",
        ["ou"] = "ঔ", ["ri"] = "ঋ",
    };

    private static readonly HashSet<string> VowelSigns = new()
    { "া", "ি", "ী", "ু", "ূ", "ে", "ো", "ৈ", "ৌ", "ৃ" };

    private static readonly HashSet<string> HardLetters = new()
    { "ট", "ঠ", "ড", "ঢ", "ণ", "ড়", "ঢ়" };

    private static readonly Dictionary<string, string> WordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ami"] = "আমি", ["amra"] = "আমরা", ["amar"] = "আমার", ["amake"] = "আমাকে",
        ["amader"] = "আমাদের", ["tumi"] = "তুমি", ["tomake"] = "তোমাকে", ["tomar"] = "তোমার",
        ["tomer"] = "তোমার", ["apni"] = "আপনি", ["apnar"] = "আপনার", ["apnake"] = "আপনাকে",
        ["tui"] = "তুই", ["tor"] = "তোর", ["tora"] = "তোরা", ["se"] = "সে",
        ["tahar"] = "তাহার", ["tar"] = "তার", ["tara"] = "তারা", ["take"] = "তাকে",
        ["tader"] = "তাদের", ["eder"] = "এদের", ["nijer"] = "নিজের", ["nijei"] = "নিজেই",
        ["ke"] = "কে", ["kake"] = "কাকে", ["ki"] = "কি",
        ["kothay"] = "কোথায়", ["keno"] = "কেন", ["kokhono"] = "কখনো", ["kokhoni"] = "কখনোই",
        ["kivabe"] = "কিভাবে", ["koto"] = "কত", ["kotota"] = "কতটা", ["kon"] = "কোন",
        ["je"] = "যে", ["jeta"] = "যেটা", ["jegulo"] = "যেগুলো", ["jar"] = "যার",
        ["jokhon"] = "যখন", ["tokhon"] = "তখন", ["jodi"] = "যদি", ["hole"] = "হলে",
        ["jehetu"] = "যেহেতু", ["kintu"] = "কিন্তু", ["tobe"] = "তবে", ["tai"] = "তাই",
        ["ar"] = "আর", ["o"] = "ও", ["ba"] = "বা", ["to"] = "তো",
        ["na"] = "না", ["ni"] = "নি", ["abar"] = "আবার", ["ekhono"] = "এখনো",
        ["ekhane"] = "এখানে", ["okhane"] = "ওখানে", ["ekhon"] = "এখন", ["ajke"] = "আজকে",
        ["age"] = "আগে", ["pore"] = "পরে",
        ["ekdom"] = "একদম", ["onek"] = "অনেক", ["khub"] = "খুব",
        ["beshi"] = "বেশি", ["kom"] = "কম", ["hoito"] = "হয়তো",
        ["bhalo"] = "ভালো", ["valo"] = "ভালো", ["kharap"] = "খারাপ", ["boro"] = "বড়",
        ["choto"] = "ছোট", ["notun"] = "নতুন", ["purano"] = "পুরনো", ["sundor"] = "সুন্দর",
        ["mishti"] = "মিষ্টি", ["kemon"] = "কেমন", ["emon"] = "এমন",
        ["mon"] = "মন", ["kotha"] = "কথা", ["bishoy"] = "বিষয়",
        ["somoy"] = "সময়", ["shomoy"] = "সময়", ["jinish"] = "জিনিস",
        ["bepar"] = "ব্যাপার", ["dorkar"] = "দরকার", ["proyojon"] = "প্রয়োজন", ["iccha"] = "ইচ্ছা",
        ["kaj"] = "কাজ", ["dhonnobad"] = "ধন্যবাদ",
        ["thanks"] = "ধন্যবাদ", ["hello"] = "হ্যালো",
        ["hi"] = "হাই", ["sorry"] = "সরি", ["salam"] = "সালাম",
        ["nomoshkar"] = "নমস্কার", ["accha"] = "আচ্ছা", ["acha"] = "আচ্ছা", ["acchi"] = "আছি",
        ["hmm"] = "হুম", ["thik"] = "ঠিক", ["ha"] = "হ্যাঁ",
        ["ache"] = "আছে", ["acho"] = "আছো", ["achen"] = "আছেন",
        ["nei"] = "নেই", ["nai"] = "নাই", ["chilo"] = "ছিল",
        ["chilam"] = "ছিলাম", ["hobe"] = "হবে",
        ["hoy"] = "হয়", ["hoye"] = "হয়ে", ["hoyni"] = "হয়নি", ["hocche"] = "হচ্ছে",
        ["korbo"] = "করবো", ["kore"] = "করে", ["kori"] = "করি",
        ["koro"] = "করো", ["korchen"] = "করছেন", ["korchi"] = "করছি", ["korechi"] = "করেছি",
        ["korte"] = "করতে", ["korle"] = "করলে", ["korbe"] = "করবে",
        ["korben"] = "করবেন", ["korun"] = "করুন", ["korlam"] = "করলাম", ["korini"] = "করিনি",
        ["koren"] = "করেন", ["jabo"] = "যাবো", ["jachhi"] = "যাচ্ছি",
        ["jachche"] = "যাচ্ছে", ["jete"] = "যেতে", ["jabe"] = "যাবে",
        ["jawa"] = "যাওয়া", ["gese"] = "গেছে", ["geche"] = "গেছে", ["gele"] = "গেলে",
        ["giye"] = "গিয়ে", ["giyechi"] = "গিয়েছি", ["gelo"] = "গেলো", ["jai"] = "যাই",
        ["jao"] = "যাও", ["ashbo"] = "আসবো", ["ashchi"] = "আসছি", ["ashbe"] = "আসবে",
        ["ashe"] = "আসে", ["ashen"] = "আসেন", ["asha"] = "আশা", ["ashte"] = "আসতে",
        ["ashole"] = "আসলে", ["dekha"] = "দেখা", ["dekhi"] = "দেখি",
        ["dekho"] = "দেখো", ["dekhe"] = "দেখে", ["dekhte"] = "দেখতে", ["dekhbo"] = "দেখবো",
        ["dekhechi"] = "দেখেছি", ["dekhlam"] = "দেখলাম", ["dekhchen"] = "দেখছেন",
        ["bola"] = "বলা", ["bolo"] = "বলো", ["bolchi"] = "বলছি",
        ["bolen"] = "বলেন", ["bole"] = "বলে", ["bolbo"] = "বলবো",
        ["shona"] = "শোনা", ["shuno"] = "শুনো",
        ["shuni"] = "শুনি", ["shune"] = "শুনে", ["shon"] = "শোন",
        ["khai"] = "খাই", ["khao"] = "খাও", ["khabo"] = "খাবো",
        ["kheye"] = "খেয়ে", ["khabar"] = "খাবার", ["khelam"] = "খেলাম", ["khele"] = "খেলে",
        ["chai"] = "চাই", ["chao"] = "চাও",
        ["chay"] = "চায়", ["diche"] = "দিচ্ছে",
        ["dicchhi"] = "দিচ্ছি", ["dibo"] = "দিবো", ["dibe"] = "দিবে",
        ["dao"] = "দাও", ["dawa"] = "দেওয়া", ["diye"] = "দিয়ে",
        ["din"] = "দিন", ["dilo"] = "দিলো", ["dilam"] = "দিলাম",
        ["nao"] = "নাও", ["niye"] = "নিয়ে", ["nibo"] = "নিবো", ["nibe"] = "নিবে",
        ["likho"] = "লিখো", ["likhi"] = "লিখি", ["likhlam"] = "লিখলাম",
        ["lekha"] = "লেখা", ["pora"] = "পড়া", ["pori"] = "পড়ি", ["porbo"] = "পড়বো",
        ["porlam"] = "পড়লাম", ["poro"] = "পড়ো", ["porte"] = "পড়তে", ["porchi"] = "পড়ছি",
        ["cholche"] = "চলছে", ["cholbe"] = "চলবে", ["cholo"] = "চলো",
        ["chol"] = "চল", ["kichu"] = "কিছু",
        ["bujhlam"] = "বুঝলাম", ["buji"] = "বুঝি", ["bujho"] = "বুঝো", ["bujhe"] = "বুঝে",
        ["bujhechi"] = "বুঝেছি", ["bujhen"] = "বুঝেন", ["mone"] = "মনে",
        ["dhoro"] = "ধরো", ["dhori"] = "ধরি",
        ["dhore"] = "ধরে", ["rakho"] = "রাখো",
        ["rakhi"] = "রাখি", ["rakhbo"] = "রাখবো",
        ["bhai"] = "ভাই", ["vai"] = "ভাই", ["bon"] = "বোন", ["apu"] = "আপু",
        ["didi"] = "দিদি", ["dada"] = "দাদা", ["mama"] = "মামা", ["mami"] = "মামি",
        ["mashi"] = "মাসি", ["chacha"] = "চাচা", ["chachi"] = "চাচি", ["kaku"] = "কাকু",
        ["kakima"] = "কাকিমা", ["nana"] = "নানা", ["nani"] = "নানি", ["dadi"] = "দাদি",
        ["thakuma"] = "ঠাকুমা", ["ammu"] = "আম্মু", ["abbu"] = "আব্বু", ["ma"] = "মা",
        ["baba"] = "বাবা", ["bou"] = "বউ", ["jamai"] = "জামাই", ["meye"] = "মেয়ে",
        ["chele"] = "ছেলে", ["manush"] = "মানুষ", ["lok"] = "লোক", ["bondhu"] = "বন্ধু",
        ["vabi"] = "ভাবি", ["bhabi"] = "ভাবি",
        ["bangladesh"] = "বাংলাদেশ", ["bangla"] = "বাংলা", ["dhaka"] = "ঢাকা", ["desh"] = "দেশ",
        ["desher"] = "দেশের", ["bidesh"] = "বিদেশ", ["ghor"] = "ঘর", ["ghore"] = "ঘরে",
        ["bari"] = "বাড়ি", ["barite"] = "বাড়িতে", ["dokan"] = "দোকান", ["bazar"] = "বাজার",
        ["skul"] = "স্কুল", ["college"] = "কলেজ",
        ["office"] = "অফিস", ["bhasha"] = "ভাষা", ["vasha"] = "ভাষা", ["gan"] = "গান",
        ["boi"] = "বই", ["khela"] = "খেলা", ["kheli"] = "খেলি",
        ["pani"] = "পানি", ["jol"] = "জল",
        ["machh"] = "মাছ", ["mach"] = "মাছ", ["mangsho"] = "মাংস", ["bhat"] = "ভাত",
        ["ruti"] = "রুটি", ["dim"] = "ডিম", ["dudh"] = "দুধ", ["cha"] = "চা",
        ["biye"] = "বিয়ে", ["eid"] = "ঈদ", ["shubho"] = "শুভ", ["jonmodin"] = "জন্মদিন",
        ["jonmo"] = "জন্ম", ["shotti"] = "সত্যি",
        ["khubi"] = "খুবই", ["ok"] = "ওকে",
    };

    private static bool IsBanglaVowelLetter(char c) =>
        "অআইঈউঊএঐওঔ".IndexOf(c) >= 0;

    public static bool HasBanglishLetters(string text)
    {
        foreach (char c in text)
        {
            if (c >= 0x0980 && c <= 0x09FF) return false;
        }
        return text.Any(char.IsLetter);
    }

    private static bool LooksLikeEnglish(string word)
    {
        if (word.Any(char.IsDigit)) return true;
        if (SmartWordHandler.IsCommonEnglishWord(word)) return true;
        return false;
    }

    public static string TransliterateWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        string lower = word.ToLowerInvariant().TrimEnd('_');
        if (WordMap.TryGetValue(lower, out var mapped))
            return mapped;

        if (LooksLikeEnglish(word)) return word;

        var sb = new StringBuilder();
        int i = 0;
        bool lastWasConsonant = false;
        bool pendingHasanta = false;
        bool isFirstSegment = true;

        while (i < word.Length)
        {
            string bangla = "";
            int matchLen = 0;
            bool matchedCaseSensitive = false;

            foreach (var (roman, bn) in CaseSensitiveMap)
            {
                if (i + roman.Length <= word.Length &&
                    string.CompareOrdinal(word, i, roman, 0, roman.Length) == 0 &&
                    roman.Length > matchLen)
                {
                    bangla = bn;
                    matchLen = roman.Length;
                    matchedCaseSensitive = true;
                }
            }

            if (!matchedCaseSensitive)
            {
                foreach (var (roman, bn) in Map)
                {
                    if (i + roman.Length <= word.Length &&
                        string.Compare(word, i, roman, 0, roman.Length, StringComparison.OrdinalIgnoreCase) == 0 &&
                        roman.Length > matchLen)
                    {
                        bangla = bn;
                        matchLen = roman.Length;
                    }
                }
            }

            if (matchLen == 0)
            {
                sb.Append(word[i]);
                i++;
                lastWasConsonant = false;
                pendingHasanta = false;
                continue;
            }

            string matchedSource = word.Substring(i, matchLen);
            bool firstCharUpper = char.IsUpper(matchedSource[0]);

            bool isVowelSign = VowelSigns.Contains(bangla);
            bool isSingleVowel = matchLen == 1 && "aeiou".Contains(char.ToLowerInvariant(matchedSource[0]));

            if (pendingHasanta && !isVowelSign)
            {
                sb.Append(Hasanta);
            }
            pendingHasanta = false;

            if (isVowelSign || isSingleVowel)
            {
                bool atStart = !lastWasConsonant && isFirstSegment && sb.Length == 0;
                bool prevIsVowelFull = sb.Length > 0 && IsBanglaVowelLetter(sb[sb.Length - 1]);
                bool doubleVowel = matchLen >= 2 && matchedSource[0] == matchedSource[1];

                if (lastWasConsonant && !prevIsVowelFull)
                {
                    sb.Append(bangla);
                }
                else if (doubleVowel && !atStart)
                {
                    sb.Append(VowelFull.TryGetValue(matchedSource.ToLowerInvariant(), out var dv)
                        ? dv : bangla);
                }
                else if (sb.Length == 0)
                {
                    string key = matchedSource.ToLowerInvariant();
                    sb.Append(VowelFull.TryGetValue(key, out var full) ? full : bangla);
                }
                else if (prevIsVowelFull)
                {
                    sb.Append(bangla);
                }
                else
                {
                    string key = matchedSource.ToLowerInvariant();
                    sb.Append(VowelFull.TryGetValue(key, out var full2) ? full2 : bangla);
                }

                lastWasConsonant = false;
            }
            else
            {
                sb.Append(bangla);
                lastWasConsonant = true;

                if (matchedCaseSensitive)
                    pendingHasanta = true;
                else if (HardLetters.Contains(bangla) && word.Length > i + matchLen)
                {
                    string rest = word.Substring(i + matchLen);
                    bool nextIsVowelStart = rest.Length > 0 && "aeiou".Contains(char.ToLowerInvariant(rest[0]));
                    bool typedHard = firstCharUpper;
                    if (typedHard || bangla is "ড" or "ঢ" or "ড়")
                        pendingHasanta = true;
                }
            }

            isFirstSegment = false;
            i += matchLen;
        }

        return sb.ToString();
    }

    public static string Transliterate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var sb = new StringBuilder();
        var word = new StringBuilder();

        foreach (char c in text)
        {
            if (char.IsLetter(c))
            {
                word.Append(c);
            }
            else
            {
                if (word.Length > 0)
                {
                    sb.Append(TransliterateWord(word.ToString()));
                    word.Clear();
                }
                sb.Append(c);
            }
        }

        if (word.Length > 0)
            sb.Append(TransliterateWord(word.ToString()));

        return sb.ToString();
    }
}
