using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ChatApp.Ui;

public enum ButonStili
{
    Dolu,       // teal dolgu
    Cerceve,    // koyu zemin üstünde çerçeveli
    Tehlike,    // kırmızı
    Acik        // beyaz zemin üstünde açık gri
}

/// <summary>Köşeleri yuvarlak, üzerine gelince açılan buton.</summary>
public class YuvarlakButon : Button
{
    private ButonStili stil = ButonStili.Dolu;
    private bool uzerinde, basili;

    public YuvarlakButon()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = new Font("Segoe UI Semibold", 9.75F);
        Cursor = Cursors.Hand;
    }

    [Category("Görünüm")]
    [DefaultValue(ButonStili.Dolu)]
    public ButonStili Stil
    {
        get => stil;
        set { stil = value; Invalidate(); }
    }

    protected override void OnMouseEnter(EventArgs e) { uzerinde = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { uzerinde = basili = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { basili = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { basili = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Tema.Zemin);

        Color dolgu, yazi, kenar;
        switch (stil)
        {
            case ButonStili.Cerceve:
                dolgu = Color.Transparent; yazi = Tema.YaziAcik; kenar = Tema.KoyuCizgi; break;
            case ButonStili.Tehlike:
                dolgu = Tema.Hata; yazi = Color.White; kenar = dolgu; break;
            case ButonStili.Acik:
                dolgu = Color.FromArgb(244, 247, 247); yazi = Tema.Yazi; kenar = Tema.Cizgi; break;
            default:
                dolgu = Tema.Vurgu; yazi = Color.White; kenar = dolgu; break;
        }

        if (!Enabled)
        {
            dolgu = stil == ButonStili.Cerceve ? Color.Transparent : Color.FromArgb(120, 150, 160, 162);
            yazi = stil == ButonStili.Cerceve ? Color.FromArgb(90, 120, 126) : Color.FromArgb(235, 240, 240);
            kenar = stil == ButonStili.Cerceve ? Color.FromArgb(32, 70, 76) : dolgu;
        }
        else if (basili || uzerinde)
        {
            float k = basili ? 0.82f : 0.92f;
            dolgu = dolgu == Color.Transparent ? Color.FromArgb(basili ? 50 : 30, 255, 255, 255)
                : Color.FromArgb(dolgu.A, (int)(dolgu.R * k), (int)(dolgu.G * k), (int)(dolgu.B * k));
        }

        var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        using (var yol = Tema.Yuvarlak(r, 8))
        {
            if (dolgu != Color.Transparent)
                using (var b = new SolidBrush(dolgu)) g.FillPath(b, yol);
            using (var p = new Pen(kenar)) g.DrawPath(p, yol);
        }

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, yazi,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

/// <summary>Başlıklı, beyaz, köşeleri yuvarlak kutu.</summary>
public class Kart : Panel
{
    private string baslik = "";
    private readonly Font baslikFontu = new("Segoe UI Semibold", 10.5F);

    public Kart()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Tema.Kart;
        Padding = new Padding(14, 42, 14, 14);
    }

    [Category("Görünüm")]
    [DefaultValue("")]
    public string Baslik
    {
        get => baslik;
        set { baslik = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Tema.Zemin);
        using (var yol = Tema.Yuvarlak(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10))
        using (var b = new SolidBrush(BackColor))
        using (var p = new Pen(Tema.Cizgi))
        {
            g.FillPath(b, yol);
            g.DrawPath(p, yol);
        }
        if (baslik.Length > 0)
            TextRenderer.DrawText(g, baslik, baslikFontu, new Point(14, 13), Tema.Yazi);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) baslikFontu.Dispose();
        base.Dispose(disposing);
    }
}

public enum Durum
{
    Kapali,
    Bekliyor,
    Acik
}

/// <summary>Yeşil/sarı/gri noktalı durum etiketi ("Bağlı", "Bağlanıyor..." gibi).</summary>
public class DurumRozeti : Control
{
    private Durum durum = Durum.Kapali;

    public DurumRozeti()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.Selectable, false);
        Font = new Font("Segoe UI Semibold", 9F);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Durum Durum
    {
        get => durum;
        set { durum = value; Invalidate(); }
    }

    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.White);

        Color renk = durum switch { Durum.Acik => Tema.Basari, Durum.Bekliyor => Tema.Uyari, _ => Tema.YaziSoluk };
        var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        using (var yol = Tema.Yuvarlak(r, r.Height / 2))
        using (var b = new SolidBrush(Color.FromArgb(28, renk)))
        using (var p = new Pen(Color.FromArgb(90, renk)))
        {
            g.FillPath(b, yol);
            g.DrawPath(p, yol);
        }
        using (var nokta = new SolidBrush(renk))
            g.FillEllipse(nokta, 12, Height / 2f - 4, 8, 8);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(24, 0, Width - 28, Height), renk,
            TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
    }
}

/// <summary>Büyük sayı + açıklama gösteren istatistik kutusu (sunucu ekranı).</summary>
public class IstatistikKutusu : Control
{
    private string deger = "0";
    private string aciklama = "";
    private Color renk = Tema.Vurgu;
    private readonly Font degerFontu = new("Segoe UI Semibold", 18F);
    private readonly Font aciklamaFontu = new("Segoe UI", 8.5F);

    public IstatistikKutusu()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.Selectable, false);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Deger { get => deger; set { deger = value; Invalidate(); } }

    [Category("Görünüm")]
    [DefaultValue("")]
    public string Aciklama { get => aciklama; set { aciklama = value; Invalidate(); } }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color Renk { get => renk; set { renk = value; Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Tema.Zemin);
        using (var yol = Tema.Yuvarlak(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10))
        using (var b = new SolidBrush(Color.White))
        using (var p = new Pen(Tema.Cizgi))
        {
            g.FillPath(b, yol);
            g.DrawPath(p, yol);
        }
        using (var serit = new SolidBrush(renk))
            g.FillRectangle(serit, 0, 12, 4, Height - 24);

        TextRenderer.DrawText(g, aciklama.ToUpper(new System.Globalization.CultureInfo("tr-TR")), aciklamaFontu, new Point(16, 12), Tema.YaziSoluk);
        TextRenderer.DrawText(g, deger, degerFontu, new Point(14, 30), Tema.Yazi);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { degerFontu.Dispose(); aciklamaFontu.Dispose(); }
        base.Dispose(disposing);
    }
}
