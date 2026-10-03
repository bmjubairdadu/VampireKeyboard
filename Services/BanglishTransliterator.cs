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
        ("s", "স"), ("h", "হ"), ("w", "ও"), ("x", "ক্স"), ("q", "ক"),
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

    private static readonly Dictionary<string, string> WordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ami"] = "আমি", ["amra"] = "আমরা", ["amar"] = "আমার", ["amake"] = "আমাকে",
        ["amader"] = "আমাদের", ["tumi"] = "তুমি", ["tomake"] = "তোমাকে", ["tomar"] = "তোমার",
        ["tomer"] = "তোমার", ["apni"] = "আপনি", ["apnar"] = "আপনার", ["apnake"] = "আপনাকে",
        ["tui"] = "তুই", ["tor"] = "তোর", ["tora"] = "তোরা", ["se"] = "সে",
        ["tahar"] = "তাহার", ["tar"] = "তার", ["tara"] = "তারা", ["take"] = "তাকে",
        ["tader"] = "তাদের", ["eder"] = "এদের", ["nijer"] = "নিজের", ["nijei"] = "নিজেই",
        ["ke"] = "কে", ["kake"] = "কাকে", ["ki"] = "কি", ["kinu"] = "কিনু",
        ["kothay"] = "কোথায়", ["keno"] = "কেন", ["kokhono"] = "কখনো", ["kokhoni"] = "কখনোই",
        ["kivabe"] = "কিভাবে", ["koto"] = "কত", ["kotota"] = "কতটা", ["kon"] = "কোন",
        ["je"] = "যে", ["jeta"] = "যেটা", ["jegulo"] = "যেগুলো", ["jar"] = "যার",
        ["jokhon"] = "যখন", ["tokhon"] = "তখন", ["jodi"] = "যদি", ["hole"] = "হলে",
        ["jehetu"] = "যেহেতু", ["kintu"] = "কিন্তু", ["tobe"] = "তবে", ["tai"] = "তাই",
        ["ar"] = "আর", ["o"] = "ও", ["ba"] = "বা", ["to"] = "তো",
        ["na"] = "না", ["ni"] = "নি", ["abar"] = "আবার", ["ekhono"] = "এখনো",
        ["ekhane"] = "এখানে", ["okhane"] = "ওখানে", ["ekhon"] = "এখন", ["ajke"] = "আজকে",
        ["kal"] = "কাল", ["kalk"] = "কালকে", ["age"] = "আগে", ["pore"] = "পরে",
        ["ekdom"] = "একদম", ["onek"] = "অনেক", ["khub"] = "খুব", ["besh"] = "বেশ",
        ["beshi"] = "বেশি", ["kom"] = "কম", ["hoito"] = "হয়তো", ["hoyoito"] = "হয়তো",
        ["bhalo"] = "ভালো", ["valo"] = "ভালো", ["kharap"] = "খারাপ", ["boro"] = "বড়",
        ["choto"] = "ছোট", ["notun"] = "নতুন", ["purano"] = "পুরনো", ["sundor"] = "সুন্দর",
        ["mishti"] = "মিষ্টি", ["shustho"] = "সুস্থ", ["kemon"] = "কেমন", ["emon"] = "এমন",
        ["omon"] = "ওমন", ["mon"] = "মন", ["kotha"] = "কথা", ["bishoy"] = "বিষয়",
        ["somoy"] = "সময়", ["shomoy"] = "সময়", ["jinish"] = "জিনিস", ["jinis"] = "জিনিস",
        ["bepar"] = "ব্যাপার", ["dorkar"] = "দরকার", ["proyojon"] = "প্রয়োজন", ["iccha"] = "ইচ্ছা",
        ["ichha"] = "ইচ্ছা", ["kaj"] = "কাজ", ["kam"] = "কাম", ["dhonnobad"] = "ধন্যবাদ",
        ["dhonnobash"] = "ধন্যবাদ", ["thanks"] = "ধন্যবাদ", ["thank"] = "ধন্যবাদ", ["hello"] = "হ্যালো",
        ["hi"] = "হাই", ["sorry"] = "সরি", ["salaam"] = "সালাম", ["salam"] = "সালাম",
        ["nomoshkar"] = "নমস্কার", ["accha"] = "আচ্ছা", ["acha"] = "আচ্ছা", ["acchi"] = "আছি",
        ["hmm"] = "হুম", ["thik"] = "ঠিক", ["thikache"] = "ঠিকাছে", ["ha"] = "হ্যাঁ",
        ["hae"] = "হ্যাঁ", ["aunno"] = "হ্যাঁ", ["unno"] = "উন",
        ["ache"] = "আছে", ["acho"] = "আছো", ["achen"] = "আছেন", ["achhen"] = "আছেন",
        ["nei"] = "নেই", ["nai"] = "নাই", ["chilo"] = "ছিল", ["chil"] = "ছিল",
        ["chilam"] = "ছিলাম", ["chilo_na"] = "ছিলনা", ["hobe"] = "হবে", ["hobe_na"] = "হবেনা",
        ["hoy"] = "হয়", ["hoye"] = "হয়ে", ["hoyni"] = "হয়নি", ["hocche"] = "হচ্ছে",
        ["hoyonni"] = "হয়নি", ["korbo"] = "করবো", ["kore"] = "করে", ["kori"] = "করি",
        ["koro"] = "করো", ["korchen"] = "করছেন", ["korchi"] = "করছি", ["korechi"] = "করেছি",
        ["korechilo"] = "করেছিল", ["korte"] = "করতে", ["korle"] = "করলে", ["korbe"] = "করবে",
        ["korben"] = "করবেন", ["korun"] = "করুন", ["korlam"] = "করলাম", ["korini"] = "করিনি",
        ["koren"] = "করেন", ["korar"] = "করার", ["jabo"] = "যাবো", ["jachhi"] = "যাচ্ছি",
        ["jachche"] = "যাচ্ছে", ["jete"] = "যেতে", ["jabe"] = "যাবে", ["jaba"] = "যাবা",
        ["jawa"] = "যাওয়া", ["gese"] = "গেছে", ["geche"] = "গেছে", ["gele"] = "গেলে",
        ["giye"] = "গিয়ে", ["giyechi"] = "গিয়েছি", ["gelo"] = "গেলো", ["jai"] = "যাই",
        ["jao"] = "যাও", ["ashbo"] = "আসবো", ["ashchi"] = "আসছি", ["ashbe"] = "আসবে",
        ["ashe"] = "আসে", ["ashen"] = "আসেন", ["asha"] = "আশা", ["ashte"] = "আসতে",
        ["ashle"] = "আসলে", ["ashole"] = "আসলে", ["dekha"] = "দেখা", ["dekhi"] = "দেখি",
        ["dekho"] = "দেখো", ["dekhe"] = "দেখে", ["dekhte"] = "দেখতে", ["dekhbo"] = "দেখবো",
        ["dekhechi"] = "দেখেছি", ["dekhlam"] = "দেখলাম", ["dekhsen"] = "দেখসেন", ["dekhchen"] = "দেখছেন",
        ["dekhtesi"] = "দেখতেছি", ["bola"] = "বলা", ["bolo"] = "বলো", ["bolchi"] = "বলছি",
        ["bolen"] = "বলেন", ["bole"] = "বলে", ["bolar"] = "বলার", ["bolbo"] = "বলবো",
        ["bolechi"] = "বলেছি", ["bolle"] = "বললে", ["shona"] = "শোনা", ["shuno"] = "শুনো",
        ["shuni"] = "শুনি", ["shune"] = "শুনে", ["shon"] = "শোন", ["shonbo"] = "শুনবো",
        ["khawa"] = "খাওয়া", ["khai"] = "খাই", ["khao"] = "খাও", ["khabo"] = "খাবো",
        ["kheye"] = "খেয়ে", ["khabar"] = "খাবার", ["khelam"] = "খেলাম", ["khele"] = "খেলে",
        ["khawa"] = "খাওয়া", ["chawa"] = "চাওয়া", ["chai"] = "চাই", ["chao"] = "চাও",
        ["chay"] = "চায়", ["chaiche"] = "চাইছে", ["dicha"] = "দিচ্ছা", ["dicche"] = "দিচ্ছে",
        ["dicchhi"] = "দিচ্ছি", ["dibi"] = "দিবি", ["dibo"] = "দিবো", ["dibe"] = "দিবে",
        ["dao"] = "দাও", ["dio"] = "দাও", ["dawa"] = "দেওয়া", ["diye"] = "দিয়ে",
        ["din"] = "দিন", ["dilo"] = "দিলো", ["dilam"] = "দিলাম", ["nite"] = "নিতে",
        ["nao"] = "নাও", ["niye"] = "নিয়ে", ["nibo"] = "নিবো", ["nibe"] = "নিবে",
        ["nichche"] = "নিচ্ছে", ["nichhi"] = "নিচ্ছি", ["neoa"] = "নেওয়া", ["likha"] = "লেখা",
        ["likho"] = "লিখো", ["likhi"] = "লিখি", ["likhle"] = "লিখলে", ["likhlam"] = "লিখলাম",
        ["lekha"] = "লেখা", ["pora"] = "পড়া", ["pori"] = "পড়ি", ["porbo"] = "পড়বো",
        ["porlam"] = "পড়লাম", ["poro"] = "পড়ো", ["porte"] = "পড়তে", ["porchi"] = "পড়ছি",
        ["cholche"] = "চলছে", ["cholbe"] = "চলবে", ["cholchilo"] = "চলছিল", ["cholo"] = "চলো",
        ["chol"] = "চল", ["kichu"] = "কিছু", ["kichu"] = "কিছু", ["kichui"] = "কিছুই",
        ["bujhlam"] = "বুঝলাম", ["buji"] = "বুঝি", ["bujho"] = "বুঝো", ["bujhe"] = "বুঝে",
        ["bujhechi"] = "বুঝেছি", ["bujhen"] = "বুঝেন", ["bujhsen"] = "বুঝসেন", ["mone"] = "মনে",
        ["mone_hoy"] = "মনেহয়", ["monehochhe"] = "মনেহচ্ছে", ["dhoro"] = "ধরো", ["dhori"] = "ধরি",
        ["dhore"] = "ধরে", ["dhorbo"] = "ধরবো", ["dhorlam"] = "ধরলাম", ["rakho"] = "রাখো",
        ["rakhi"] = "রাখি", ["rekhechi"] = "রেখেছি", ["rakhbo"] = "রাখবো", ["rakhle"] = "রাখলে",
        ["bhai"] = "ভাই", ["vai"] = "ভাই", ["bon"] = "বোন", ["apu"] = "আপু",
        ["didi"] = "দিদি", ["dada"] = "দাদা", ["mama"] = "মামা", ["mami"] = "মামি",
        ["mashi"] = "মাসি", ["chacha"] = "চাচা", ["chachi"] = "চাচি", ["kaku"] = "কাকু",
        ["kakima"] = "কাকিমা", ["nana"] = "নানা", ["nani"] = "নানি", ["dadi"] = "দাদি",
        ["thakuma"] = "ঠাকুমা", ["ammu"] = "আম্মু", ["abbu"] = "আব্বু", ["ma"] = "মা",
        ["baba"] = "বাবা", ["bou"] = "বউ", ["jamai"] = "জামাই", ["meye"] = "মেয়ে",
        ["chele"] = "ছেলে", ["manush"] = "মানুষ", ["lok"] = "লোক", [" Bondhu"] = "বন্ধু",
        ["bondhu"] = "বন্ধু", ["vabi"] = "ভাবি", ["bhabi"] = "ভাবি", ["vabhi"] = "ভাবি",
        ["bangladesh"] = "বাংলাদেশ", ["bangla"] = "বাংলা", ["dhaka"] = "ঢাকা", ["desh"] = "দেশ",
        ["desher"] = "দেশের", ["bidesh"] = "বিদেশ", ["ghor"] = "ঘর", ["ghore"] = "ঘরে",
        ["bari"] = "বাড়ি", ["barite"] = "বাড়িতে", ["dokan"] = "দোকান", ["bazar"] = "বাজার",
        ["school"] = "স্কুল", ["skul"] = "স্কুল", ["college"] = "কলেজ", ["university"] = "ইউনিভার্সিটি",
        ["office"] = "অফিস", ["bhasha"] = "ভাষা", ["vasha"] = "ভাষা", ["gan"] = "গান",
        ["boi"] = "বই", ["khela"] = "খেলা", ["kheli"] = "খেলি", ["khelechi"] = "খেলেছি",
        ["football"] = "ফুটবল", ["cricket"] = "ক্রিকেট", ["pani"] = "পানি", ["jol"] = "জল",
        ["machh"] = "মাছ", ["mach"] = "মাছ", ["mangsho"] = "মাংস", ["bhat"] = "ভাত",
        ["ruti"] = "রুটি", ["dim"] = "ডিম", ["dudh"] = "দুধ", ["cha"] = "চা",
        ["chaa"] = "চা", ["coffee"] = "কফি", ["biye"] = "বিয়ে", ["bijoy"] = "বিজয়",
        ["eid"] = "ঈদ", ["puja"] = "পূজা", ["shubho"] = "শুভ", ["jonmodin"] = "জন্মদিন",
        ["jonmo"] = "জন্ম", ["mithye"] = "মিথ্যে", ["mithya"] = "মিথ্যা", ["shotti"] = "সত্যি",
        ["shotti"] = "সত্যি", ["baba_re"] = "বাবারে", ["khub_i"] = "খুবই", ["khubi"] = "খুবই",
        ["ok"] = "ওকে", ["hmm_hmm"] = "হুমহুম",
    };

    private static readonly string[] EnglishWords =
    {
        "the", "and", "for", "with", "this", "that", "from", "have", "not",
        "are", "was", "you", "your", "will", "can", "what", "when", "where",
        "who", "how", "why", "yes", "no", "ok", "okay", "please", "me",
        "my", "we", "they", "he", "she", "it", "is", "in", "on", "at", "to",
        "do", "does", "did", "done", "get", "got", "go", "going", "come",
        "came", "want", "need", "like", "love", "know", "think", "see",
        "look", "make", "made", "take", "give", "find", "use", "work",
        "http", "https", "www", "com", "org", "net", "gmail", "yahoo",
        "facebook", "google", "youtube", "windows", "linux", "android",
        "whatsapp", "messenger", "imo", "zoom", "email", "password", "login",
        "account", "file", "folder", "download", "upload", "install", "update",
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
        string w = word.ToLowerInvariant();
        foreach (var e in EnglishWords)
        {
            if (w == e) return true;
        }
        if (w.StartsWith("http") || w.StartsWith("www.") || word.Contains('@') || word.Contains('.')) return true;
        return false;
    }

    public static string TransliterateWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        if (WordMap.TryGetValue(word.TrimEnd('_').ToLowerInvariant(), out var mapped))
            return mapped;

        if (LooksLikeEnglish(word)) return word;

        var sb = new StringBuilder();
        int i = 0;
        bool lastWasConsonant = false;
        bool pendingHasanta = false;

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

            if (pendingHasanta)
            {
                if (VowelSigns.Contains(bangla))
                {
                    pendingHasanta = false;
                }
                else
                {
                    sb.Append(Hasanta);
                    pendingHasanta = false;
                }
            }

            bool isVowelSign = VowelSigns.Contains(bangla);
            bool isSingleVowel = matchLen == 1 && "aeiou".Contains(char.ToLowerInvariant(matchedSource[0]));
            bool forceFullVowel = (isSingleVowel && firstCharUpper && !matchedCaseSensitive);

            if (isVowelSign || isSingleVowel)
            {
                if (lastWasConsonant && !forceFullVowel)
                {
                    sb.Append(bangla);
                }
                else
                {
                    string key = matchedSource.ToLowerInvariant();
                    if (VowelFull.TryGetValue(key, out var full))
                        sb.Append(full);
                    else
                        sb.Append(bangla switch
                        {
                            "া" => "আ",
                            "ি" => "ই",
                            "ু" => "উ",
                            "ে" => "এ",
                            "ো" => "ও",
                            "ৈ" => "ঐ",
                            "ৌ" => "ঔ",
                            "ৃ" => "ঋ",
                            _ => bangla,
                        });
                }

                lastWasConsonant = false;
            }
            else
            {
                sb.Append(bangla);
                lastWasConsonant = true;
                if (matchedCaseSensitive || (firstCharUpper && char.ToLowerInvariant(matchedSource[0]) is >= 'a' and <= 'z' && !"aeiou".Contains(char.ToLowerInvariant(matchedSource[0]))))
                    pendingHasanta = true;
            }

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
