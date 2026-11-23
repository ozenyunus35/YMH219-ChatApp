using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Networking;
using ChatApp.Core.Protocol;

namespace ChatApp.Tests;

/// <summary>
/// Gerçek TCP bağlantısıyla (localhost) sunucu + istemci testleri.
/// Her test boş bir port alıyor, böylece testler birbirini etkilemiyor.
/// </summary>
public class ServerClientTests : IDisposable
{
    private readonly ChatServer server = new();
    private readonly List<ChatClient> clients = new();

    public ServerClientTests()
    {
        server.Start(0, IPAddress.Loopback);
    }

    public void Dispose()
    {
        foreach (var c in clients) c.Dispose();
        server.Dispose();
    }

    private async Task<(ChatClient Client, ConcurrentQueue<Packet> Inbox)> Connect(string name)
    {
        var client = new ChatClient();
        var inbox = new ConcurrentQueue<Packet>();
        client.PacketReceived += (_, p) => inbox.Enqueue(p);
        clients.Add(client);
        await client.ConnectAsync("127.0.0.1", server.Port, name);
        return (client, inbox);
    }

    private static async Task<bool> WaitUntil(Func<bool> condition, int ms = 3000)
    {
        var deadline = DateTime.Now.AddMilliseconds(ms);
        while (DateTime.Now < deadline)
        {
            if (condition()) return true;
            await Task.Delay(20);
        }
        return condition();
    }

    [Fact]
    public async Task Client_Connects_AndAppearsInServerList()
    {
        var (client, _) = await Connect("ali");

        Assert.Equal(ConnectionState.Connected, client.State);
        Assert.True(await WaitUntil(() => server.GetClients().Any(c => c.UserName == "ali")));
        Assert.Contains("127.0.0.1", server.GetClients()[0].Address);
    }

    [Fact]
    public async Task Message_IsDeliveredToEveryone_WithServerTime()
    {
        var (ali, aliInbox) = await Connect("ali");
        var (_, veliInbox) = await Connect("veli");
        DateTime before = DateTime.Now.AddSeconds(-1);

        await ali.SendMessageAsync("herkese selam");

        Assert.True(await WaitUntil(() => veliInbox.Any(p => p.Type == PacketType.Message)));
        Assert.True(await WaitUntil(() => aliInbox.Any(p => p.Type == PacketType.Message)));
        var received = veliInbox.First(p => p.Type == PacketType.Message);
        Assert.Equal("ali", received.From);
        Assert.Equal("herkese selam", received.Text);
        Assert.True(received.Time >= before);
        Assert.Equal(1, server.TotalMessages);
    }

    [Fact]
    public async Task PrivateMessage_OnlyReachesTargetAndSender()
    {
        var (ali, aliInbox) = await Connect("ali");
        var (_, veliInbox) = await Connect("veli");
        var (_, ayseInbox) = await Connect("ayse");

        await ali.SendPrivateAsync("veli", "sadece sana");

        Assert.True(await WaitUntil(() => veliInbox.Any(p => p.Type == PacketType.Private)));
        Assert.True(await WaitUntil(() => aliInbox.Any(p => p.Type == PacketType.Private)));
        await Task.Delay(200);
        Assert.DoesNotContain(ayseInbox, p => p.Type == PacketType.Private);
    }

    [Fact]
    public async Task PrivateMessage_ToUnknownUser_ReturnsError()
    {
        var (ali, aliInbox) = await Connect("ali");

        await ali.SendPrivateAsync("hayalet", "kimse yok mu");

        Assert.True(await WaitUntil(() => aliInbox.Any(p => p.Type == PacketType.Error && p.Text!.Contains("hayalet"))));
    }

    [Fact]
    public async Task DuplicateUserName_IsRejected()
    {
        await Connect("ali");

        var second = new ChatClient();
        clients.Add(second);
        var ex = await Assert.ThrowsAsync<ChatException>(() => second.ConnectAsync("127.0.0.1", server.Port, "ALI"));

        Assert.Contains("kullanımda", ex.Message);
        Assert.Equal(ConnectionState.Disconnected, second.State);
    }

    [Fact]
    public async Task InvalidUserName_IsRejectedBeforeConnecting()
    {
        var client = new ChatClient();
        clients.Add(client);

        await Assert.ThrowsAsync<ChatException>(() => client.ConnectAsync("127.0.0.1", server.Port, "a b"));
    }

    [Fact]
    public async Task UserList_IsSentToEveryone_AndUpdatesOnLeave()
    {
        var (ali, _) = await Connect("ali");
        var (veli, _) = await Connect("veli");

        Assert.True(await WaitUntil(() => ali.OnlineUsers.Count == 2));

        await veli.DisconnectAsync();

        Assert.True(await WaitUntil(() => ali.OnlineUsers.SequenceEqual(new[] { "ali" })));
        Assert.True(await WaitUntil(() => server.GetClients().Count == 1));
    }

    [Fact]
    public async Task JoinAndLeave_ProduceSystemMessages()
    {
        var (_, aliInbox) = await Connect("ali");
        var (veli, _) = await Connect("veli");

        Assert.True(await WaitUntil(() => aliInbox.Any(p => p.Type == PacketType.System && p.Text!.Contains("veli sohbete katıldı"))));
        await veli.DisconnectAsync();
        Assert.True(await WaitUntil(() => aliInbox.Any(p => p.Type == PacketType.System && p.Text!.Contains("veli sohbetten ayrıldı"))));
    }

    [Fact]
    public async Task Typing_IsRelayedToOthersOnly()
    {
        var (ali, aliInbox) = await Connect("ali");
        var (_, veliInbox) = await Connect("veli");

        await ali.SendTypingAsync();

        Assert.True(await WaitUntil(() => veliInbox.Any(p => p.Type == PacketType.Typing && p.From == "ali")));
        await Task.Delay(150);
        Assert.DoesNotContain(aliInbox, p => p.Type == PacketType.Typing);
    }

    [Fact]
    public async Task ServerStop_ClientNoticesConnectionLost_WithoutCrashing()
    {
        var (ali, _) = await Connect("ali");
        string? lostReason = null;
        ali.ConnectionLost += (_, r) => lostReason = r;

        server.Stop();

        Assert.True(await WaitUntil(() => ali.State == ConnectionState.Disconnected));
        Assert.True(await WaitUntil(() => lostReason != null));
        await Assert.ThrowsAsync<ChatException>(() => ali.SendMessageAsync("kimse yok"));
    }

    [Fact]
    public async Task ConnectingToClosedPort_GivesFriendlyError()
    {
        // Boş bir port bulup hemen kapatıyoruz: orada dinleyen kimse yok
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int freePort = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        var client = new ChatClient();
        clients.Add(client);
        var ex = await Assert.ThrowsAsync<ChatException>(() => client.ConnectAsync("127.0.0.1", freePort, "ali"));

        Assert.Contains("sunucu", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task Kick_DisconnectsUser_WithReason()
    {
        var (ali, _) = await Connect("ali");
        string? reason = null;
        ali.ConnectionLost += (_, r) => reason = r;

        Assert.True(server.Kick("ali"));

        Assert.True(await WaitUntil(() => reason != null));
        Assert.Contains("kesildi", reason);
        Assert.False(server.Kick("olmayan"));
    }

    [Fact]
    public async Task Announcement_ReachesClients_AsServer()
    {
        var (_, inbox) = await Connect("ali");

        server.Announce("Bakım 5 dakika sonra");

        Assert.True(await WaitUntil(() => inbox.Any(p => p.Type == PacketType.Message && p.From == "sunucu")));
    }

    [Fact]
    public async Task TooLongMessage_IsRejectedOnClient()
    {
        var (ali, _) = await Connect("ali");

        await Assert.ThrowsAsync<ArgumentException>(() => ali.SendMessageAsync(new string('x', 5000)));
    }

    [Fact]
    public async Task RawGarbage_FromClient_DoesNotCrashServer()
    {
        using var raw = new TcpClient();
        await raw.ConnectAsync(IPAddress.Loopback, server.Port);
        var writer = new StreamWriter(raw.GetStream()) { AutoFlush = true };
        var reader = new StreamReader(raw.GetStream());

        await writer.WriteLineAsync("bu json değil");
        string? answer = await reader.ReadLineAsync();

        Assert.True(Packet.TryParse(answer, out var p));
        Assert.Equal(PacketType.Error, p!.Type);
        Assert.True(server.IsRunning);

        // sunucu hâlâ yeni bağlantı kabul ediyor
        var (ali, _) = await Connect("ali");
        Assert.Equal(ConnectionState.Connected, ali.State);
    }

    [Fact]
    public void StartingTwice_Throws_AndBusyPort_GivesFriendlyError()
    {
        Assert.Throws<InvalidOperationException>(() => server.Start(0, IPAddress.Loopback));

        using var other = new ChatServer();
        var ex = Assert.Throws<ChatException>(() => other.Start(server.Port, IPAddress.Loopback));
        Assert.Contains(server.Port.ToString(), ex.Message);
    }
}
