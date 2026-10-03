using System.Collections.Generic;
using System.Linq;

namespace VampireKeyboard.Services;

public static class SmartWordHandler
{
    private static readonly HashSet<string> CommonEnglishWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "this", "that", "from", "have", "not",
        "are", "was", "you", "your", "will", "can", "what", "when", "where",
        "who", "how", "why", "yes", "no", "ok", "okay", "please", "me",
        "my", "we", "they", "he", "she", "it", "is", "in", "on", "at", "to",
        "do", "does", "did", "done", "get", "got", "go", "going", "come",
        "came", "want", "need", "like", "love", "know", "think", "see",
        "look", "make", "made", "take", "give", "find", "use", "work",
        "time", "year", "people", "way", "day", "man", "thing", "woman",
        "life", "child", "world", "school", "state", "family", "student",
        "group", "country", "problem", "hand", "part", "place", "case",
        "week", "company", "system", "program", "question", "government",
        "number", "night", "point", "home", "water", "room", "mother",
        "area", "money", "story", "fact", "month", "lot", "right", "study",
        "book", "eye", "job", "word", "business", "issue", "side", "kind",
        "head", "house", "service", "friend", "father", "power", "hour",
        "game", "line", "end", "member", "law", "car", "city", "name",
        "team", "minute", "idea", "kid", "body", "information", "back",
        "parent", "face", "others", "level", "office", "door", "health",
        "person", "art", "war", "history", "party", "result", "change",
        "morning", "reason", "research", "girl", "guy", "moment", "air",
        "teacher", "force", "education", "website", "email", "computer",
        "code", "project", "data", "file", "folder", "software", "hardware",
        "internet", "online", "download", "upload", "install", "update",
        "version", "windows", "linux", "android", "application", "app",
        "developer", "development", "design", "database", "server", "client",
        "network", "technology", "digital", "mobile", "phone", "screen",
        "keyboard", "mouse", "laptop", "desktop", "browser", "google",
        "facebook", "youtube", "twitter", "instagram", "whatsapp", "messenger",
        "imo", "zoom", "meeting", "office", "salary", "boss", "interview",
        "resume", "experience", "company", "manager", "employee", "team",
        "task", "deadline", "project", "meeting", "email", "call", "chat",
        "message", "notification", "profile", "account", "password", "login",
        "logout", "signup", "settings", "privacy", "security", "update",
        "upgrade", "backup", "restore", "cloud", "storage", "memory", "battery",
        "charger", "cable", "wifi", "internet", "network", "signal", "balance",
        "recharge", "bill", "payment", "bank", "account", "card", "cash",
        "market", "shop", "store", "price", "discount", "offer", "sale",
        "shopping", "order", "delivery", "product", "quality", "brand",
        "fashion", "style", "color", "size", "shirt", "pant", "shoes",
        "watch", "bag", "book", "pen", "paper", "note", "letter", "document",
    };

    public static bool IsCommonEnglishWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        return CommonEnglishWords.Contains(word.ToLowerInvariant());
    }

    public static bool LooksLikeTechnicalTerm(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        string w = word.ToLowerInvariant();
        return w.StartsWith("http") || w.StartsWith("www.")
            || word.Contains('@') || word.Contains('.')
            || word.Contains('/') || word.Contains('\\')
            || word.Any(char.IsDigit);
    }
}
