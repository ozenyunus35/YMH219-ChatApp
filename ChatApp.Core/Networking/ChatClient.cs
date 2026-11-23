using System.Net.Sockets;
using ChatApp.Core.Protocol;

namespace ChatApp.Core.Networking;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected
}

/// <summary>Kullanıcıya gösterilebilecek, anlaşılır mesajlı bağlantı hatası.</summary>
public sealed class ChatException : Exception
{
    public ChatException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// İstemci tarafı: TcpClient ile sunucuya bağlanır, paket gönderir ve gelenleri olay olarak bildirir.
/// </summary>
public sealed class ChatClient : IDisposable
{
    private readonly EventRaiser events = new();
    private TcpClient? tcp;
    private StreamReader? reader;
    private StreamWriter? writer;
    private readonly SemaphoreSlim writeLock = new(1, 1);
    private CancellationTokenSource? cts;
    private bool userRequestedDisconnect;
    private List<string> onlineUsers = new();

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
    public string? UserName { get; private set; }
    public string? ServerAddress { get; private set; }
    public IReadOnlyList<string> OnlineUsers => onlineUsers;

    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Sunucudan gelen her paket (mesaj, özel mesaj, sistem, hata, yazıyor).</summary>
    public event EventHandler<Packet>? PacketReceived;
    public event EventHandler? StateChanged;
    public event EventHandler? UsersChanged;

    /// <summary>Bağlantı beklenmedik şekilde koptuğunda (kullanıcı kendisi kesmediyse).</summary>
    public event EventHandler<string>? ConnectionLost;

    public async Task ConnectAsync(string host, int port, string userName)
    {
        if (State != ConnectionState.Disconnected)
            throw new InvalidOperationException("Zaten bağlı.");

        if (ChatRules.ValidateUserName(userName) is string nameError)
            throw new ChatException(nameError);

        if (string.IsNullOrWhiteSpace(host))
            throw new ChatException("Sunucu adresi boş olamaz.");

        SetState(ConnectionState.Connecting);
        userRequestedDisconnect = false;
        tcp = new TcpClient();

        try
        {
            using (var timeout = new CancellationTokenSource(ConnectTimeout))
            {
                try
                {
                    await tcp.ConnectAsync(host.Trim(), port, timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new ChatException($"{host}:{port} adresine {ConnectTimeout.TotalSeconds:0} saniyede bağlanılamadı (zaman aşımı).");
                }
                catch (SocketException ex)
                {
                    throw new ChatException(ex.SocketErrorCode switch
                    {
                        SocketError.ConnectionRefused => $"{host}:{port} adresinde çalışan bir sunucu bulunamadı. Sunucu açık mı?",
                        SocketError.HostNotFound or SocketError.NoData => $"\"{host}\" adresi bulunamadı.",
                        _ => $"Sunucuya bağlanılamadı: {ex.Message}"
                    }, ex);
                }

                NetworkStream stream = tcp.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                await writer.WriteLineAsync(Packet.Hello(userName.Trim()).ToJson());

                // İlk cevap ya Welcome ya da Error olmalı
                string? line;
                try
                {
                    line = await reader.ReadLineAsync(timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new ChatException("Sunucu cevap vermedi (zaman aşımı).");
                }

                if (!Packet.TryParse(line, out Packet? answer))
                    throw new ChatException("Sunucu bağlantıyı kapattı veya anlaşılamayan cevap verdi.");

                if (answer!.Type == PacketType.Error)
                    throw new ChatException(answer.Text ?? "Sunucu bağlantıyı reddetti.");

                if (answer.Type != PacketType.Welcome)
                    throw new ChatException("Beklenmeyen sunucu cevabı.");
            }
        }
        catch (Exception ex) when (ex is ChatException or IOException or SocketException)
        {
            CloseSocket();
            SetState(ConnectionState.Disconnected);
            if (ex is ChatException)
                throw;
            throw new ChatException("Bağlantı sırasında hata oluştu: " + ex.Message, ex);
        }

        UserName = userName.Trim();
        ServerAddress = $"{host.Trim()}:{port}";
        cts = new CancellationTokenSource();
        SetState(ConnectionState.Connected);
        // Okuma döngüsü arka planda; olaylar EventRaiser ile UI thread'ine aktarılıyor
        CancellationToken token = cts.Token;
        _ = Task.Run(() => ReadLoopAsync(token));
    }

    public Task SendMessageAsync(string text)
    {
        if (ChatRules.ValidateMessage(text) is string error)
            throw new ArgumentException(error);
        return SendAsync(new Packet { Type = PacketType.Message, Text = text.Trim() });
    }

    public Task SendPrivateAsync(string to, string text)
    {
        if (ChatRules.ValidateMessage(text) is string error)
            throw new ArgumentException(error);
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Alıcı boş olamaz.");
        return SendAsync(new Packet { Type = PacketType.Private, To = to.Trim(), Text = text.Trim() });
    }

    public Task SendTypingAsync() => SendAsync(new Packet { Type = PacketType.Typing });

    public async Task DisconnectAsync()
    {
        if (State == ConnectionState.Disconnected)
            return;

        userRequestedDisconnect = true;
        try
        {
            await SendAsync(Packet.Goodbye());
        }
        catch (ChatException)
        {
            // bağlantı zaten kopmuşsa önemli değil
        }

        cts?.Cancel();
        CloseSocket();
        onlineUsers = new List<string>();
        events.Raise(() => UsersChanged?.Invoke(this, EventArgs.Empty));
        SetState(ConnectionState.Disconnected);
    }

    private async Task SendAsync(Packet packet)
    {
        if (State != ConnectionState.Connected || writer is null)
            throw new ChatException("Sunucuya bağlı değilsiniz.");

        await writeLock.WaitAsync();
        try
        {
            await writer.WriteLineAsync(packet.ToJson());
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or SocketException)
        {
            throw new ChatException("Mesaj gönderilemedi, bağlantı kopmuş olabilir.", ex);
        }
        finally
        {
            writeLock.Release();
        }
    }

    private async Task ReadLoopAsync(CancellationToken token)
    {
        string reason = "Sunucu bağlantıyı kapattı.";
        try
        {
            while (!token.IsCancellationRequested)
            {
                string? line = await reader!.ReadLineAsync(token);
                if (line is null)
                    break;

                if (!Packet.TryParse(line, out Packet? packet))
                    continue;

                if (packet!.Type == PacketType.Users)
                {
                    onlineUsers = packet.Users ?? new List<string>();
                    events.Raise(() => UsersChanged?.Invoke(this, EventArgs.Empty));
                    continue;
                }

                if (packet.Type == PacketType.Error)
                    reason = packet.Text ?? reason;     // ör. sunucudan atılınca sebebi sakla

                events.Raise(() => PacketReceived?.Invoke(this, packet));
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
            reason = "Sunucu ile bağlantı koptu.";
        }

        if (userRequestedDisconnect)
            return;

        CloseSocket();
        onlineUsers = new List<string>();
        State = ConnectionState.Disconnected;
        events.Raise(() =>
        {
            UsersChanged?.Invoke(this, EventArgs.Empty);
            StateChanged?.Invoke(this, EventArgs.Empty);
            ConnectionLost?.Invoke(this, reason);
        });
    }

    private void SetState(ConnectionState state)
    {
        if (State == state)
            return;
        State = state;
        events.Raise(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }

    private void CloseSocket()
    {
        try { tcp?.Close(); } catch (Exception) { /* zaten kapalı */ }
        tcp = null;
        reader = null;
        writer = null;
    }

    public void Dispose()
    {
        userRequestedDisconnect = true;
        cts?.Cancel();
        CloseSocket();
        writeLock.Dispose();
    }
}
