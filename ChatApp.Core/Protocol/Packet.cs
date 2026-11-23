using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp.Core.Protocol;

/// <summary>
/// Sunucu ile istemci arasında gidip gelen her şeyin türü.
/// </summary>
public enum PacketType
{
    Hello,      // istemci -> sunucu: kullanıcı adıyla katılma isteği
    Welcome,    // sunucu -> istemci: katılım kabul edildi
    Message,    // herkese açık mesaj
    Private,    // özel mesaj (sadece alıcı ve gönderen görür)
    Users,      // çevrimiçi kullanıcı listesi
    System,     // "Ali katıldı" gibi sistem bildirimi
    Typing,     // "yazıyor..." bilgisi
    Error,      // hata (ör. kullanıcı adı alınmış)
    Bye         // bağlantıyı düzgünce kapatma
}

/// <summary>
/// Ağ üzerinden gönderilen tek paket. Her paket tek satırlık JSON olarak yazılıyor,
/// bu yüzden StreamReader.ReadLine ile paketleri birbirinden ayırmak kolay.
/// (Mesajın içindeki satır sonları JSON'da \n olarak kaçışlandığı için sorun çıkarmıyor.)
/// </summary>
public sealed class Packet
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public PacketType Type { get; init; }
    public string? From { get; init; }
    public string? To { get; init; }
    public string? Text { get; init; }
    public DateTime? Time { get; init; }
    public List<string>? Users { get; init; }

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    public static bool TryParse(string? line, out Packet? packet)
    {
        packet = null;
        if (string.IsNullOrWhiteSpace(line))
            return false;

        try
        {
            packet = JsonSerializer.Deserialize<Packet>(line, Options);
            if (packet is null || !Enum.IsDefined(packet.Type))
            {
                packet = null;
                return false;
            }
            return true;
        }
        catch (JsonException)
        {
            packet = null;
            return false;
        }
    }

    // Okunabilirlik için küçük fabrika metotları
    public static Packet Hello(string user) => new() { Type = PacketType.Hello, From = user };
    public static Packet Welcome(string user) => new() { Type = PacketType.Welcome, To = user, Time = DateTime.Now };
    public static Packet Chat(string from, string text, DateTime time) => new() { Type = PacketType.Message, From = from, Text = text, Time = time };
    public static Packet PrivateMessage(string from, string to, string text, DateTime time) => new() { Type = PacketType.Private, From = from, To = to, Text = text, Time = time };
    public static Packet UserList(IEnumerable<string> users) => new() { Type = PacketType.Users, Users = users.ToList() };
    public static Packet SystemNotice(string text) => new() { Type = PacketType.System, Text = text, Time = DateTime.Now };
    public static Packet TypingFrom(string user) => new() { Type = PacketType.Typing, From = user };
    public static Packet Fail(string text) => new() { Type = PacketType.Error, Text = text, Time = DateTime.Now };
    public static Packet Goodbye() => new() { Type = PacketType.Bye };
}
