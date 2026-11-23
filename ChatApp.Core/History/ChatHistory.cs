using System.Text;
using ChatApp.Core.Protocol;

namespace ChatApp.Core.History;

/// <summary>
/// Ekranda görünen mesajları biriktirir ve okunabilir bir .txt dosyasına yazar.
/// </summary>
public sealed class ChatHistory
{
    private readonly List<Packet> items = new();

    public IReadOnlyList<Packet> Items => items;

    public void Add(Packet packet)
    {
        if (packet.Type is PacketType.Message or PacketType.Private or PacketType.System or PacketType.Error)
            items.Add(packet);
    }

    public void Clear() => items.Clear();

    public static string FormatLine(Packet p)
    {
        string time = (p.Time ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm:ss");
        return p.Type switch
        {
            PacketType.Message => $"[{time}] {p.From}: {p.Text}",
            PacketType.Private => $"[{time}] (özel) {p.From} → {p.To}: {p.Text}",
            PacketType.System => $"[{time}] * {p.Text}",
            PacketType.Error => $"[{time}] ! {p.Text}",
            _ => $"[{time}] {p.Type}"
        };
    }

    public void Save(string path, string? header = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var writer = new StreamWriter(path, append: false, new UTF8Encoding(true));
        writer.WriteLine($"# Sohbet geçmişi - {DateTime.Now:dd.MM.yyyy HH:mm}");
        if (!string.IsNullOrEmpty(header))
            writer.WriteLine("# " + header);
        writer.WriteLine($"# {items.Count} kayıt");
        writer.WriteLine();

        foreach (var p in items)
            writer.WriteLine(FormatLine(p));
    }

    public static string SuggestFileName(string user, DateTime now) => $"sohbet-{user}-{now:yyyyMMdd-HHmm}.txt";
}
