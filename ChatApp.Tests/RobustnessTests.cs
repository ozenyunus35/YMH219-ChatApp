using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Networking;
using ChatApp.Core.Protocol;

namespace ChatApp.Tests;

/// <summary>Kod incelemesinde bulunan ağ hatalarının tekrar etmemesi için: yavaş istemci, boş alıcı, katılma sırası, uzun satır, geçersiz port.</summary>
public class RobustnessTests
{
    static readonly TimeSpan T = TimeSpan.FromSeconds(15);

    static Packet P(string line) { Assert.True(Packet.TryParse(line, out var p)); return p!; }

    static ChatServer Start(TimeSpan? hs = null)
    {
        var s = new ChatServer();
        if (hs != null) s.HandshakeTimeout = hs.Value;
        s.Start(0, IPAddress.Loopback);
        return s;
    }

    sealed class Raw : IDisposable
    {
        public TcpClient Tcp = new();
        public StreamReader R = null!;
        public StreamWriter W = null!;
        public static async Task<Raw> Open(int port)
        {
            var r = new Raw();
            await r.Tcp.ConnectAsync(IPAddress.Loopback, port);
            var s = r.Tcp.GetStream();
            r.R = new StreamReader(s); r.W = new StreamWriter(s) { AutoFlush = true };
            return r;
        }
        public static async Task<Raw> Join(int port, string name)
        {
            var r = await Open(port);
            await r.W.WriteLineAsync(Packet.Hello(name).ToJson());
            var l = await r.R.ReadLineAsync().WaitAsync(T);
            Assert.Equal(PacketType.Welcome, P(l!).Type);
            return r;
        }
        public void Dispose() => Tcp.Close();
    }

    [Fact]
    public async Task SlowReader_MustNotBlockAnnounce()
    {
        using var server = Start();
        using var slow = await Raw.Join(server.Port, "slow");
        var text = new string('x', 1000);
        // Okumayan istemcinin ağ tamponları dolana kadar duyuru gönder; hiçbir çağrı takılmamalı
        long enUzunMs = 0; int adet = 0;
        var task = Task.Run(() =>
        {
            var toplam = System.Diagnostics.Stopwatch.StartNew();
            while (toplam.Elapsed < TimeSpan.FromSeconds(4) || adet < 3000)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                server.Announce(text);
                enUzunMs = Math.Max(enUzunMs, sw.ElapsedMilliseconds);
                adet++;
                if (adet >= 30000) break;
            }
        });
        var done = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30)));
        Assert.True(done == task, "Announce() blocked because one client does not read");
        Assert.True(enUzunMs < 2000, $"one Announce() call took {enUzunMs} ms ({adet} calls)");
    }

    [Fact]
    public async Task PrivateWithoutTo_MustNotReachUnauthenticatedConnection()
    {
        using var server = Start();
        using var pending = await Raw.Open(server.Port);
        using var bob = await Raw.Join(server.Port, "bob");
        await bob.W.WriteLineAsync("{\"type\":\"private\",\"text\":\"secret\"}");
        var read = pending.R.ReadLineAsync();
        var done = await Task.WhenAny(read, Task.Delay(1500));
        Assert.True(done != read, "unauthenticated connection received: " + (done == read ? read.Result : ""));
    }

    [Fact]
    public async Task ConcurrentChatter_JoinAlwaysGetsWelcomeFirst()
    {
        using var server = Start();
        using var spammer = await Raw.Join(server.Port, "spam");
        // Sürekli mesaj yağdıran istemci: katılmalar sırasında yayın yapılıyor. Arada bir nefes alıyor ki
        // 2 çekirdekli makinede diğer bağlantıların yazıcılarına hiç sıra gelmemesi durumu (gerçekçi değil) oluşmasın.
        _ = Task.Run(async () =>
        {
            try
            {
                for (int n = 0; ; n++)
                {
                    await spammer.W.WriteLineAsync("{\"type\":\"message\",\"text\":\"hi\"}");
                    if (n % 20 == 0) await Task.Delay(1);
                }
            }
            catch { }
        });
        _ = Task.Run(async () => { try { while (true) await spammer.R.ReadLineAsync(); } catch { } });
        int failures = 0; string? first = null;
        await Parallel.ForEachAsync(Enumerable.Range(0, 300), new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (i, _) =>
        {
            using var c = new ChatClient();
            try { await c.ConnectAsync("127.0.0.1", server.Port, "u" + i).WaitAsync(T); await c.DisconnectAsync(); }
            catch (ChatException e) { Interlocked.Increment(ref failures); first ??= e.Message; }
        });
        Assert.True(failures == 0, $"{failures}/300 joins failed, e.g. '{first}'");
    }

    [Fact]
    public async Task HugeLineWithoutNewline_ServerMustDropConnection()
    {
        using var server = Start();
        using var evil = await Raw.Join(server.Port, "evil");
        var chunk = new string('a', 1 << 20);
        bool dropped = false;
        try { for (int i = 0; i < 64; i++) { await evil.W.WriteAsync(chunk).WaitAsync(T); await Task.Delay(5); } }
        catch (IOException) { dropped = true; }
        Assert.True(dropped, "server accepted 64 MB without a newline");
    }

    [Fact]
    public async Task ConnectAsync_InvalidPort_MustResetState()
    {
        using var c = new ChatClient();
        Exception? ex = null;
        try { await c.ConnectAsync("127.0.0.1", 70000, "alice"); } catch (Exception e) { ex = e; }
        Assert.True(c.State == ConnectionState.Disconnected, $"State={c.State}, ex={ex?.GetType().Name}");
    }

    [Fact]
    public async Task DisconnectDuringConnect_ConnectMustFailCleanly()
    {
        var l = new TcpListener(IPAddress.Loopback, 0); l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        _ = l.AcceptTcpClientAsync();
        using var c = new ChatClient();
        var connect = c.ConnectAsync("127.0.0.1", port, "alice");
        await Task.Delay(300);
        await c.DisconnectAsync();
        var ex = await Record.ExceptionAsync(() => connect.WaitAsync(T));
        l.Stop();
        Assert.True(ex is ChatException or OperationCanceledException, "got: " + ex?.GetType().Name + " " + ex?.Message);
        Assert.Equal(ConnectionState.Disconnected, c.State);
    }

    [Fact]
    public async Task Control_NoHelloIsTimedOut()
    {
        using var server = Start(TimeSpan.FromMilliseconds(300));
        using var r = await Raw.Open(server.Port);
        var end = await Task.Run(async () => { string? l; do l = await r.R.ReadLineAsync(); while (l != null); return true; }).WaitAsync(T);
        Assert.True(end);
    }

    [Fact]
    public async Task Control_SameNameSimultaneous_OnlyOneWins()
    {
        using var server = Start();
        for (int round = 0; round < 20; round++)
        {
            string n = "dup" + round;
            var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(async k =>
            {
                using var c = new ChatClient();
                try { await c.ConnectAsync("127.0.0.1", server.Port, k % 2 == 0 ? n : n.ToUpper()).WaitAsync(T); await Task.Delay(300); return true; }
                catch (ChatException) { return false; }
            }));
            Assert.Equal(1, results.Count(x => x));
        }
    }

    [Fact]
    public async Task Control_ConcurrentSendsNotInterleaved()
    {
        using var server = Start();
        using var a = await Raw.Join(server.Port, "aa");
        using var b = await Raw.Join(server.Port, "bb");
        var text = new string('z', 900);
        int got = 0;
        var reader = Task.Run(async () => { while (got < 1200) { var l = await b.R.ReadLineAsync(); if (l == null) break; if (P(l).Type == PacketType.Message) got++; } });
        _ = Task.Run(async () => { while (await a.R.ReadLineAsync() != null) { } });
        var senders = Enumerable.Range(0, 4).Select(_ => Task.Run(() => { for (int i = 0; i < 300; i++) server.Announce(text); })).ToArray();
        await Task.WhenAll(senders); await reader.WaitAsync(T);
        Assert.Equal(1200, got);
    }

    [Fact]
    public async Task Control_HelloTwiceAndGarbage_ConnectionSurvives()
    {
        using var server = Start();
        using var a = await Raw.Join(server.Port, "aa");
        foreach (var bad in new[] { Packet.Hello("aa").ToJson(), "{", "null", "{\"type\":99}", "{\"type\":\"message\",\"text\":null}", "{\"type\":\"private\",\"to\":\"zz\",\"text\":\"x\"}", "[1,2]", "{\"time\":\"bogus\"}" })
            await a.W.WriteLineAsync(bad);
        await a.W.WriteLineAsync("{\"type\":\"message\",\"text\":\"alive\"}");
        string? l; bool alive = false;
        while ((l = await a.R.ReadLineAsync().WaitAsync(T)) != null) { if (l.Contains("alive")) { alive = true; break; } }
        Assert.True(alive);
    }
}
