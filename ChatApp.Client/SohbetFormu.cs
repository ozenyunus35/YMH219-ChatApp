using System.Drawing.Drawing2D;
using ChatApp.Core.History;
using ChatApp.Core.Networking;
using ChatApp.Core.Protocol;
using ChatApp.Ui;

namespace ChatApp.Client;

public partial class SohbetFormu : Form
{
    private readonly ChatClient istemci;
    private readonly ChatHistory gecmis = new();
    private readonly Dictionary<string, DateTime> yazanlar = new();
    private string? ozelAlici;
    private DateTime sonYaziyorBildirimi = DateTime.MinValue;

    public SohbetFormu()
    {
        InitializeComponent();

        // InitializeComponent'ten sonra oluşturuluyor ki UI thread'inin SynchronizationContext'ini yakalasın
        istemci = new ChatClient();

        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
        catch (Exception) { /* varsayılan ikon */ }

        istemci.PacketReceived += Istemci_PaketGeldi;
        istemci.UsersChanged += (_, _) => KullanicilariGuncelle();
        istemci.StateChanged += (_, _) => ArayuzuGuncelle();
        istemci.ConnectionLost += Istemci_BaglantiKoptu;

        yaziyorZamanlayici.Interval = 1000;
        txtMesaj.GotFocus += (_, _) => pnlGiris.Invalidate();
        txtMesaj.LostFocus += (_, _) => pnlGiris.Invalidate();
        ipucu.SetToolTip(lstKullanicilar, "Bir kullanıcıya çift tıklayarak (veya seçip Enter) özel mesaj gönderebilirsiniz");
        ipucu.SetToolTip(btnGecmisKaydet, "Ekrandaki mesajları .txt dosyasına kaydet");
        ipucu.SetToolTip(btnGonder, "Gönder (Enter)");
        ipucu.SetToolTip(btnOzelIptal, "Genel sohbete dön (Esc)");
        ipucu.SetToolTip(txtKullanici, "2-20 karakter: harf, rakam, _ - .");
        ipucu.SetToolTip(btnBaglan, "Sunucuya bağlan (Enter)");

        // Sunucu ve port kutularında da Enter bağlansın
        txtSunucu.KeyDown += txtKullanici_KeyDown;
        txtPort.KeyDown += txtKullanici_KeyDown;

        // Çift tıklama boş alana denk gelirse özel mod açılmasın; klavye ve sağ tık da çalışsın
        lstKullanicilar.DoubleClick -= lstKullanicilar_DoubleClick;
        lstKullanicilar.MouseDoubleClick += (_, e) =>
        {
            int i = lstKullanicilar.IndexFromPoint(e.Location);
            if (i >= 0 && lstKullanicilar.Items[i] is string ad) OzelModuDene(ad);
        };
        lstKullanicilar.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && lstKullanicilar.SelectedItem is string ad)
            {
                e.SuppressKeyPress = true;
                OzelModuDene(ad);
            }
        };
        var kisiMenu = new ContextMenuStrip();
        var ozelMenu = new ToolStripMenuItem("Özel mesaj gönder");
        ozelMenu.Click += (_, _) => { if (lstKullanicilar.SelectedItem is string ad) OzelModuDene(ad); };
        kisiMenu.Items.Add(ozelMenu);
        kisiMenu.Opening += (_, e) =>
        {
            var p = lstKullanicilar.PointToClient(Cursor.Position);
            int i = lstKullanicilar.IndexFromPoint(p);
            if (i >= 0) lstKullanicilar.SelectedIndex = i;
            e.Cancel = !(lstKullanicilar.SelectedItem is string ad && ad != istemci.UserName && istemci.State == ConnectionState.Connected);
        };
        lstKullanicilar.ContextMenuStrip = kisiMenu;

        // Özel mod çipi ad uzunluğuna göre büyüsün (uzun adda ✕ kesiliyordu)
        btnOzelIptal.AutoSize = true;
        btnOzelIptal.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnOzelIptal.Padding = new Padding(6, 0, 6, 0);

        // Bağlı değilken mesaj kutusu gri değil, salt okunur (koyu/açık temada gri blok görünmesin)
        txtMesaj.BackColor = Color.White;
        // Odak mesaj kutusundayken fare tekerleği sohbet geçmişini kaydırsın
        txtMesaj.MouseWheel += (_, e) => mesajListesi.Kaydir(e.Delta);

        lblUyari.AutoEllipsis = true;
        uyariZamanlayici.Tick += (_, _) => { uyariZamanlayici.Stop(); UyariGizle(); };
        pnlGiris.Resize += (_, _) => GirisDuzeniniAyarla();

        ArayuzuGuncelle();
    }

    /// <summary>Komut satırından ön doldurma: ChatApp.Client.exe [kullanıcı] [sunucu] [port]</summary>
    public SohbetFormu(string[] args) : this()
    {
        if (args.Length > 0) txtKullanici.Text = args[0];
        if (args.Length > 1) txtSunucu.Text = args[1];
        if (args.Length > 2) txtPort.Text = args[2];
    }

    private readonly System.Windows.Forms.Timer uyariZamanlayici = new() { Interval = 5000 };
    private bool gonderiyor;

    private void OzelModuDene(string ad)
    {
        if (ad != istemci.UserName && istemci.State == ConnectionState.Connected)
            OzelModuAc(ad);
    }

    private void SohbetFormu_Shown(object? sender, EventArgs e)
    {
        txtKullanici.Focus();
    }

    // ===================== Bağlantı =====================

    private async void btnBaglan_Click(object? sender, EventArgs e)
    {
        await BaglanAsync();
    }

    private async void txtKullanici_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await BaglanAsync();
        }
    }

    internal async Task BaglanAsync()
    {
        if (istemci.State != ConnectionState.Disconnected)
            return;

        if (!ChatRules.TryParsePort(txtPort.Text, out int port))
        {
            Bildir("Port 1024 ile 65535 arasında bir sayı olmalı.", hata: true);
            txtPort.Focus();
            return;
        }

        if (ChatRules.ValidateUserName(txtKullanici.Text) is string adHatasi)
        {
            Bildir(adHatasi, hata: true);
            txtKullanici.Focus();
            return;
        }

        UyariGizle();
        try
        {
            await istemci.ConnectAsync(txtSunucu.Text, port, txtKullanici.Text.Trim());

            mesajListesi.BenimAdim = istemci.UserName;
            lstKullanicilar.BenimAdim = istemci.UserName;
            EkranaEkle(Packet.SystemNotice($"{istemci.ServerAddress} sunucusuna \"{istemci.UserName}\" olarak bağlandınız."));
        }
        catch (ChatException ex)
        {
            // Bağlantı hatası programı kapatmıyor, kullanıcıya gösteriliyor
            Bildir(ex.Message, hata: true);
        }
        finally
        {
            ArayuzuGuncelle();
        }

        // Odak, arayüz güncellendikten sonra veriliyor (önce verilince kutu henüz salt okunurdu)
        if (istemci.State == ConnectionState.Connected)
            txtMesaj.Focus();
    }

    private async void btnKes_Click(object? sender, EventArgs e)
    {
        await istemci.DisconnectAsync();
        EkranaEkle(Packet.SystemNotice("Bağlantıyı kestiniz."));
        OzelModuKapat();
        ArayuzuGuncelle();
        txtKullanici.Focus();
    }

    private void Istemci_BaglantiKoptu(object? sender, string sebep)
    {
        EkranaEkle(Packet.Fail(sebep));
        Bildir(sebep + "  Tekrar bağlanmak için \"Bağlan\" butonunu kullanın.", hata: true);
        OzelModuKapat();
        ArayuzuGuncelle();
        txtKullanici.Focus();
    }

    // ===================== Mesajlar =====================

    private void Istemci_PaketGeldi(object? sender, Packet paket)
    {
        if (paket.Type == PacketType.Typing)
        {
            if (paket.From != null)
                yazanlar[paket.From] = DateTime.Now;
            YaziyorGoster();
            return;
        }

        if (paket.From != null && yazanlar.Remove(paket.From))
            YaziyorGoster();

        EkranaEkle(paket);

        if (paket.Type == PacketType.Error)
            Bildir(paket.Text ?? "Sunucudan hata geldi.", hata: true);
    }

    private void EkranaEkle(Packet paket)
    {
        mesajListesi.Ekle(paket);
        gecmis.Add(paket);
    }

    private async void btnGonder_Click(object? sender, EventArgs e)
    {
        await GonderAsync();
    }

    private async void txtMesaj_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;      // "ding" sesini engelle
            await GonderAsync();
        }
        else if (e.KeyCode == Keys.Escape && ozelAlici != null)
        {
            OzelModuKapat();
        }
    }

    internal async Task GonderAsync()
    {
        // Enter basılı tutulunca / Gönder'e çift tıklanınca aynı mesaj iki kez gitmesin
        if (gonderiyor)
            return;
        string yazi = txtMesaj.Text.Trim();
        if (yazi.Length == 0 || istemci.State != ConnectionState.Connected)
            return;

        gonderiyor = true;
        try
        {
            if (ChatRules.TryParseWhisper(yazi, out string alici, out string ozelMetin))
                await istemci.SendPrivateAsync(alici, ozelMetin);
            else if (ozelAlici != null)
                await istemci.SendPrivateAsync(ozelAlici, yazi);
            else
                await istemci.SendMessageAsync(yazi);

            txtMesaj.Clear();
            UyariGizle();
        }
        catch (Exception ex) when (ex is ArgumentException or ChatException)
        {
            Bildir(ex.Message, hata: true);
        }
        finally
        {
            gonderiyor = false;
        }

        txtMesaj.Focus();
    }

    private async void txtMesaj_TextChanged(object? sender, EventArgs e)
    {
        // "yazıyor..." bilgisini en fazla 2 saniyede bir gönder
        if (istemci.State != ConnectionState.Connected || txtMesaj.TextLength == 0)
            return;
        // Özel mesaj yazarken herkese "yazıyor..." gitmesin
        if (ozelAlici != null || txtMesaj.Text.StartsWith("/w", StringComparison.OrdinalIgnoreCase))
            return;
        if (DateTime.Now - sonYaziyorBildirimi < TimeSpan.FromSeconds(2))
            return;

        sonYaziyorBildirimi = DateTime.Now;
        try
        {
            await istemci.SendTypingAsync();
        }
        catch (ChatException)
        {
            // önemli değil, bağlantı kopmuşsa zaten ayrıca bildiriliyor
        }
    }

    private void yaziyorZamanlayici_Tick(object? sender, EventArgs e)
    {
        foreach (var ad in yazanlar.Where(y => DateTime.Now - y.Value > TimeSpan.FromSeconds(3)).Select(y => y.Key).ToList())
            yazanlar.Remove(ad);
        YaziyorGoster();
    }

    private void YaziyorGoster()
    {
        lblYaziyor.Text = yazanlar.Count switch
        {
            0 => "",
            1 => $"{yazanlar.Keys.First()} yazıyor...",
            _ => $"{string.Join(", ", yazanlar.Keys)} yazıyor..."
        };
        yaziyorZamanlayici.Enabled = yazanlar.Count > 0;
    }

    // ===================== Özel mesaj =====================

    private void lstKullanicilar_DoubleClick(object? sender, EventArgs e)
    {
        if (lstKullanicilar.SelectedItem is string ad && ad != istemci.UserName && istemci.State == ConnectionState.Connected)
            OzelModuAc(ad);
    }

    internal void OzelModuAc(string ad)
    {
        ozelAlici = ad;
        btnOzelIptal.Text = $"Özel: {ad}  ✕";
        btnOzelIptal.Visible = true;
        btnGonder.Text = "Özel gönder";
        txtMesaj.PlaceholderText = $"{ad} kullanıcısına özel mesaj...  (Esc: iptal)";
        GirisDuzeniniAyarla();
        txtMesaj.Focus();
    }

    private void btnOzelIptal_Click(object? sender, EventArgs e) => OzelModuKapat();

    private void OzelModuKapat()
    {
        ozelAlici = null;
        btnOzelIptal.Visible = false;
        btnGonder.Text = "Gönder";
        txtMesaj.PlaceholderText = "Mesaj yazın...  (/w kullanıcı mesaj = özel mesaj)";
        GirisDuzeniniAyarla();
    }

    private void GirisDuzeniniAyarla()
    {
        if (btnOzelIptal.Visible)
        {
            btnOzelIptal.PerformLayout();
            btnOzelIptal.Left = btnGonder.Left - btnOzelIptal.Width - 8;
        }
        int sag = btnOzelIptal.Visible ? btnOzelIptal.Left : btnGonder.Left;
        txtMesaj.Width = Math.Max(80, sag - txtMesaj.Left - 10);
    }

    // ===================== Ekran güncelleme =====================

    private void KullanicilariGuncelle()
    {
        lstKullanicilar.BeginUpdate();
        lstKullanicilar.Items.Clear();
        foreach (string ad in istemci.OnlineUsers)
            lstKullanicilar.Items.Add(ad);
        lstKullanicilar.EndUpdate();

        lblCevrimici.Text = $"ÇEVRİMİÇİ — {istemci.OnlineUsers.Count}";
        if (ozelAlici != null && !istemci.OnlineUsers.Contains(ozelAlici))
            OzelModuKapat();
        ArayuzuGuncelle();
    }

    private void ArayuzuGuncelle()
    {
        var durum = istemci.State;
        bool bagli = durum == ConnectionState.Connected;
        bool kapali = durum == ConnectionState.Disconnected;

        // Enabled=false koyu temada gri kutu gösteriyor, o yüzden ReadOnly + soluk yazı kullanıyoruz
        foreach (TextBox kutu in new[] { txtSunucu, txtPort, txtKullanici })
        {
            kutu.ReadOnly = !kapali;
            kutu.TabStop = kapali;
            kutu.ForeColor = kapali ? Color.White : Color.FromArgb(130, 170, 172);
        }
        btnBaglan.Enabled = kapali;
        btnBaglan.Text = durum == ConnectionState.Connecting ? "Bağlanıyor..." : "Bağlan";
        btnKes.Enabled = bagli;
        txtMesaj.ReadOnly = !bagli;
        txtMesaj.TabStop = bagli;
        btnGonder.Enabled = bagli;
        if (!bagli)
        {
            txtMesaj.Clear();
            txtMesaj.PlaceholderText = "Bağlanınca mesaj yazabilirsiniz";
        }
        else if (ozelAlici is null && txtMesaj.PlaceholderText.StartsWith("Bağlanınca"))
            txtMesaj.PlaceholderText = "Mesaj yazın...  (/w kullanıcı mesaj = özel mesaj)";

        durumRozeti.Durum = bagli ? Durum.Acik : kapali ? Durum.Kapali : Durum.Bekliyor;
        durumRozeti.Text = bagli ? "Bağlı" : kapali ? "Bağlı değil" : "Bağlanıyor...";

        lblOdaBilgi.Text = bagli
            ? $"{istemci.OnlineUsers.Count} kişi çevrimiçi  ·  {istemci.ServerAddress}  ·  {istemci.UserName} olarak"
            : "Bağlanmak için soldaki formu doldurun";
        Text = bagli ? $"Sohbet - {istemci.UserName} @ {istemci.ServerAddress}" : "Sohbet - YMH219";

        if (kapali)
        {
            lstKullanicilar.Items.Clear();
            lblCevrimici.Text = "ÇEVRİMİÇİ — 0";
        }
    }

    private void Bildir(string mesaj, bool hata)
    {
        lblUyari.Text = (hata ? "⚠  " : "✓  ") + mesaj;
        ipucu.SetToolTip(lblUyari, mesaj);      // uzun mesaj kesilirse tamamı ipucunda
        pnlUyari.BackColor = hata ? Color.FromArgb(253, 232, 235) : Color.FromArgb(225, 245, 234);
        lblUyari.ForeColor = hata ? Color.FromArgb(160, 30, 50) : Color.FromArgb(20, 110, 60);
        pnlUyari.Visible = true;
        // Başarı mesajları birkaç saniye sonra kendiliğinden kapanıyor
        uyariZamanlayici.Stop();
        if (!hata)
            uyariZamanlayici.Start();
    }

    private void UyariGizle() => pnlUyari.Visible = false;

    private void btnUyariKapat_Click(object? sender, EventArgs e) => UyariGizle();

    // ===================== Geçmiş =====================

    private void btnGecmisKaydet_Click(object? sender, EventArgs e)
    {
        if (gecmis.Items.Count == 0)
        {
            Bildir("Kaydedilecek mesaj yok.", hata: true);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Sohbet geçmişini kaydet",
            Filter = "Metin dosyası (*.txt)|*.txt",
            FileName = ChatHistory.SuggestFileName(istemci.UserName ?? "sohbet", DateTime.Now)
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            GecmisiKaydet(dialog.FileName);
    }

    internal bool GecmisiKaydet(string yol)
    {
        try
        {
            gecmis.Save(yol, $"Kullanıcı: {istemci.UserName ?? "-"}  ·  Sunucu: {istemci.ServerAddress ?? "-"}");
            Bildir($"{gecmis.Items.Count} mesaj kaydedildi: {Path.GetFileName(yol)}", hata: false);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            Bildir("Geçmiş kaydedilemedi: " + ex.Message, hata: true);
            return false;
        }
    }

    // ===================== Çizim =====================

    private void pnlUst_Paint(object? sender, PaintEventArgs e)
    {
        using var p = new Pen(Tema.Cizgi);
        e.Graphics.DrawLine(p, 0, pnlUst.Height - 1, pnlUst.Width, pnlUst.Height - 1);
    }

    private void pnlGiris_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(pnlAlt.BackColor);
        using var yol = Tema.Yuvarlak(new RectangleF(0.5f, 0.5f, pnlGiris.Width - 1.5f, pnlGiris.Height - 1.5f), 12);
        using var b = new SolidBrush(Color.White);
        using var k = new Pen(txtMesaj.Focused ? Tema.Vurgu : Tema.Cizgi);
        g.FillPath(b, yol);
        g.DrawPath(k, yol);
    }

    private void SohbetFormu_FormClosing(object? sender, FormClosingEventArgs e)
    {
        uyariZamanlayici.Dispose();
        istemci.Dispose();      // soket kapanınca sunucu ayrıldığımızı anlıyor
    }
}
