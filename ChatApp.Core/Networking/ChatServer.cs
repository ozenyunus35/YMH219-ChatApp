using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Protocol;

namespace ChatApp.Core.Networking;

/// <summary>Sunucu ekranında listelenen bağlı istemci bilgisi.</summary>
public sealed record ClientInfo(Guid Id, string UserName, string Address, DateTime ConnectedAt, int MessageCount);

/// <summary>
/// TcpListener ile bağlantıları kabul eden, gelen mesajları diğer istemcilere ileten sunucu.
/// Her istemci için ayrı bir async okuma döngüsü çalışıyor.
/// </summary>
public sealed class ChatServer : IDisposable
{
    private readonly ConcurrentDictionary<Guid, ClientConnection> clients = new();
    private readonly EventRaiser events = new();
    private TcpListener? listener;
    private CancellationTokenSource? cts;
    private int totalMessages;

    public bool IsRunning { get; private set; }
    public int Port { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public int TotalMessages => totalMessages;

    /// <summary>İlk paketi (Hello) bekleme süresi.</summary>
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public event EventHandler<ClientInfo>? ClientConnected;
    public event EventHandler<ClientInfo>? ClientDisconnected;
    public event EventHandler<Packet>? MessageRelayed;
    public event EventHandler<string>? Log;
    public event EventHandler? StateChanged;

    public IReadOnlyList<ClientInfo> GetClients() =>
        clients.Values.Where(c => c.UserName != null).Select(c => c.ToInfo()).OrderBy(c => c.ConnectedAt).ToList();

    public IReadOnlyList<string> OnlineUsers =>
        clients.Values.Where(c => c.UserName != null).OrderBy(c => c.ConnectedAt).Select(c => c.UserName!).ToList();

    /// <summary>Sunucuyu başlatır. port = 0 verilirse işletim sistemi boş bir port seçer (testler için).</summary>
    public void Start(int port, IPAddress? address = null)
    {
        if (IsRunning)
            throw new InvalidOperationException("Sunucu zaten çalışıyor.");

        listener = new TcpListener(address ?? IPAddress.Any, port);
        try
        {
            listener.Start();
        }
        catch (SocketException ex)
        {
            listener = null;
            throw new ChatException($"Sunucu {port} portunda başlatılamadı. Port başka bir program tarafından kullanılıyor olabilir. ({ex.SocketErrorCode})", ex);
        }

        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        cts = new CancellationTokenSource();
        IsRunning = true;
        StartedAt = DateTime.Now;
        totalMessages = 0;

        WriteLog($"Sunucu başlatıldı, port {Port} dinleniyor.");
        events.Raise(() => StateChanged?.Invoke(this, EventArgs.Empty));

        CancellationToken token = cts.Token;
        _ = Task.Run(() => AcceptLoopAsync(token));
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        cts?.Cancel();
        listener?.Stop();

        foreach (var client in clients.Values)
        {
            client.TrySend(Packet.SystemNotice("Sunucu kapatıldı."));
            client.Close();
        }
        clients.Clear();

        WriteLog("Sunucu durduruldu.");
        events.Raise(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }

    /// <summary>Sunucu ekranından herkese duyuru.</summary>
    public void Announce(string text)
    {
        if (ChatRules.ValidateMessage(text) is string error)
            throw new ArgumentException(error);

        var packet = Packet.Chat("sunucu", text.Trim(), DateTime.Now);
        Broadcast(packet);
        WriteLog($"Duyuru: {text.Trim()}");
        events.Raise(() => MessageRelayed?.Invoke(this, packet));
    }

    /// <summary>Kullanıcıyı sunucudan çıkarır.</summary>
    public bool Kick(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;
        var target = clients.Values.FirstOrDefault(c => c.UserName != null &&
                                                        string.Equals(c.UserName, userName, StringComparison.OrdinalIgnoreCase));
        if (target is null)
            return false;

        target.TrySend(Packet.Fail("Sunucu tarafından bağlantınız kesildi."));
        target.Close();     // kuyruktaki hata mesajı gönderildikten sonra kapanıyor
        return true;
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient tcp;
            try
            {
                tcp = await listener!.AcceptTcpClientAsync(token);
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
            {
                WriteLog("Bağlantı kabul edilemedi: " + ex.Message);
                continue;
            }

            _ = HandleClientAsync(tcp, token);
        }
    }

    private async Task HandleClientAsync(TcpClient tcp, CancellationToken token)
    {
        var connection = new ClientConnection(tcp);
        clients[connection.Id] = connection;

        try
        {
            if (!await HandshakeAsync(connection, token))
                return;

            while (!token.IsCancellationRequested)
            {
                string? line = await connection.ReadLineAsync(token);
                if (line is null)
                    break;      // karşı taraf bağlantıyı kapattı

                if (!Packet.TryParse(line, out Packet? packet))
                {
                    connection.TrySend(Packet.Fail("Anlaşılamayan paket."));
                    continue;
                }

                if (packet!.Type == PacketType.Bye)
                    break;

                HandlePacket(connection, packet);
            }
        }
        catch (InvalidDataException)
        {
            // satır sonu göndermeden dev veri yollayan bağlantı: belleği doldurmasın diye kesiliyor
            connection.TrySend(Packet.Fail("Mesaj çok uzun, bağlantı kesildi."));
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
            // istemci aniden koptu; aşağıda temizleniyor
        }
        finally
        {
            RemoveClient(connection);
        }
    }

    private async Task<bool> HandshakeAsync(ClientConnection connection, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(HandshakeTimeout);

        string? line;
        try
        {
            line = await connection.ReadLineAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            connection.TrySend(Packet.Fail("Zaman aşımı: kullanıcı adı gönderilmedi."));
            return false;
        }
        catch (InvalidDataException)
        {
            connection.TrySend(Packet.Fail("İlk paket çok uzun."));
            return false;
        }

        if (!Packet.TryParse(line, out Packet? hello) || hello!.Type != PacketType.Hello)
        {
            connection.TrySend(Packet.Fail("İlk paket kullanıcı adı (hello) olmalı."));
            return false;
        }

        string name = hello.From?.Trim() ?? "";
        if (ChatRules.ValidateUserName(name) is string error)
        {
            connection.TrySend(Packet.Fail(error));
            return false;
        }

        lock (clients)
        {
            if (clients.Values.Any(c => string.Equals(c.UserName, name, StringComparison.OrdinalIgnoreCase)))
            {
                connection.TrySend(Packet.Fail($"\"{name}\" kullanıcı adı şu anda kullanımda."));
                return false;
            }
            // Önce Welcome kuyruğa giriyor, sonra ad görünür oluyor: aynı anda yapılan bir yayın
            // (mesaj, kullanıcı listesi) Welcome'dan önce gidip istemcinin bağlanmasını bozmasın
            connection.TrySend(Packet.Welcome(name));
            connection.UserName = name;
        }

        Broadcast(Packet.SystemNotice($"{name} sohbete katıldı."), except: connection);
        Broadcast(Packet.UserList(OnlineUsers));

        var info = connection.ToInfo();
        WriteLog($"{name} bağlandı ({connection.Address}).");
        events.Raise(() => ClientConnected?.Invoke(this, info));
        return true;
    }

    private void HandlePacket(ClientConnection sender, Packet packet)
    {
        string from = sender.UserName!;

        switch (packet.Type)
        {
            case PacketType.Message:
            {
                if (ChatRules.ValidateMessage(packet.Text) is string error)
                {
                    sender.TrySend(Packet.Fail(error));
                    return;
                }

                // Zamanı ve göndereni sunucu belirliyor, istemcinin yazdığına güvenmiyoruz
                var relay = Packet.Chat(from, packet.Text!.Trim(), DateTime.Now);
                sender.CountMessage();
                Interlocked.Increment(ref totalMessages);
                Broadcast(relay);
                events.Raise(() => MessageRelayed?.Invoke(this, relay));
                break;
            }

            case PacketType.Private:
            {
                if (ChatRules.ValidateMessage(packet.Text) is string error)
                {
                    sender.TrySend(Packet.Fail(error));
                    return;
                }

                if (string.IsNullOrWhiteSpace(packet.To))
                {
                    sender.TrySend(Packet.Fail("Özel mesajın alıcısı boş."));
                    return;
                }
                // Henüz adını göndermemiş bağlantılar (UserName null) alıcı olamaz
                var target = clients.Values.FirstOrDefault(c => c.UserName != null &&
                                                                string.Equals(c.UserName, packet.To, StringComparison.OrdinalIgnoreCase));
                if (target is null)
                {
                    sender.TrySend(Packet.Fail($"\"{packet.To}\" adında çevrimiçi kullanıcı yok."));
                    return;
                }

                var relay = Packet.PrivateMessage(from, target.UserName!, packet.Text!.Trim(), DateTime.Now);
                sender.CountMessage();
                Interlocked.Increment(ref totalMessages);
                target.TrySend(relay);
                if (target != sender)
                    sender.TrySend(relay);      // gönderen de kendi mesajını görsün
                events.Raise(() => MessageRelayed?.Invoke(this, relay));
                break;
            }

            case PacketType.Typing:
                Broadcast(Packet.TypingFrom(from), except: sender);
                break;

            default:
                sender.TrySend(Packet.Fail($"Bu paket türü istemciden kabul edilmiyor: {packet.Type}"));
                break;
        }
    }

    private void RemoveClient(ClientConnection connection)
    {
        if (!clients.TryRemove(connection.Id, out _))
        {
            connection.Close();
            return;
        }

        connection.Close();
        if (connection.UserName is null)
            return;     // el sıkışma tamamlanmadan ayrıldı

        var info = connection.ToInfo();
        if (IsRunning)
        {
            Broadcast(Packet.SystemNotice($"{connection.UserName} sohbetten ayrıldı."));
            Broadcast(Packet.UserList(OnlineUsers));
        }
        WriteLog($"{connection.UserName} ayrıldı.");
        events.Raise(() => ClientDisconnected?.Invoke(this, info));
    }

    private void Broadcast(Packet packet, ClientConnection? except = null)
    {
        foreach (var client in clients.Values)
        {
            if (client != except && client.UserName != null)
                client.TrySend(packet);
        }
    }

    private void WriteLog(string text) => events.Raise(() => Log?.Invoke(this, text));

    public void Dispose()
    {
        Stop();
        cts?.Dispose();
    }

    // ---------------------------------------------------------------

    /// <summary>Sunucunun tek bir istemciyle olan bağlantısı.</summary>
    private sealed class ClientConnection
    {
        // JSON'da Türkçe karakterler \uXXXX olarak 6 katına çıkabiliyor
        private const int MaxLineLength = ChatRules.MaxMessageLength * 6 + 512;
        private const int MaxQueuedPackets = 2048;   // yoğun sohbette normal istemci atılmasın, takılan istemci yine de sınırlı

        private readonly TcpClient tcp;
        private readonly LineReader reader;
        private readonly StreamWriter writer;
        // Her istemcinin kendi gönderim kuyruğu ve yazıcısı var: mesaj okumayan (ağı tıkanan) bir istemci
        // yayın yapan thread'i ve dolayısıyla bütün sunucuyu kilitleyemiyor
        private readonly System.Threading.Channels.Channel<string> outbox =
            System.Threading.Channels.Channel.CreateBounded<string>(new System.Threading.Channels.BoundedChannelOptions(MaxQueuedPackets)
            {
                SingleReader = true,
                FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
            });
        private readonly Task writerTask;
        private int messageCount;
        private int closed;

        public ClientConnection(TcpClient tcp)
        {
            this.tcp = tcp;
            NetworkStream stream = tcp.GetStream();
            reader = new LineReader(stream, MaxLineLength);
            writer = new StreamWriter(stream) { AutoFlush = true };
            Address = tcp.Client.RemoteEndPoint?.ToString() ?? "?";
            writerTask = Task.Run(WriteLoopAsync);
        }

        private async Task WriteLoopAsync()
        {
            try
            {
                await foreach (string line in outbox.Reader.ReadAllAsync())
                    await writer.WriteLineAsync(line);
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException or SocketException)
            {
                // bağlantı koptu; okuma tarafı temizliyor
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? UserName { get; set; }
        public string Address { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public void CountMessage() => Interlocked.Increment(ref messageCount);

        public ClientInfo ToInfo() => new(Id, UserName ?? "?", Address, ConnectedAt, messageCount);

        public Task<string?> ReadLineAsync(CancellationToken token) => reader.ReadLineAsync(token);

        /// <summary>
        /// Paketi kuyruğa koyar, beklemez. Kuyruk doluysa istemci mesajları okumuyor demektir:
        /// sunucuyu yavaşlatmasın diye bağlantısı kesiliyor.
        /// </summary>
        public void TrySend(Packet packet)
        {
            if (Volatile.Read(ref closed) == 1)
                return;
            if (!outbox.Writer.TryWrite(packet.ToJson()))
                CloseNow();
        }

        /// <summary>Kuyrukta bekleyen paketleri (ör. "neden atıldınız" mesajı) kısa süre gönderip kapatır.</summary>
        public void Close()
        {
            outbox.Writer.TryComplete();
            try { writerTask.Wait(500); } catch (AggregateException) { }
            CloseNow();
        }

        private void CloseNow()
        {
            if (Interlocked.Exchange(ref closed, 1) == 1)
                return;
            outbox.Writer.TryComplete();
            try { tcp.Close(); } catch (Exception) { /* zaten kapalı */ }
        }
    }
}
