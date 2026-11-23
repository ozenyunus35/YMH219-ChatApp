using System.ComponentModel;
using System.Drawing.Drawing2D;
using ChatApp.Core.Protocol;

namespace ChatApp.Ui;

/// <summary>
/// Mesajları balon şeklinde çizen kaydırılabilir liste.
/// Her mesajın konumu bir kez hesaplanıp saklanıyor (Yerlesim), çizimde sadece görünenler çiziliyor.
/// </summary>
public class MesajListesi : ScrollableControl
{
    private const int AvatarBoyu = 34;
    private const int Bosluk = 14;
    private const int BalonIcBoslugu = 10;

    private readonly List<Packet> mesajlar = new();
    private readonly List<Yerlesim> yerlesim = new();
    private int toplamYukseklik;

    private readonly Font metinFontu = new("Segoe UI", 10F);
    private readonly Font adFontu = new("Segoe UI Semibold", 9F);
    private readonly Font saatFontu = new("Segoe UI", 8F);
    private readonly Font avatarFontu = new("Segoe UI Semibold", 9F);
    private readonly Font sistemFontu = new("Segoe UI", 8.5F);

    private sealed record Yerlesim(Packet Mesaj, Rectangle Balon, bool Benim, bool BaslikGoster, int Yukseklik);

    public MesajListesi()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Tema.Zemin;
        AutoScroll = true;
    }

    private string? benimAdim;

    /// <summary>Kendi mesajlarımızı sağa yaslamak için.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? BenimAdim
    {
        get => benimAdim;
        set { benimAdim = value; HepsiniYerlestir(); }
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Invalidate();
    }

    [Browsable(false)]
    public int MesajSayisi => mesajlar.Count;

    [Browsable(false)]
    public IReadOnlyList<Packet> Mesajlar => mesajlar;

    public void Ekle(Packet mesaj)
    {
        bool enAlttaydi = -AutoScrollPosition.Y + ClientSize.Height >= toplamYukseklik - 40;
        mesajlar.Add(mesaj);
        YerlesimEkle(mesaj, ClientSize.Width);
        AutoScrollMinSize = new Size(0, toplamYukseklik);

        if (enAlttaydi || mesaj.From == BenimAdim)
            EnAltaKaydir();
        Invalidate();
    }

    public void Temizle()
    {
        mesajlar.Clear();
        yerlesim.Clear();
        toplamYukseklik = Bosluk;
        AutoScrollMinSize = Size.Empty;
        Invalidate();
    }

    public void EnAltaKaydir()
    {
        AutoScrollPosition = new Point(0, Math.Max(0, toplamYukseklik - ClientSize.Height));
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        HepsiniYerlestir();
    }

    private void HepsiniYerlestir()
    {
        yerlesim.Clear();
        toplamYukseklik = Bosluk;
        foreach (var m in mesajlar)
            YerlesimEkle(m, ClientSize.Width);
        AutoScrollMinSize = new Size(0, toplamYukseklik);
        Invalidate();
    }

    private void YerlesimEkle(Packet m, int genislik)
    {
        int y = toplamYukseklik;

        if (m.Type is PacketType.System or PacketType.Error)
        {
            Size s = TextRenderer.MeasureText(m.Text ?? "", sistemFontu);
            int w = s.Width + 24;
            var r = new Rectangle((genislik - w) / 2, y, w, s.Height + 10);
            yerlesim.Add(new Yerlesim(m, r, false, false, r.Height));
            toplamYukseklik += r.Height + 10;
            return;
        }

        bool benim = m.From != null && m.From == BenimAdim;

        // Aynı kişi 2 dakika içinde arka arkaya yazdıysa adı ve avatarı tekrar gösterme
        Yerlesim? onceki = yerlesim.Count > 0 ? yerlesim[^1] : null;
        bool baslik = onceki is null
                      || onceki.Mesaj.Type is PacketType.System or PacketType.Error
                      || onceki.Mesaj.From != m.From
                      || onceki.Mesaj.Type != m.Type
                      || (m.Time - onceki.Mesaj.Time) > TimeSpan.FromMinutes(2);

        int maksBalon = Math.Max(160, (int)(genislik * 0.62));
        Size metin = TextRenderer.MeasureText(m.Text ?? "", metinFontu, new Size(maksBalon - 2 * BalonIcBoslugu, int.MaxValue),
                                              TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        Size saat = TextRenderer.MeasureText(SaatYazisi(m), saatFontu);
        int ustSatir = baslik && !benim ? 18 : 0;
        if (m.Type == PacketType.Private) ustSatir = 18;

        int balonW = Math.Max(metin.Width, saat.Width + (m.Type == PacketType.Private ? 90 : 0)) + 2 * BalonIcBoslugu;
        balonW = Math.Min(Math.Max(balonW, 80), maksBalon);
        int balonH = ustSatir + metin.Height + saat.Height + 2 * BalonIcBoslugu - 2;

        int x = benim ? genislik - Bosluk - balonW : Bosluk + AvatarBoyu + 10;
        if (baslik) y += 4;
        var balon = new Rectangle(x, y, balonW, balonH);
        yerlesim.Add(new Yerlesim(m, balon, benim, baslik, balonH));
        toplamYukseklik = y + balonH + 6;
    }

    private static string SaatYazisi(Packet m) => (m.Time ?? DateTime.Now).ToString("HH:mm");

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (mesajlar.Count == 0)
        {
            TextRenderer.DrawText(g, "Henüz mesaj yok.\nBağlanınca mesajlar burada görünecek.", sistemFontu, ClientRectangle,
                Tema.YaziSoluk, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            return;
        }

        int kayma = AutoScrollPosition.Y;
        foreach (var y in yerlesim)
        {
            Rectangle b = y.Balon;
            b.Offset(0, kayma);
            if (b.Bottom < -40 || b.Top > Height + 40)
                continue;
            Ciz(g, y, b);
        }
    }

    private void Ciz(Graphics g, Yerlesim y, Rectangle b)
    {
        Packet m = y.Mesaj;

        if (m.Type is PacketType.System or PacketType.Error)
        {
            Color c = m.Type == PacketType.Error ? Tema.Hata : Tema.YaziSoluk;
            using (var yol = Tema.Yuvarlak(b, b.Height / 2f))
            using (var f = new SolidBrush(Color.FromArgb(m.Type == PacketType.Error ? 30 : 18, c)))
                g.FillPath(f, yol);
            TextRenderer.DrawText(g, m.Text, sistemFontu, b, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        bool ozel = m.Type == PacketType.Private;
        bool sunucu = m.From == "sunucu";

        if (!y.Benim && y.BaslikGoster)
            Tema.AvatarCiz(g, m.From, new Rectangle(Bosluk, b.Top, AvatarBoyu, AvatarBoyu), avatarFontu);

        Color dolgu = y.Benim ? (ozel ? Tema.Ozel : Tema.Vurgu) : Color.White;
        if (!y.Benim && ozel) dolgu = Color.FromArgb(243, 238, 252);
        if (sunucu) dolgu = Color.FromArgb(255, 248, 225);
        Color yazi = y.Benim ? Color.White : Tema.Yazi;

        using (var yol = BalonYolu(b, y.Benim, y.BaslikGoster))
        {
            using (var golge = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
            {
                g.TranslateTransform(0, 1.5f);
                g.FillPath(golge, yol);
                g.TranslateTransform(0, -1.5f);
            }
            using var f = new SolidBrush(dolgu);
            g.FillPath(f, yol);
        }

        int ix = b.X + BalonIcBoslugu;
        int iy = b.Y + BalonIcBoslugu - 2;

        if (ozel)
        {
            string etiket = y.Benim ? $"özel → {m.To}" : $"{m.From} · size özel";
            TextRenderer.DrawText(g, etiket, adFontu, new Point(ix, iy), y.Benim ? Color.FromArgb(230, 220, 250) : Tema.Ozel);
            iy += 18;
        }
        else if (!y.Benim && y.BaslikGoster)
        {
            TextRenderer.DrawText(g, sunucu ? "Sunucu duyurusu" : m.From, adFontu, new Point(ix, iy), Tema.KullaniciRengi(m.From));
            iy += 18;
        }

        var metinAlani = new Rectangle(ix, iy, b.Width - 2 * BalonIcBoslugu, b.Bottom - iy);
        TextRenderer.DrawText(g, m.Text, metinFontu, metinAlani, yazi, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

        string saat = SaatYazisi(m);
        Size s = TextRenderer.MeasureText(saat, saatFontu);
        TextRenderer.DrawText(g, saat, saatFontu, new Point(b.Right - BalonIcBoslugu - s.Width + 4, b.Bottom - s.Height - 4),
            y.Benim ? Color.FromArgb(210, 240, 236) : Tema.YaziSoluk);
    }

    /// <summary>Konuşan tarafa bakan köşesi sivri balon.</summary>
    private static GraphicsPath BalonYolu(Rectangle r, bool sag, bool kuyruk)
    {
        const float rad = 12f;
        float d = rad * 2;
        var p = new GraphicsPath();
        float ustSol = !sag && kuyruk ? 3 : d;
        float ustSag = sag && kuyruk ? 3 : d;

        p.AddArc(r.X, r.Y, ustSol, ustSol, 180, 90);
        p.AddArc(r.Right - ustSag, r.Y, ustSag, ustSag, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            metinFontu.Dispose();
            adFontu.Dispose();
            saatFontu.Dispose();
            avatarFontu.Dispose();
            sistemFontu.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>Avatarlı çevrimiçi kullanıcı listesi (owner-draw ListBox).</summary>
public class KullaniciListesi : ListBox
{
    private readonly Font adFontu = new("Segoe UI Semibold", 9.75F);
    private readonly Font kucukFont = new("Segoe UI", 8F);
    private readonly Font avatarFontu = new("Segoe UI Semibold", 8.5F);

    public KullaniciListesi()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 42;
        BorderStyle = BorderStyle.None;
        BackColor = Tema.Koyu;
        IntegralHeight = false;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? BenimAdim { get; set; }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool secili = (e.State & DrawItemState.Selected) != 0;
        using (var b = new SolidBrush(secili ? Tema.KoyuIkinci : BackColor))
            g.FillRectangle(b, e.Bounds);

        string ad = Items[e.Index]?.ToString() ?? "";
        var avatar = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 6, 30, 30);
        Tema.AvatarCiz(g, ad, avatar, avatarFontu);

        // çevrimiçi noktası
        using (var kenar = new SolidBrush(secili ? Tema.KoyuIkinci : BackColor))
            g.FillEllipse(kenar, avatar.Right - 10, avatar.Bottom - 10, 12, 12);
        using (var nokta = new SolidBrush(Color.FromArgb(52, 211, 153)))
            g.FillEllipse(nokta, avatar.Right - 8, avatar.Bottom - 8, 8, 8);

        TextRenderer.DrawText(g, ad, adFontu, new Point(avatar.Right + 10, e.Bounds.Y + 5), Color.White);
        string alt = ad == BenimAdim ? "siz" : "çift tıkla: özel mesaj";
        TextRenderer.DrawText(g, alt, kucukFont, new Point(avatar.Right + 10, e.Bounds.Y + 22), Color.FromArgb(130, 170, 172));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            adFontu.Dispose();
            kucukFont.Dispose();
            avatarFontu.Dispose();
        }
        base.Dispose(disposing);
    }
}
