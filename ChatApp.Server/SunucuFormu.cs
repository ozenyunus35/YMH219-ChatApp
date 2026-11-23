using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Networking;
using ChatApp.Core.Protocol;
using ChatApp.Ui;

namespace ChatApp.Server;

public partial class SunucuFormu : Form
{
    private readonly ChatServer sunucu;

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

    private void btnDurdur_Click(object? sender, EventArgs e)
    {
        sunucu.Stop();
        IstemcileriGuncelle();
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
        if (cevap == DialogResult.Yes && sunucu.Kick(ad))
            GunlugeYaz($"{ad} sunucudan çıkarıldı.", Tema.Hata);
    }

    private void lvIstemciler_SelectedIndexChanged(object? sender, EventArgs e)
    {
        btnCikar.Enabled = lvIstemciler.SelectedItems.Count > 0;
    }

    // ===================== Ekran =====================

    private void IstemcileriGuncelle()
    {
        string? secili = lvIstemciler.SelectedItems.Count > 0 ? lvIstemciler.SelectedItems[0].Text : null;

        lvIstemciler.BeginUpdate();
        lvIstemciler.Items.Clear();
        foreach (ClientInfo c in sunucu.GetClients())
        {
            var item = new ListViewItem(c.UserName);
            item.SubItems.Add(c.Address);
            item.SubItems.Add(c.ConnectedAt.ToString("HH:mm:ss"));
            item.SubItems.Add(c.MessageCount.ToString());
            item.ForeColor = Tema.KullaniciRengi(c.UserName);
            item.Selected = c.UserName == secili;
            lvIstemciler.Items.Add(item);
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
        btnDuyuru.Enabled = calisiyor;

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
            kutuSure.Deger = (DateTime.Now - bas).ToString(@"hh\:mm\:ss");
        else
            kutuSure.Deger = "-";
        kutuMesaj.Deger = sunucu.TotalMessages.ToString();
    }

    private void GunlugeYaz(string metin, Color renk)
    {
        rtbGunluk.SelectionStart = rtbGunluk.TextLength;
        rtbGunluk.SelectionColor = Tema.YaziSoluk;
        rtbGunluk.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  ");
        rtbGunluk.SelectionColor = renk;
        rtbGunluk.AppendText(metin + Environment.NewLine);
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
        sunucu.Dispose();
    }
}
