namespace ChatApp.Core.Protocol;

/// <summary>Kullanıcı adı ve mesaj için ortak kurallar (hem sunucu hem istemci kullanıyor).</summary>
public static class ChatRules
{
    public const int DefaultPort = 5050;
    public const int MinUserNameLength = 2;
    public const int MaxUserNameLength = 20;
    public const int MaxMessageLength = 1000;

    /// <summary>Kullanıcı adı geçerliyse null, değilse hata mesajı döner.</summary>
    public static string? ValidateUserName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Kullanıcı adı boş olamaz.";

        name = name.Trim();
        if (name.Length < MinUserNameLength || name.Length > MaxUserNameLength)
            return $"Kullanıcı adı {MinUserNameLength}-{MaxUserNameLength} karakter olmalı.";

        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                return "Kullanıcı adında sadece harf, rakam, '_', '-' ve '.' olabilir.";
        }

        if (name.Equals("sunucu", StringComparison.OrdinalIgnoreCase))
            return "\"sunucu\" adı ayrılmış, başka bir ad seçin.";

        return null;
    }

    public static string? ValidateMessage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Boş mesaj gönderilemez.";
        if (text.Length > MaxMessageLength)
            return $"Mesaj en fazla {MaxMessageLength} karakter olabilir.";
        return null;
    }

    public static bool TryParsePort(string? text, out int port)
    {
        return int.TryParse(text, out port) && port is >= 1024 and <= 65535;
    }

    /// <summary>
    /// "/w Ali merhaba" şeklindeki özel mesaj komutunu ayrıştırır.
    /// </summary>
    public static bool TryParseWhisper(string input, out string target, out string text)
    {
        target = text = "";
        if (!input.StartsWith("/w ", StringComparison.OrdinalIgnoreCase))
            return false;

        string rest = input[3..].TrimStart();
        int space = rest.IndexOf(' ');
        if (space <= 0)
            return false;

        target = rest[..space];
        text = rest[(space + 1)..].Trim();
        return text.Length > 0;
    }
}
