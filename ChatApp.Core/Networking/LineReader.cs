using System.Text;

namespace ChatApp.Core.Networking;

/// <summary>
/// Satır satır okuyan, ama bir satırın uzunluğuna sınır koyan okuyucu.
/// StreamReader.ReadLineAsync sınırsız; satır sonu göndermeyen bir bağlantı belleği doldurabiliyordu.
/// </summary>
public sealed class LineReader
{
    private readonly StreamReader reader;
    private readonly char[] buffer = new char[4096];
    private int pos, len;

    public LineReader(Stream stream, int maxLineLength)
    {
        reader = new StreamReader(stream);
        MaxLineLength = maxLineLength;
    }

    public int MaxLineLength { get; }

    /// <summary>Sonraki satır; bağlantı kapandıysa null. Satır sınırı aşarsa <see cref="InvalidDataException"/>.</summary>
    public async Task<string?> ReadLineAsync(CancellationToken token)
    {
        StringBuilder? sb = null;
        while (true)
        {
            if (pos == len)
            {
                len = await reader.ReadAsync(buffer.AsMemory(), token);
                pos = 0;
                if (len == 0)
                    return sb is { Length: > 0 } ? sb.ToString() : null;
            }

            int nl = Array.IndexOf(buffer, '\n', pos, len - pos);
            int end = nl < 0 ? len : nl;
            sb ??= new StringBuilder();
            sb.Append(buffer, pos, end - pos);
            pos = nl < 0 ? len : nl + 1;

            if (sb.Length > MaxLineLength)
                throw new InvalidDataException($"Satır çok uzun (en fazla {MaxLineLength} karakter).");

            if (nl >= 0)
            {
                if (sb.Length > 0 && sb[^1] == '\r')
                    sb.Length--;
                return sb.ToString();
            }
        }
    }
}
