using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VampireKeyboard.Services;

/// <summary>
/// Banglish (Roman Bengali) -> Bangla (Unicode) transliteration engine.
///
/// Pipeline for a single word:
///   1. Curated dictionary lookup  -> always wins (handles irregular words).
///   2. If the word is real English / technical / contains digits -> leave as-is.
///   3. Greedy longest-match syllable parser using real Bengali rules:
///        * consonant + vowel     -> ক + া   (matra hangs off the consonant)
///        * consonant + consonant -> ক + ্ + খ (hasanta / virama)
///        * bare consonant at end -> ক      (inherent vowel is never written)
///        * medial "o"            -> dropped, it is the inherent vowel of the
///                                   first syllable            (korte -> করতে)
///        * final  "o"            -> matra ো                  (koro  -> করো)
///        * final  "se"           -> silent, only ে survives  (lagse -> লাগে)
///        * uppercase T/D/N/R/S/H/Sh -> retroflex ট ড ণ ড় শ ষ ঢ়
///        * ligature clusters ksh/kkh -> ক্ষ, chh -> ছ, nd -> ন্ধ, ...
/// </summary>
public static class BanglishTransliterator
{
    private const char Hasanta = '্';

    // ------------------------------------------------------------- consonants
    // Matched case-INSENSITIVELY -> ordinary (dental) consonants.
    private static readonly (string Roman, string Bangla)[] Consonants =
    {
        ("k", "ক"), ("kh", "খ"), ("g", "গ"), ("gh", "ঘ"), ("ng", "ং"),
        ("c", "চ"), ("ch", "চ"), ("j", "জ"), ("jh", "ঝ"), ("ny", "ঞ"),
        ("t", "ত"), ("th", "থ"), ("d", "দ"), ("dh", "ধ"), ("n", "ন"),
        ("p", "প"), ("ph", "ফ"), ("b", "ব"), ("bh", "ভ"), ("m", "ম"),
        ("y", "য়"), ("r", "র"), ("l", "ল"), ("w", "ব"), ("v", "ভ"),
        ("sh", "শ"), ("s", "স"), ("h", "হ"), ("z", "জ"), ("f", "ফ"),
        ("x", "ক্স"), ("q", "ক"),
    };

    // Matched EXACTLY (case sensitive) -> retroflex consonants (Banglish convention).
    private static readonly (string Roman, string Bangla)[] RetroConsonants =
    {
        ("T", "ট"), ("Th", "ঠ"), ("D", "ড"), ("Dh", "ঢ"), ("N", "ণ"),
        ("S", "শ"), ("Sh", "ষ"), ("H", "হ"), ("R", "ড়"), ("Rh", "ঢ়"),
        ("Y", "য়"), ("Z", "ঝ"),
    };

    // Ligature clusters, checked BEFORE single consonants (longest match wins anyway).
    private static readonly (string Roman, string Bangla)[] Clusters =
    {
        ("kkh", "ক্ষ"), ("ksh", "ক্ষ"), ("gy", "জ্ঞ"), ("gny", "জ্ঞ"),
        ("ngg", "ঙ্গ"), ("nch", "ঞ্চ"), ("chk", "চ্ক"), ("chh", "ছ"),
        ("shh", "শ"), ("ssh", "ষ"), ("tth", "ঠ"), ("thh", "ঠ"),
        ("ddh", "দ্ধ"), ("dd", "দ্দ"), ("ndh", "ন্ধ"), ("nd", "ন্ধ"),
        ("nt", "ন্ট"), ("mp", "ম্প"), ("mb", "ম্ব"), ("sch", "শ্চ"),
    };

    // ----------------------------------------------------------------- vowels
    // Attached to a preceding consonant (matra / dependent vowel sign).
    private static readonly (string Roman, string Bangla)[] VowelSigns =
    {
        ("aa", "া"), ("ai", "া"), ("ee", "ী"), ("ii", "ী"), ("uu", "ূ"),
        ("oo", "ূ"), ("oi", "ে"), ("ou", "ৌ"),
        ("a", "া"), ("i", "ি"), ("u", "ু"), ("e", "ে"), ("o", "ো"),
        ("ri", "ৃ"), ("ri", "ৃ"),
    };

    // Stand-alone vowel letter (independent vowel).
    private static readonly (string Roman, string Bangla)[] VowelFulls =
    {
        ("aa", "আ"), ("ai", "আ"), ("ee", "ঈ"), ("ii", "ঈ"), ("uu", "ঊ"),
        ("oo", "ঊ"), ("oi", "ঐ"), ("ou", "ঔ"),
        ("a", "আ"), ("i", "ই"), ("u", "উ"), ("e", "এ"), ("o", "ও"),
        ("ri", "ঋ"), ("ri", "ঋ"),
    };

    // NOTE: RawWords is declared at the bottom of this file. Static field
    // initialisers run in declaration order, so WordMap must be built lazily
    // to guarantee RawWords is already populated.
    private static Dictionary<string, string>? _wordMap;
    private static Dictionary<string, string> WordMap =>
        _wordMap ??= BuildWordMap();

    public static bool HasBanglishLetters(string text)
    {
        foreach (char c in text)
            if (c >= 0x0980 && c <= 0x09FF) return false;
        return text.Any(char.IsLetter);
    }

    private static bool LooksLikeEnglish(string word)
    {
        if (word.Any(char.IsDigit)) return true;
        if (SmartWordHandler.LooksLikeTechnicalTerm(word)) return true;
        if (SmartWordHandler.IsCommonEnglishWord(word)) return true;
        return false;
    }

    // ------------------------------------------------------------------ core
    public static string TransliterateWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        string key = word.ToLowerInvariant();
        if (WordMap.TryGetValue(key, out var mapped)) return mapped;

        if (LooksLikeEnglish(word)) return word;

        string result = Parse(word);

        // Reject output that has no Bangla at all -> keep original text.
        foreach (char c in result)
            if (c >= 0x0980 && c <= 0x09FF) return result;

        return word;
    }

    private static string Parse(string word)
    {
        var sb = new StringBuilder();
        int i = 0;

        while (i < word.Length)
        {
            // 0) silent final particle "se" -> only the ে survives (lagse -> লাগে).
            //    Guarded so short words such as "ase" (আসে) keep their real স.
            if (i == word.Length - 2 && IsSilentFinalSe(word, i))
            {
                sb.Append('ে');
                i += 2;
                continue;
            }

            // 1) ligature cluster?
            int clen = MatchCluster(word, i, out string cluster);
            if (clen > 0)
            {
                sb.Append(cluster);
                i += clen;
                AppendPendingHasantaIfNeeded(sb, word, i);
                continue;
            }

            // 2) consonant? (retroflex first, then ordinary - take the longer match)
            int rlen = MatchRetro(word, i, out string retro);
            int olen = MatchConsonant(word, i, out string ordinary);
            string? consonant = null;
            int len = 0;
            if (rlen > 0 && rlen >= olen) { consonant = retro; len = rlen; }
            else if (olen > 0) { consonant = ordinary; len = olen; }

            if (consonant != null)
            {
                i += len;

                // "ng" before g/h forms ঙ্গ (sung -> সুঙ্গ)
                if (consonant == "ং" && i < word.Length && (word[i] == 'g' || word[i] == 'G'))
                {
                    sb.Append("ঙ");
                    continue;
                }

                // A medial "o" (followed by another consonant) is NOT a matra -
                // it is the inherent vowel of this open syllable (korte -> করতে).
                if (IsMedialO(word, i))
                {
                    sb.Append(consonant);
                    i++;
                    continue;
                }

                int vlen = MatchVowel(word, i, out string matra);
                if (vlen > 0)
                {
                    sb.Append(consonant);
                    sb.Append(matra);
                    i += vlen;
                    continue;
                }

                sb.Append(consonant);
                AppendPendingHasantaIfNeeded(sb, word, i);
                continue;
            }

            // 3) stand-alone vowel letter
            int flen = MatchVowelFull(word, i, out string full);
            if (flen > 0)
            {
                sb.Append(full);
                i += flen;
                continue;
            }

            // 4) unknown character -> copy verbatim
            sb.Append(word[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>Verb roots whose word-final "se" drops the স entirely: lagse, korse.</summary>
    private static readonly string[] SilentSeRoots =
    {
        "lag", "kor", "jas", "khal", "bosh", "dhuk", "thak", "dhar",
        "pok", "chok", "nol", "huk", "shun", "dekh", "jog", "rot",
        "pon", "thuk", "rok", "bas", "chhuk", "mukh", "hath", "pith"
    };

    /// <summary>Word-final "se" whose 's' is a silent particle: lagse, korse, jasse.</summary>
    private static bool IsSilentFinalSe(string word, int i)
    {
        if (i < 1 || i + 2 != word.Length) return false;
        if (word[i] != 's' && word[i] != 'S') return false;
        if (word[i + 1] != 'e' && word[i + 1] != 'E') return false;

        // A whitelist of verb roots keeps this from eating real স (pase, tase, house).
        string root = word.Substring(0, i).ToLowerInvariant();
        foreach (string r in SilentSeRoots)
            if (root == r) return true;

        return false;
    }

    /// <summary>An "o" followed by another consonant rather than ending the word.</summary>
    private static bool IsMedialO(string word, int i)
    {
        if (i <= 0 || i + 1 >= word.Length) return false;   // final "o" -> matra
        if (word[i] != 'o' && word[i] != 'O') return false;
        char next = word[i + 1];
        if (!char.IsLetter(next)) return false;
        return !IsVowelStart(word, i + 1);                  // next is a consonant
    }

    /// <summary>Adds a virama when the next character starts another consonant.</summary>
    private static void AppendPendingHasantaIfNeeded(StringBuilder sb, string word, int next)
    {
        if (next >= word.Length) return;
        if (!char.IsLetter(word[next])) return;

        // A vowel right after never takes a hasanta.
        if (IsVowelStart(word, next)) return;

        // A medial "o" also never takes one - that syllable keeps its inherent vowel.
        if (IsMedialO(word, next)) return;

        // A silent final "se" contributes nothing but its ে, so the স never
        // materialises and must not be preceded by a virama (lagse -> লাগে).
        if (next + 2 == word.Length && IsSilentFinalSe(word, next)) return;

        // ং / ঃ never carry a hasanta.
        char last = sb.Length > 0 ? sb[sb.Length - 1] : '\0';
        if (last == 'ং' || last == 'ঃ') return;

        sb.Append(Hasanta);
    }

    private static bool IsVowelStart(string word, int i)
    {
        if (i < 0 || i >= word.Length) return false;
        char c = word[i];
        if (!char.IsLetter(c)) return false;
        foreach (var (roman, _) in VowelFulls)
            if (roman.Length == 1 && char.ToLowerInvariant(c) == roman[0])
                return true;
        return false;
    }

    // -------------------------------------------------------------- matching
    private static int MatchCluster(string w, int i, out string value)
        => MatchTable(Clusters, w, i, caseSensitive: false, out value);

    private static int MatchRetro(string w, int i, out string value)
        => MatchTable(RetroConsonants, w, i, caseSensitive: true, out value);

    private static int MatchConsonant(string w, int i, out string value)
        => MatchTable(Consonants, w, i, caseSensitive: false, out value);

    private static int MatchVowel(string w, int i, out string value)
        => MatchTable(VowelSigns, w, i, caseSensitive: false, out value);

    private static int MatchVowelFull(string w, int i, out string value)
        => MatchTable(VowelFulls, w, i, caseSensitive: false, out value);

    private static int MatchTable((string Roman, string Bangla)[] table,
                                   string w, int i, bool caseSensitive, out string value)
    {
        int best = 0;
        value = string.Empty;
        foreach (var (r, b) in table)
        {
            if (r.Length <= best || i + r.Length > w.Length) continue;
            if (Match(w, i, r, caseSensitive)) { best = r.Length; value = b; }
        }
        return best;
    }

    private static bool Match(string s, int i, string token, bool caseSensitive)
    {
        for (int k = 0; k < token.Length; k++)
        {
            char a = s[i + k], b = token[k];
            if (caseSensitive ? a != b : char.ToLowerInvariant(a) != char.ToLowerInvariant(b))
                return false;
        }
        return true;
    }

    // -------------------------------------------------------- dictionary
    private static Dictionary<string, string> BuildWordMap()
    {
        var m = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string line in RawWords)
        {
            int eq = line.IndexOf('=');
            if (eq <= 0) continue;
            string key = line[..eq].Trim();
            // The hook converts one word at a time, so a key containing a space
            // can never match and is only a source of confusion.
            if (key.Contains(' ')) continue;
            m[key] = line[(eq + 1)..].Trim();
        }
        return m;
    }

    private static readonly string[] RawWords = BuildRawWords();

    private static string[] BuildRawWords() => new[]
    {
        // ---- pronouns / honorifics
        "ami=আমি","amra=আমরা","amar=আমার","amake=আমাকে","amader=আমাদের",
        "tumi=তুমি","tomake=তোমাকে","tomar=তোমার","tomer=তোমার","toke=তোকে",
        "tora=তোরা","tor=তোর",
        "apni=আপনি","apnar=আপনার","apnake=আপনাকে","apnader=আপনাদের","apnara=আপনারা",
        "tui=তুই","tuke=তুকে","se=সে","tahay=তাহায়","tahare=তাহারে","taharker=তাহার",
        "tar=তার","tara=তোরা","take=তাকে","tader=তাদের","eder=এদের",
        "nijer=নিজের","nijei=নিজেই","ke=কে","kake=কাকে","ki=কি","kichi=কিছু","kichu=কিছু",
        "kotojon=কতজন","karo=কারো","kichue=কিছুই","shudhu=শুধু","sirf=শুধু",

        // ---- questions / conjunctions
        "kemon=কেমন","keno=কেন","kokhono=কখনো","kokhoni=কখনোই","kivabe=কিভাবে","koto=কত",
        "kotota=কতটা","kon=কোন","konthay=কোথায়","kothay=কোথায়","koyek=কয়েক","koyeb=কয়েক",
        "shob=সব","shokhon=সকল","jeta=যেটা","jetar=যেতার","jegulo=যেগুলো","jar=যার",
        "jekhane=যেখানে","jekhon=যখন","jokhon=যখন","jodi=যদি","je=যে","jei=যে",
        "jeno=যেনো","jate=যাতে","jehetu=যেহেতু",
        "tokhon=তখন","tarpor=তারপর","kintu=কিন্তু","tobe=তবে","tai=তাই",
        "na=না","ni=নি","to=তো","ba=বা","ar=আর","o=ও","ebong=এবং",
        "hate=হতে","till=পর্যন্ত","dhore=ধরে","mule=মূলে","madhye=মধ্যে","vortof=ভরতে",

        // ---- numbers
        "ek=এক","dui=দুই","tin=তিন","char=চার","pach=পাঁচ","choy=ছয়","sat=সাত",
        "aat=আট","noy=নয়","dosh=দশ","baro=বারো","hajar=হাজার","lakh=লক্ষ","koti=কোটি",

        // ---- time
        "ekhon=এখন","ekhono=এখনো","ekhane=এখানে","okhane=ওখানে","ajke=আজকে","kal=কাল",
        "age=আগে","pore=পরে","shomoy=সময়","somoy=সময়","bishoy=বিষয়","diner=দিনের","din=দিন",
        "raat=রাত","shokal=সকাল","bela=বেলা","shaam=শাম","dupur=দুপুর","ratri=রাত্রি",
        "hobo=হবে","sona=সোনা","mash=মাস","mahina=মেহিনা","bochor=বছর","bachhor=বছর",
        "soptahiko=সপ্তাহিক","weekend=উইকেন্ড",

        // ---- adjectives / adverbs
        "bhalo=ভালো","valo=ভালো","kharap=খারাপ","boro=বড়","choto=ছোট","notun=নতুন",
        "purano=পুরনো","sundor=সুন্দর","mishti=মিষ্টি","emon=এমন","ekdom=একদম","ekdomi=একদমমি",
        "onek=অনেক","khub=খুব","khubi=খুবই","beshi=বেশি","kom=কম","hoito=হয়তো","thik=ঠিক",
        "proyojon=প্রয়োজন","iccha=ইচ্ছা","dorkar=দরকার","shonkha=শঙ্খা","shato=শত",
        "shobar=শতবার","opomo=অপেক্ষা","niyom=নিয়ম","bandhan=বন্ধন","rokom=রকম",
        "shathe=সাথে","bhabe=ভাবে",

        // ---- place names (retroflex must not be guessed from capital letters)
        "dhaka=ঢাকা","dhakar=ঢাকার","dhake=ঢাকায়","dhakay=ঢাকায়","dhakate=ঢাকাতে",
        "dhakabashi=ঢাকাবাসী","kolkata=কলকাতা","khulna=খুলনা","chattogram=চট্টগ্রাম",
        "sylhet=সিলেট","rajshahi=রাজশাহী","birbom=বর্ধম",

        // ---- verbs: kor- family
        "kora=করা","kori=করি","koro=করো","korbo=করবো","korben=করবেন","korchi=করছি",
        "kore=করে","korun=করুন","korechi=করেছি","korecho=করেছো","korchen=করছেন",
        "korlam=করলাম","korle=করলে","korini=করিনি","kortesen=করতেন","korta=করতা",
        "koreche=করেছে","korechilen=করেছিলেন","kore6=করেছে",
        "korte=করতে","kortecho=করতেছো","kortechi=করতেছি","korao=করাও",
        "korar=করার","korer=করার","korleki=করলেকি",

        // ---- verbs: ja- family
        "jaoa=যাওয়া","jao=যাও","jabo=যাবো","jaben=যাবেন","jachhi=যাচ্ছি","jachche=যাচ্ছে",
        "jachchen=যাচ্ছেন","jachhilo=যাচ্ছিল","jete=যেতে","jabe=যাবে","jassi=যাসি",
        "gelam=গেলাম","gele=গেলে","gelo=গেলো","geche=গেছে","gese=গেছে",
        "geyechi=গিয়েছি","giye=গিয়ে","giyechi=গিয়েছি","giyechilen=গিয়েছিলেন",

        // ---- verbs: as- family
        "asa=আসা","asbo=আসবো","ashbo=আসবো","aschi=আসছি","ashchi=আসছি","ashche=আসছে",
        "ashe=আসে","ashen=আসেন","asha=আশা","ashlei=আসলেই","ashole=আসলে","ashte=আসতে",
        "ashtechilen=আসতেছিলেন","aselam=আসলাম",

        // ---- verbs: dekh- family
        "dekha=দেখা","dekhi=দেখি","dekho=দেখো","dekhe=দেখে","dekhte=দেখতে","dekhbo=দেখবো",
        "dekhben=দেখবেন","dekhchi=দেখছি","dekhechi=দেখেছি","dekhlam=দেখলাম","dekhlen=দেখলেন",
        "dekhle=দেখলে","dekhay=দেখায়","dekhte6=দেখতে","dekhe6=দেখেছে",

        // ---- verbs: shon- family
        "shona=শোনা","shuno=শুনো","shuni=শুনি","shune=শুনে","shunle=শুনলে","shunlam=শুনলাম",
        "shunbo=শুনবো","shunchi=শুনছি","shunche=শুনছে","shon=শোন",

        // ---- verbs: kha- family
        "khai=খাই","khao=খাও","khabo=খাবো","khaben=খাবেন","kheye=খেয়ে","khei=খেয়ে",
        "khaye=খেয়ে","khabe=খাবে","khachhi=খাচ্ছি","khe6=খেয়েছে","kheue6=খেয়েছে",
        "khabar=খাবার","bhojan=ভোজন","khelam=খেলাম","khele=খেলে","khelbo=খেলবো",
        "khelchi=খেলছি","khele6=খেলেছে","khela=খেলা","kheleo=খেলে",

        // ---- verbs: bola- family
        "bola=বলা","bolo=বলো","boli=বলি","bole=বলে","bolbo=বলবো","bolben=বলবেন",
        "bolchi=বলছি","bolen=বলেন","bole6=বলেছে","bollam=বললাম","bolte=বলতে",
        "bolle=বললে","boler=বলার",

        // ---- verbs: da- family (dena)
        "dena=দেওয়া","dao=দাও","dai=দাও","dile=দিলে","dilam=দিলাম","dilen=দিলেন",
        "dibo=দিবো","diben=দিবেন","diche=দিচ্ছে","dicchhi=দিচ্ছি","dite=দিতে",
        "dewa=দেওয়া","deyechi=দিয়েছি","dewar=দেওয়ার","deya=দেয়া",

        // ---- verbs: ni- family
        "nibo=নিবো","niben=নিবেন","nilam=নিলাম","niyechi=নিয়েছি","niye=নিয়ে","nile=নিলে",
        "niyo=নিয়ো","nite=নিতে","nichi=নিচ্ছি",

        // ---- verbs: thak- family
        "thako=থাকা","thake=থাকে","thaklo=থাকলো","thakchi=থাকছি","thaken=থাকেন",
        "theke=থেকে","theko=থেকে","thakte=থাকতে","thakiye=থাকিয়ে","thekay=থেকে",

        // ---- verbs: bukh- family
        "bujha=বুঝা","bujhi=বুঝি","bujho=বুঝো","bujhe=বুঝে","bujhte=বুঝতে","bujhe6=বুঝেছে",
        "bujhle=বুঝলে","bujhlam=বুঝলাম","bujhbo=বুঝবো","bujhni=বুঝনি","bujhchte=বুঝতে",

        // ---- verbs: likh- family
        "lekha=লেখা","likha=লেখা","likhi=লিখি","likho=লেখো","likhe=লিখে","likhbo=লিখবো",
        "likhchi=লিখছি","likhe6=লিখেছে","likhlam=লিখলাম","likhte=লিখতে",
        "pothay=পাঠায়","pora=পড়া","pori=পড়ি","poro=পড়ো","pore=পড়ে","porbo=পড়বো",
        "porchi=পড়ছি","porlam=পড়লাম","porte=পড়তে","podte=পড়তে",

        // ---- verbs: dhor- / rakh- family
        "dhora=ধরা","dhori=ধরি","dhoro=ধরো","dhore=ধরে","dhorbo=ধরবো","dhorle=ধরলে",
        "rakha=রাখা","rakhi=রাখি","rakho=রাখো","rakhe=রাখে","rakhiye=রাখিয়ে",

        // ---- verbs: chol- family
        "chola=চলা","cholo=চলো","choli=চলি","chole=চলে","cholbo=চলবো","cholchi=চলছি",
        "cholche=চলছে","cholbe=চলবে","cholle=চললে",

        // ---- verbs: para- family
        "para=পারা","pari=পারি","paro=পারো","pare=পারে","parbo=পারবো","parbe=পারবে",
        "parbi=পারবি","parben=পারবেন","parlam=পারলাম","parle=পারলে","parcha=পারছে",
        "parini=পারিনি",

        // ---- irregular common words
        "chai=চাই","chao=চাও","chailo=চাইলো","chaile=চাইলে","chaina=চাইনা","chaite=চাইতে",
        "nachai=নাচাই","lagbe=লাগবে","laglo=লাগলো","lagi=লাগি","lage=লাগে","lager=লাগার",
        "peya=পেয়া","peye=পেয়ে","peyer=পেয়ার","peyechi=পেয়েছি",
        "nol=নল","rong=রং","murong=মুরগি",
        "shanto=শান্ত","band=বন্ধ","bandho=বন্ধ","bandh=বন্ধ",
        "buddho=বুদ্ধ","vuddho=বুদ্ধ","griho=গৃহ","grihokar=গৃহকার",
        "grihokarer=গৃহকারের","bondho=বন্ধ","mrittu=মৃত্তু","mrityu=মৃত্যু",

        // ---- body / family / social
        "bhai=ভাই","vai=ভাই","bon=বোন","apu=আপু","didi=দিদি","didima=দিদিমা",
        "dada=দাদা","dadi=দাদি","mama=মামা","mami=মামি","mashi=মাসি","chacha=চাচা",
        "chachi=চাচি","kaku=কাকু","kakima=কাকিমা","nana=নানা","nani=নানি",
        "thakuma=ঠাকুমা","ammu=আম্মু","abbu=আব্বু","ma=মা","baba=বাবা","bou=বউ",
        "jamai=জামাই","meye=মেয়ে","chele=ছেলে","cheler=ছেলের","meyeder=মেয়েদের",
        "manush=মানুষ","lok=লোক","bondhu=বন্ধু","pader=পাড়ের",

        // ---- body parts
        "matha=মাথা","chokh=চোখ","noshto=নাক","kotha=কথা","kan=কান","deta=দাঁত",
        "gola=গলা","haat=হাত","hat=হাত","pa=পা","pair=পায়ের","pukh=পেট","kothao=কোথাও",

        // ---- places
        "bangladesh=বাংলাদেশ","bangla=বাংলা","desh=দেশ","desher=দেশের",
        "bidesh=বিদেশ","ghor=ঘর","ghore=ঘরে","bari=বাড়ি","barite=বাড়িতে","barir=বাড়ির",
        "dokan=দোকান","bazar=বাজার","skul=স্কুল","school=স্কুল","college=কলেজ",
        "hospital=হাসপাতাল","bank=ব্যাংক","restoran=রেস্তোরাঁ","office=অফিস",
        "griha=গৃহ",

        // ---- misc nouns
        "mon=মন","jinish=জিনিস","bepar=ব্যাপার","shortho=শর্টকাট","system=সিস্টেম",
        "khela=খেলা","boi=বই","gan=গান","rupa=রূপা","rupi=রুপি",
        "pani=পানি","jol=জল","cha=চা","chaa=চা","ruti=রুটি","bhat=ভাত",
        "mach=মাছ","mangsho=মাংস","dim=ডিম","dudh=দুধ","mirchi=মরিচ",
        "kobita=কবিতা","kob=কবি","kobi=কবি","likhok=লেখক","bitkary=বিতর্ক",
        "shudh=শুদ্ধ","prem=প্রেম","bhalobasa=ভালোবাসা","bhalobashi=ভালোবাসি",
        "taqat=শক্তি","budhi=বুদ্ধি","buddhi=বুদ্ধি","shingar=শিঙ্গার",
        "kacher=কাচার","kalam=কলম","jibon=জীবন","jagate=জগতে","jagat=জগৎ",
        "janal=জানাল","nicher=নিচের",

        // ---- nature
        "akash=আকাশ","rup=রূপ","suraj=সূর্য","chand=চাঁদ","foron=ফোরন","badal=মেঘ",
        "batash=বাতাস","barsha=বৃষ্টি",

        // ---- greetings / common
        "nomoshkar=নমস্কার","salam=সালাম","wah=ওয়াহ","dhonnobad=ধন্যবাদ",
        "thanks=ধন্যবাদ","hello=হ্যালো","sorry=সরি","accha=আচ্ছা","acha=আচ্ছা",
        "acche=আছে","ache=আছে","accho=আছো","acho=আছো","achen=আছেন",
        "achi=আছি","achho=আছো","achhe=আছে","dost=দোস্ত","lagchi=লাগছি",
        "ekjon=একজন","jon=জন","jons=জনের","joner=জনের",
        "nei=নেই","nai=নাই","chilo=ছিল","chilam=ছিলাম","chilen=ছিলেন",
        "hobe=হবে","hoy=হয়","hoye=হয়ে","hoyni=হয়নি","hocche=হচ্ছে","hocchilo=হচ্ছিল",
        "holo=হলো","koren=করেন","ektu=একটু","darun=দারুণ","janai=জানাই",
        "kemon=কেমন","bolbo=বলবো","khabe=খাবে",
        "kemonachho=কেমন আছো","kemonacho=কেমন আছো","kemonachen=কেমন আছেন",

        // ---- numerals used as words
        "ekta=একটা","ektai=একটাই","duita=দুটো","tin=তিন","char=চার",
        "panch=পাঁচ","choy=ছয়","sat=সাত","aath=আট","noy=নয়","dash=দশ",

        // ---- Banglish technical words that should still convert
        "note=নোট","notebook=নোটবুক","file=ফাইল","folder=ফোল্ডার","link=লিংক",
        "password=পাসওয়ার্ড","account=অ্যাকউন্ট","email=ইমেইল","website=ওয়েবসাইট",
    };

    // ---------------------------------------------------------------- text
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
