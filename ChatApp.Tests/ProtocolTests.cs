using ChatApp.Core.History;
using ChatApp.Core.Protocol;

namespace ChatApp.Tests;

public class PacketTests
{
    [Fact]
    public void Message_RoundTripsThroughJson()
    {
        var time = new DateTime(2026, 11, 24, 10, 30, 15);
        var original = Packet.Chat("Ayşe", "Merhaba \"dünya\"\nikinci satır", time);

        string json = original.ToJson();
        bool ok = Packet.TryParse(json, out var parsed);

        Assert.True(ok);
        Assert.Equal(PacketType.Message, parsed!.Type);
        Assert.Equal("Ayşe", parsed.From);
        Assert.Equal(original.Text, parsed.Text);
        Assert.Equal(time, parsed.Time);
        Assert.DoesNotContain('\n', json);      // tek satır olmalı, yoksa satır bazlı okuma bozulur
    }

    [Fact]
    public void UserList_RoundTrips()
    {
        var packet = Packet.UserList(new[] { "ali", "veli" });

        Packet.TryParse(packet.ToJson(), out var parsed);

        Assert.Equal(new[] { "ali", "veli" }, parsed!.Users);
    }

    [Fact]
    public void NullFields_AreNotWritten()
    {
        string json = Packet.Goodbye().ToJson();

        Assert.Equal("{\"type\":\"bye\"}", json);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("merhaba")]
    [InlineData("{\"type\":")]
    [InlineData("{\"type\":\"uçmak\"}")]
    [InlineData("{\"type\":99}")]
    [InlineData("null")]
    public void TryParse_RejectsGarbage(string line)
    {
        Assert.False(Packet.TryParse(line, out var p));
        Assert.Null(p);
    }
}

public class ChatRulesTests
{
    [Theory]
    [InlineData("ali")]
    [InlineData("Yunus_Emre")]
    [InlineData("çağrı.ş")]
    [InlineData("user-42")]
    public void ValidUserNames(string name)
    {
        Assert.Null(ChatRules.ValidateUserName(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("bu-kullanici-adi-cok-uzun")]
    [InlineData("ali veli")]
    [InlineData("ali!")]
    [InlineData("Sunucu")]
    public void InvalidUserNames(string? name)
    {
        Assert.NotNull(ChatRules.ValidateUserName(name));
    }

    [Fact]
    public void Message_EmptyOrTooLong_IsRejected()
    {
        Assert.NotNull(ChatRules.ValidateMessage("  "));
        Assert.NotNull(ChatRules.ValidateMessage(new string('x', ChatRules.MaxMessageLength + 1)));
        Assert.Null(ChatRules.ValidateMessage(new string('x', ChatRules.MaxMessageLength)));
    }

    [Theory]
    [InlineData("5050", true, 5050)]
    [InlineData("80", false, 0)]
    [InlineData("70000", false, 0)]
    [InlineData("abc", false, 0)]
    public void Port_Parsing(string text, bool ok, int expected)
    {
        Assert.Equal(ok, ChatRules.TryParsePort(text, out int port));
        if (ok) Assert.Equal(expected, port);
    }

    [Fact]
    public void Whisper_Command_IsParsed()
    {
        Assert.True(ChatRules.TryParseWhisper("/w Veli selam nasılsın", out var to, out var text));
        Assert.Equal("Veli", to);
        Assert.Equal("selam nasılsın", text);

        Assert.False(ChatRules.TryParseWhisper("/w Veli", out _, out _));
        Assert.False(ChatRules.TryParseWhisper("selam", out _, out _));
    }
}

public class ChatHistoryTests
{
    [Fact]
    public void FormatLine_ShowsTimeSenderAndText()
    {
        var t = new DateTime(2026, 11, 24, 9, 5, 0);

        Assert.Equal("[24.11.2026 09:05:00] Ali: selam", ChatHistory.FormatLine(Packet.Chat("Ali", "selam", t)));
        Assert.Equal("[24.11.2026 09:05:00] (özel) Ali → Veli: gizli", ChatHistory.FormatLine(Packet.PrivateMessage("Ali", "Veli", "gizli", t)));
    }

    [Fact]
    public void Add_IgnoresTypingAndUserLists()
    {
        var history = new ChatHistory();
        history.Add(Packet.TypingFrom("Ali"));
        history.Add(Packet.UserList(new[] { "Ali" }));
        history.Add(Packet.Chat("Ali", "selam", DateTime.Now));

        Assert.Single(history.Items);
    }

    [Fact]
    public void Save_WritesAllLines()
    {
        string path = Path.Combine(Path.GetTempPath(), $"chat-test-{Guid.NewGuid():N}.txt");
        try
        {
            var history = new ChatHistory();
            history.Add(Packet.SystemNotice("Ali katıldı"));
            history.Add(Packet.Chat("Ali", "ilk mesaj", DateTime.Now));

            history.Save(path, "Kullanıcı: Ali");
            string[] lines = File.ReadAllLines(path);

            Assert.Contains(lines, l => l.EndsWith("Ali: ilk mesaj"));
            Assert.Contains(lines, l => l.EndsWith("* Ali katıldı"));
            Assert.Contains("# Kullanıcı: Ali", lines);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
