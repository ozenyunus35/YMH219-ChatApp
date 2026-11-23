using System.Drawing.Drawing2D;

namespace ChatApp.Ui;

/// <summary>Sunucu ve istemcinin ortak renkleri ve çizim yardımcıları.</summary>
public static class Tema
{
    public static readonly Color Koyu = Color.FromArgb(14, 42, 47);         // yan menü / başlık
    public static readonly Color KoyuIkinci = Color.FromArgb(22, 60, 66);   // koyu zemin üstündeki kutular
    public static readonly Color KoyuCizgi = Color.FromArgb(40, 84, 90);
    public static readonly Color Vurgu = Color.FromArgb(16, 158, 140);      // teal
    public static readonly Color VurguKoyu = Color.FromArgb(11, 122, 108);
    public static readonly Color Zemin = Color.FromArgb(236, 242, 242);     // sohbet alanı
    public static readonly Color Kart = Color.White;
    public static readonly Color Yazi = Color.FromArgb(22, 34, 38);
    public static readonly Color YaziSoluk = Color.FromArgb(104, 122, 128);
    public static readonly Color YaziAcik = Color.FromArgb(214, 232, 232);
    public static readonly Color Cizgi = Color.FromArgb(214, 224, 226);
    public static readonly Color Ozel = Color.FromArgb(124, 77, 196);       // özel mesaj moru
    public static readonly Color Hata = Color.FromArgb(206, 54, 74);
    public static readonly Color Basari = Color.FromArgb(28, 160, 90);
    public static readonly Color Uyari = Color.FromArgb(214, 140, 20);

    // Avatar renkleri: kullanıcı adına göre hep aynı renk seçiliyor
    private static readonly Color[] AvatarRenkleri =
    {
        Color.FromArgb(234, 88, 12), Color.FromArgb(37, 99, 235), Color.FromArgb(219, 39, 119),
        Color.FromArgb(22, 163, 74), Color.FromArgb(147, 51, 234), Color.FromArgb(202, 138, 4),
        Color.FromArgb(8, 145, 178), Color.FromArgb(220, 38, 38)
    };

    public static Color KullaniciRengi(string? ad)
    {
        if (string.IsNullOrEmpty(ad))
            return YaziSoluk;
        if (ad == "sunucu")
            return VurguKoyu;

        // string.GetHashCode her çalıştırmada değişebiliyor, o yüzden basit kendi hash'imiz
        int h = 0;
        foreach (char c in ad.ToLowerInvariant())
            h = unchecked(h * 31 + c);
        return AvatarRenkleri[Math.Abs(h % AvatarRenkleri.Length)];
    }

    public static string BasHarfler(string? ad)
    {
        if (string.IsNullOrWhiteSpace(ad))
            return "?";
        var parcalar = ad.Split('_', '-', '.', ' ').Where(p => p.Length > 0).ToArray();
        string s = parcalar.Length >= 2 ? $"{parcalar[0][0]}{parcalar[1][0]}" : ad[..Math.Min(2, ad.Length)];
        return s.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
    }

    public static void AvatarCiz(Graphics g, string? ad, Rectangle r, Font font)
    {
        using (var b = new SolidBrush(KullaniciRengi(ad)))
            g.FillEllipse(b, r);
        TextRenderer.DrawText(g, BasHarfler(ad), font, r, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    public static GraphicsPath Yuvarlak(RectangleF r, float yaricap)
    {
        float d = Math.Max(1, Math.Min(yaricap * 2, Math.Min(r.Width, r.Height)));
        var yol = new GraphicsPath();
        yol.AddArc(r.X, r.Y, d, d, 180, 90);
        yol.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        yol.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        yol.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        yol.CloseFigure();
        return yol;
    }
}
