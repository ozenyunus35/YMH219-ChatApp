using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Networking;
using ChatApp.Core.Protocol;
using ChatApp.Ui;

namespace ChatApp.Server;

public partial class SunucuFormu : Form
{
    private readonly ChatServer sunucu;
    private readonly ToolTip ipucu = new();

    public SunucuFormu()
    {
        InitializeComponent();

        // Sunucu nesnesi form oluşturulduktan SONRA yaratılmalı: o anda WinForms'un
        // SynchronizationContext'i hazır oluyor ve sunucu olayları UI thread'ine aktarabiliyor.
        sunucu = new ChatServer();

        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
        catch (Exception) { /* varsayılan ikon */ }

        sunucu.ClientConnected += (_, info) => { IstemcileriGuncelle(); GunlugeYaz($"+ {info.UserName} bağlandı ({info.Address})", Tema.Basari); };
        sunucu.ClientDisconnected += (_, info) => { IstemcileriGuncelle(); GunlugeYaz($"- {info.UserName} ayrıldı", Tema.Uyari); };
        sunucu.MessageRelayed += Sunucu_MesajIletildi;
        // bağlanma/ayrılma ve duyurular zaten kendi olaylarıyla renkli yazılıyor, tekrar yazmayalım
        sunucu.Log += (_, metin) =>
        {
            if (!metin.Contains("bağlandı") && !metin.Contains("ayrıldı") && !metin.StartsWith("Duyuru:"))
                GunlugeYaz(metin, Tema.YaziSoluk);
        };
        sunucu.StateChanged += (_, _) => ArayuzuGuncelle();

        kutuDurum.Renk = Tema.YaziSoluk;
        kutuKullanici.Renk = Tema.Vurgu;
        kutuMesaj.Renk = Color.FromArgb(37, 99, 235);
        kutuSure.Renk = Tema.Ozel;
        lblAdresler.Text = "Bu bilgisayarın adresleri: " + string.Join(", ", YerelAdresler());

        // Tablo her saniye güncellenirken titremesin
        typeof(ListView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(lvIstemciler, true);
        lvIstemciler.Resize += (_, _) => SutunlariAyarla();
        lvIstemciler.KeyDown += (_, e) => { if (e.KeyCode == Keys.Delete && btnCikar.Enabled) btnCikar.PerformClick(); };
        txtPort.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !sunucu.IsRunning)
            {
                e.SuppressKeyPress = true;
                Baslat();
            }
        };
        ipucu.SetToolTip(btnCikar, "Seçili kullanıcıyı sunucudan çıkar (Delete)");
        ipucu.SetToolTip(txtPort, "1024-65535 arası (Enter ile başlat)");
        ipucu.SetToolTip(btnDuyuru, "Tüm kullanıcılara duyuru gönder (Enter)");
        SutunlariAyarla();

        ArayuzuGuncelle();
        GunlugeYaz("Sunucu hazır. Başlatmak için \"Sunucuyu başlat\" butonuna basın.", Tema.YaziSoluk);
        saniyeZamanlayici.Start();
    }

    // ===================== Başlat / durdur =====================

    private void btnBaslat_Click(object? sender, EventArgs e) => Baslat();

    internal bool Baslat()
    {
        lblHata.Text = "";
        if (!ChatRules.TryParsePort(txtPort.Text, out int port))
        {
            HataGoster("Port 1024 ile 65535 arasında bir sayı olmalı.");
            return false;
        }

        try
        {
            sunucu.Start(port);
            lblHata.Text = "";
            lblAdresler.Text = "İstemciler şu adreslerden bağlanabilir: " +
                               string.Join(", ", YerelAdresler().Select(a => $"{a}:{sunucu.Port}"));
            return true;
        }
        catch (ChatException ex)
        {
            HataGoster(ex.Message);
            return false;
        }
    }

    private void btnDurdur_Click(object? sender, EventArgs e) => Durdur(onayIste: true);

    /// <summary>Bağlı kullanıcı varsa önce sorulur (yanlışlıkla herkesin bağlantısı kopmasın).</summary>
    internal bool Durdur(bool onayIste)
    {
        if (onayIste && !KapatmaOnayi("Sunucu durdurulsun mu?"))
            return false;
        sunucu.Stop();
        IstemcileriGuncelle();
        return true;
    }

    private bool KapatmaOnayi(string soru)
    {
        int n = sunucu.IsRunning ? sunucu.GetClients().Count : 0;
        if (n == 0)
            return true;
        return MessageBox.Show(this, $"{n} kullanıcı bağlı, bağlantıları kesilecek.\n{soru}", "Sunucu",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private void SutunlariAyarla()
    {
        // "Adres" sütunu kalan genişliği doldursun (boşluk ya da kesik adres kalmasın)
        int diger = 0;
        for (int i = 0; i < lvIstemciler.Columns.Count; i++)
            if (i != 1) diger += lvIstemciler.Columns[i].Width;
        int kalan = lvIstemciler.ClientSize.Width - diger - 4;
        if (lvIstemciler.Columns.Count > 1 && kalan > 80)
            lvIstemciler.Columns[1].Width = kalan;
    }

    private void HataGoster(string mesaj)
    {
        lblHata.Text = "⚠ " + mesaj;
        GunlugeYaz("HATA: " + mesaj, Tema.Hata);
    }

    // ===================== Mesajlar =====================

    private void Sunucu_MesajIletildi(object? sender, Packet p)
    {
        // Özel mesajların içeriğini sunucu ekranında göstermiyoruz, sadece kimden kime gittiği
        if (p.Type == PacketType.Private)
            GunlugeYaz($"{p.From} → {p.To} (özel mesaj)", Tema.Ozel);
        else if (p.From == "sunucu")
            GunlugeYaz($"[duyuru] {p.Text}", Tema.VurguKoyu);
        else
            GunlugeYaz($"{p.From}: {p.Text}", Tema.Yazi);

        kutuMesaj.Deger = sunucu.TotalMessages.ToString();
    }

    private void btnDuyuru_Click(object? sender, EventArgs e) => DuyuruGonder();

    private void txtDuyuru_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            DuyuruGonder();
        }
    }

    internal void DuyuruGonder()
    {
        if (!sunucu.IsRunning)
            return;
        try
        {
            sunucu.Announce(txtDuyuru.Text);
            txtDuyuru.Clear();
            lblHata.Text = "";
            txtDuyuru.Focus();
        }
        catch (ArgumentException ex)
        {
            HataGoster(ex.Message);
        }
    }

    private void btnCikar_Click(object? sender, EventArgs e)
    {
        if (lvIstemciler.SelectedItems.Count == 0)
            return;

        string ad = lvIstemciler.SelectedItems[0].Text;
        var cevap = MessageBox.Show(this, $"\"{ad}\" sunucudan çıkarılsın mı?", "Kullanıcıyı çıkar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (cevap != DialogResult.Yes)
            return;
        if (sunucu.Kick(ad))
            GunlugeYaz($"{ad} sunucudan çıkarıldı.", Tema.Hata);
        else
            GunlugeYaz($"{ad} zaten ayrılmış.", Tema.YaziSoluk);
    }

    private void lvIstemciler_SelectedIndexChanged(object? sender, EventArgs e)
    {
        btnCikar.Enabled = lvIstemciler.SelectedItems.Count > 0;
    }

    // ===================== Ekran =====================

    private void IstemcileriGuncelle()
    {
        // Satırlar yerinde güncelleniyor: her saniye silip yeniden eklemek titreme, kaydırma sıçraması
        // ve "Çıkar"a basmadan önce seçimin kaybolması yapıyordu
        var istemciler = sunucu.GetClients();
        var adlar = istemciler.Select(c => c.UserName).ToHashSet();
        lvIstemciler.BeginUpdate();
        foreach (ListViewItem eski in lvIstemciler.Items.Cast<ListViewItem>().Where(i => !adlar.Contains(i.Text)).ToList())
            lvIstemciler.Items.Remove(eski);
        foreach (ClientInfo c in istemciler)
        {
            var item = lvIstemciler.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == c.UserName);
            if (item is null)
            {
                item = new ListViewItem(c.UserName);
                item.SubItems.Add(c.Address);
                item.SubItems.Add(c.ConnectedAt.ToString("HH:mm:ss"));
                item.SubItems.Add(c.MessageCount.ToString());
                item.ForeColor = Tema.KullaniciRengi(c.UserName);
                lvIstemciler.Items.Add(item);
            }
            else if (item.SubItems[3].Text != c.MessageCount.ToString())
            {
                item.SubItems[3].Text = c.MessageCount.ToString();
            }
        }
        lvIstemciler.EndUpdate();

        kutuKullanici.Deger = sunucu.GetClients().Count.ToString();
        btnCikar.Enabled = lvIstemciler.SelectedItems.Count > 0;
    }

    private void ArayuzuGuncelle()
    {
        bool calisiyor = sunucu.IsRunning;
        btnBaslat.Enabled = !calisiyor;
        btnDurdur.Enabled = calisiyor;
        txtPort.ReadOnly = calisiyor;
        txtPort.ForeColor = calisiyor ? Color.FromArgb(130, 170, 172) : Color.White;
        txtPort.TabStop = !calisiyor;
        btnDuyuru.Enabled = calisiyor;
        txtDuyuru.ReadOnly = !calisiyor;
        txtDuyuru.PlaceholderText = calisiyor ? "Tüm kullanıcılara duyuru yazın ve Enter'a basın" : "Duyuru yazmak için önce sunucuyu başlatın";

        kutuDurum.Deger = calisiyor ? $"Açık · {sunucu.Port}" : "Kapalı";
        kutuDurum.Renk = calisiyor ? Tema.Basari : Tema.YaziSoluk;
        Text = calisiyor ? $"Sohbet Sunucusu - port {sunucu.Port}" : "Sohbet Sunucusu - YMH219";

        if (!calisiyor)
            lblAdresler.Text = "Bu bilgisayarın adresleri: " + string.Join(", ", YerelAdresler());

        IstemcileriGuncelle();
        SureyiGuncelle();
    }

    private void saniyeZamanlayici_Tick(object? sender, EventArgs e)
    {
        SureyiGuncelle();
        if (sunucu.IsRunning)
            IstemcileriGuncelle();     // mesaj sayıları güncellensin
    }

    private void SureyiGuncelle()
    {
        if (sunucu.IsRunning && sunucu.StartedAt is DateTime bas)
        {
            var t = DateTime.Now - bas;
            kutuSure.Deger = $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";   // 24 saati geçince sıfırlanmasın
        }
        else
            kutuSure.Deger = "-";
        kutuMesaj.Deger = sunucu.TotalMessages.ToString();
    }

    private void GunlugeYaz(string metin, Color renk)
    {
        // Kullanıcı yukarıdaki eski kayıtları okurken günlük aşağı zıplamasın
        int sonGorunen = rtbGunluk.GetLineFromCharIndex(rtbGunluk.GetCharIndexFromPosition(new Point(1, rtbGunluk.ClientSize.Height - 1)));
        bool enAltta = sonGorunen >= rtbGunluk.Lines.Length - 2;

        // Çok uzun çalışmada günlük sınırsız büyümesin: en eski satırlar siliniyor
        if (rtbGunluk.Lines.Length > 5000)
        {
            rtbGunluk.Select(0, rtbGunluk.GetFirstCharIndexFromLine(1000));
            rtbGunluk.ReadOnly = false;
            rtbGunluk.SelectedText = "";
            rtbGunluk.ReadOnly = true;
        }

        rtbGunluk.SelectionStart = rtbGunluk.TextLength;
        rtbGunluk.SelectionLength = 0;
        rtbGunluk.SelectionColor = Tema.YaziSoluk;
        rtbGunluk.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  ");
        rtbGunluk.SelectionColor = renk;
        rtbGunluk.AppendText(metin + Environment.NewLine);
        if (enAltta)
            rtbGunluk.ScrollToCaret();
    }

    private static IEnumerable<string> YerelAdresler()
    {
        var liste = new List<string>();
        try
        {
            liste.AddRange(Dns.GetHostAddresses(Dns.GetHostName())
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                .Select(a => a.ToString()));
        }
        catch (SocketException)
        {
            // ağ yoksa sadece localhost
        }
        liste.Add("127.0.0.1");
        return liste.Distinct();
    }

    // ===================== Günlük kaydetme =====================

    private void btnGunlukKaydet_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Sunucu günlüğünü kaydet",
            Filter = "Metin dosyası (*.txt)|*.txt",
            FileName = $"sunucu-gunlugu-{DateTime.Now:yyyyMMdd-HHmm}.txt"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            GunluguKaydet(dialog.FileName);
    }

    internal bool GunluguKaydet(string yol)
    {
        try
        {
            File.WriteAllText(yol, rtbGunluk.Text);
            GunlugeYaz($"Günlük kaydedildi: {Path.GetFileName(yol)}", Tema.Basari);
            lblHata.Text = "";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            HataGoster("Günlük kaydedilemedi: " + ex.Message);
            return false;
        }
    }

    private void SunucuFormu_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !KapatmaOnayi("Sunucu kapatılsın mı?"))
        {
            e.Cancel = true;
            return;
        }
        ipucu.Dispose();
        sunucu.Dispose();
    }
}
