namespace ChatApp.Server;

partial class SunucuFormu
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlUst = new Panel();
        btnDurdur = new ChatApp.Ui.YuvarlakButon();
        btnBaslat = new ChatApp.Ui.YuvarlakButon();
        txtPort = new TextBox();
        lblPort = new Label();
        lblAltBaslik = new Label();
        lblBaslik = new Label();
        pnlIstatistik = new TableLayoutPanel();
        kutuDurum = new ChatApp.Ui.IstatistikKutusu();
        kutuKullanici = new ChatApp.Ui.IstatistikKutusu();
        kutuMesaj = new ChatApp.Ui.IstatistikKutusu();
        kutuSure = new ChatApp.Ui.IstatistikKutusu();
        tblGovde = new TableLayoutPanel();
        kartIstemciler = new ChatApp.Ui.Kart();
        lvIstemciler = new ListView();
        colKullanici = new ColumnHeader();
        colAdres = new ColumnHeader();
        colBaglandi = new ColumnHeader();
        colMesaj = new ColumnHeader();
        pnlKartAlt = new Panel();
        btnCikar = new ChatApp.Ui.YuvarlakButon();
        kartGunluk = new ChatApp.Ui.Kart();
        rtbGunluk = new RichTextBox();
        pnlAlt = new Panel();
        btnGunlukKaydet = new ChatApp.Ui.YuvarlakButon();
        btnDuyuru = new ChatApp.Ui.YuvarlakButon();
        txtDuyuru = new TextBox();
        durumCubugu = new StatusStrip();
        lblAdresler = new ToolStripStatusLabel();
        lblHata = new ToolStripStatusLabel();
        saniyeZamanlayici = new System.Windows.Forms.Timer(components);
        pnlUst.SuspendLayout();
        pnlIstatistik.SuspendLayout();
        tblGovde.SuspendLayout();
        kartIstemciler.SuspendLayout();
        pnlKartAlt.SuspendLayout();
        kartGunluk.SuspendLayout();
        pnlAlt.SuspendLayout();
        durumCubugu.SuspendLayout();
        SuspendLayout();
        //
        // pnlUst
        //
        pnlUst.BackColor = Color.FromArgb(14, 42, 47);
        pnlUst.Controls.Add(btnDurdur);
        pnlUst.Controls.Add(btnBaslat);
        pnlUst.Controls.Add(txtPort);
        pnlUst.Controls.Add(lblPort);
        pnlUst.Controls.Add(lblAltBaslik);
        pnlUst.Controls.Add(lblBaslik);
        pnlUst.Dock = DockStyle.Top;
        pnlUst.Location = new Point(0, 0);
        pnlUst.Name = "pnlUst";
        pnlUst.Size = new Size(980, 76);
        pnlUst.TabIndex = 0;
        //
        // btnDurdur
        //
        btnDurdur.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDurdur.Enabled = false;
        btnDurdur.Location = new Point(840, 20);
        btnDurdur.Name = "btnDurdur";
        btnDurdur.Size = new Size(124, 36);
        btnDurdur.Stil = ChatApp.Ui.ButonStili.Tehlike;
        btnDurdur.TabIndex = 4;
        btnDurdur.Text = "Durdur";
        btnDurdur.Click += btnDurdur_Click;
        //
        // btnBaslat
        //
        btnBaslat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBaslat.Location = new Point(706, 20);
        btnBaslat.Name = "btnBaslat";
        btnBaslat.Size = new Size(124, 36);
        btnBaslat.TabIndex = 3;
        btnBaslat.Text = "Sunucuyu başlat";
        btnBaslat.Click += btnBaslat_Click;
        //
        // txtPort
        //
        txtPort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtPort.BackColor = Color.FromArgb(22, 60, 66);
        txtPort.BorderStyle = BorderStyle.FixedSingle;
        txtPort.Font = new Font("Segoe UI", 11F);
        txtPort.ForeColor = Color.White;
        txtPort.Location = new Point(626, 24);
        txtPort.MaxLength = 5;
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(70, 27);
        txtPort.TabIndex = 2;
        txtPort.Text = "5050";
        txtPort.TextAlign = HorizontalAlignment.Center;
        //
        // lblPort
        //
        lblPort.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPort.AutoSize = true;
        lblPort.ForeColor = Color.FromArgb(160, 196, 198);
        lblPort.Location = new Point(590, 29);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(29, 15);
        lblPort.TabIndex = 1;
        lblPort.Text = "Port";
        //
        // lblAltBaslik
        //
        lblAltBaslik.AutoSize = true;
        lblAltBaslik.ForeColor = Color.FromArgb(130, 170, 172);
        lblAltBaslik.Location = new Point(22, 46);
        lblAltBaslik.Name = "lblAltBaslik";
        lblAltBaslik.Size = new Size(230, 15);
        lblAltBaslik.TabIndex = 1;
        lblAltBaslik.Text = "YMH219 · TcpListener · çok istemcili oda";
        //
        // lblBaslik
        //
        lblBaslik.AutoSize = true;
        lblBaslik.Font = new Font("Segoe UI Semibold", 17F);
        lblBaslik.ForeColor = Color.White;
        lblBaslik.Location = new Point(18, 10);
        lblBaslik.Name = "lblBaslik";
        lblBaslik.Size = new Size(185, 31);
        lblBaslik.TabIndex = 0;
        lblBaslik.Text = "Sohbet Sunucusu";
        //
        // pnlIstatistik
        //
        pnlIstatistik.BackColor = Color.FromArgb(236, 242, 242);
        pnlIstatistik.ColumnCount = 4;
        pnlIstatistik.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        pnlIstatistik.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        pnlIstatistik.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        pnlIstatistik.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        pnlIstatistik.Controls.Add(kutuDurum, 0, 0);
        pnlIstatistik.Controls.Add(kutuKullanici, 1, 0);
        pnlIstatistik.Controls.Add(kutuMesaj, 2, 0);
        pnlIstatistik.Controls.Add(kutuSure, 3, 0);
        pnlIstatistik.Dock = DockStyle.Top;
        pnlIstatistik.Location = new Point(0, 76);
        pnlIstatistik.Name = "pnlIstatistik";
        pnlIstatistik.Padding = new Padding(13, 12, 13, 2);
        pnlIstatistik.RowCount = 1;
        pnlIstatistik.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        pnlIstatistik.Size = new Size(980, 96);
        pnlIstatistik.TabIndex = 1;
        //
        // kutuDurum
        //
        kutuDurum.Aciklama = "Durum";
        kutuDurum.Dock = DockStyle.Fill;
        kutuDurum.Name = "kutuDurum";
        kutuDurum.TabIndex = 0;
        //
        // kutuKullanici
        //
        kutuKullanici.Aciklama = "Bağlı kullanıcı";
        kutuKullanici.Dock = DockStyle.Fill;
        kutuKullanici.Name = "kutuKullanici";
        kutuKullanici.TabIndex = 1;
        //
        // kutuMesaj
        //
        kutuMesaj.Aciklama = "İletilen mesaj";
        kutuMesaj.Dock = DockStyle.Fill;
        kutuMesaj.Name = "kutuMesaj";
        kutuMesaj.TabIndex = 2;
        //
        // kutuSure
        //
        kutuSure.Aciklama = "Çalışma süresi";
        kutuSure.Dock = DockStyle.Fill;
        kutuSure.Name = "kutuSure";
        kutuSure.TabIndex = 3;
        //
        // tblGovde
        //
        tblGovde.BackColor = Color.FromArgb(236, 242, 242);
        tblGovde.ColumnCount = 2;
        tblGovde.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        tblGovde.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        tblGovde.Controls.Add(kartIstemciler, 0, 0);
        tblGovde.Controls.Add(kartGunluk, 1, 0);
        tblGovde.Dock = DockStyle.Fill;
        tblGovde.Location = new Point(0, 172);
        tblGovde.Name = "tblGovde";
        tblGovde.Padding = new Padding(13, 4, 13, 4);
        tblGovde.RowCount = 1;
        tblGovde.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tblGovde.Size = new Size(980, 382);
        tblGovde.TabIndex = 2;
        //
        // kartIstemciler
        //
        kartIstemciler.Baslik = "Bağlı istemciler";
        kartIstemciler.Controls.Add(lvIstemciler);
        kartIstemciler.Controls.Add(pnlKartAlt);
        kartIstemciler.Dock = DockStyle.Fill;
        kartIstemciler.Name = "kartIstemciler";
        kartIstemciler.TabIndex = 0;
        //
        // lvIstemciler
        //
        lvIstemciler.BorderStyle = BorderStyle.None;
        lvIstemciler.Columns.AddRange(new ColumnHeader[] { colKullanici, colAdres, colBaglandi, colMesaj });
        lvIstemciler.Dock = DockStyle.Fill;
        lvIstemciler.Font = new Font("Segoe UI", 9.75F);
        lvIstemciler.FullRowSelect = true;
        lvIstemciler.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvIstemciler.MultiSelect = false;
        lvIstemciler.Name = "lvIstemciler";
        lvIstemciler.TabIndex = 0;
        lvIstemciler.UseCompatibleStateImageBehavior = false;
        lvIstemciler.View = View.Details;
        lvIstemciler.SelectedIndexChanged += lvIstemciler_SelectedIndexChanged;
        //
        // colKullanici
        //
        colKullanici.Text = "Kullanıcı";
        colKullanici.Width = 150;
        //
        // colAdres
        //
        colAdres.Text = "Adres";
        colAdres.Width = 150;
        //
        // colBaglandi
        //
        colBaglandi.Text = "Bağlandı";
        colBaglandi.Width = 80;
        //
        // colMesaj
        //
        colMesaj.Text = "Mesaj";
        colMesaj.TextAlign = HorizontalAlignment.Right;
        colMesaj.Width = 60;
        //
        // pnlKartAlt
        //
        pnlKartAlt.BackColor = Color.White;
        pnlKartAlt.Controls.Add(btnCikar);
        pnlKartAlt.Dock = DockStyle.Bottom;
        pnlKartAlt.Name = "pnlKartAlt";
        pnlKartAlt.Size = new Size(200, 44);
        pnlKartAlt.TabIndex = 1;
        //
        // btnCikar
        //
        btnCikar.Enabled = false;
        btnCikar.Location = new Point(0, 8);
        btnCikar.Name = "btnCikar";
        btnCikar.Size = new Size(200, 32);
        btnCikar.Stil = ChatApp.Ui.ButonStili.Acik;
        btnCikar.TabIndex = 0;
        btnCikar.Text = "Seçili kullanıcıyı çıkar";
        btnCikar.Click += btnCikar_Click;
        //
        // kartGunluk
        //
        kartGunluk.Baslik = "Etkinlik günlüğü";
        kartGunluk.Controls.Add(rtbGunluk);
        kartGunluk.Dock = DockStyle.Fill;
        kartGunluk.Name = "kartGunluk";
        kartGunluk.TabIndex = 1;
        //
        // rtbGunluk
        //
        rtbGunluk.BackColor = Color.White;
        rtbGunluk.BorderStyle = BorderStyle.None;
        rtbGunluk.DetectUrls = false;
        rtbGunluk.Dock = DockStyle.Fill;
        rtbGunluk.Font = new Font("Consolas", 9.5F);
        rtbGunluk.Name = "rtbGunluk";
        rtbGunluk.ReadOnly = true;
        rtbGunluk.TabIndex = 0;
        rtbGunluk.Text = "";
        //
        // pnlAlt
        //
        pnlAlt.BackColor = Color.FromArgb(236, 242, 242);
        pnlAlt.Controls.Add(btnGunlukKaydet);
        pnlAlt.Controls.Add(btnDuyuru);
        pnlAlt.Controls.Add(txtDuyuru);
        pnlAlt.Dock = DockStyle.Bottom;
        pnlAlt.Name = "pnlAlt";
        pnlAlt.Size = new Size(980, 62);
        pnlAlt.TabIndex = 3;
        //
        // btnGunlukKaydet
        //
        btnGunlukKaydet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnGunlukKaydet.Location = new Point(806, 12);
        btnGunlukKaydet.Name = "btnGunlukKaydet";
        btnGunlukKaydet.Size = new Size(158, 36);
        btnGunlukKaydet.Stil = ChatApp.Ui.ButonStili.Acik;
        btnGunlukKaydet.TabIndex = 2;
        btnGunlukKaydet.Text = "Günlüğü kaydet";
        btnGunlukKaydet.Click += btnGunlukKaydet_Click;
        //
        // btnDuyuru
        //
        btnDuyuru.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDuyuru.Enabled = false;
        btnDuyuru.Location = new Point(652, 12);
        btnDuyuru.Name = "btnDuyuru";
        btnDuyuru.Size = new Size(146, 36);
        btnDuyuru.TabIndex = 1;
        btnDuyuru.Text = "Duyuru gönder";
        btnDuyuru.Click += btnDuyuru_Click;
        //
        // txtDuyuru
        //
        txtDuyuru.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDuyuru.BorderStyle = BorderStyle.FixedSingle;
        txtDuyuru.Font = new Font("Segoe UI", 10.5F);
        txtDuyuru.Location = new Point(16, 16);
        txtDuyuru.MaxLength = 1000;
        txtDuyuru.Name = "txtDuyuru";
        txtDuyuru.PlaceholderText = "Tüm kullanıcılara duyuru...";
        txtDuyuru.Size = new Size(626, 26);
        txtDuyuru.TabIndex = 0;
        txtDuyuru.KeyDown += txtDuyuru_KeyDown;
        //
        // durumCubugu
        //
        durumCubugu.BackColor = Color.White;
        durumCubugu.Items.AddRange(new ToolStripItem[] { lblAdresler, lblHata });
        durumCubugu.Name = "durumCubugu";
        durumCubugu.Size = new Size(980, 22);
        durumCubugu.SizingGrip = false;
        durumCubugu.TabIndex = 4;
        //
        // lblAdresler
        //
        lblAdresler.Name = "lblAdresler";
        lblAdresler.Size = new Size(200, 17);
        lblAdresler.Text = "Adresler";
        //
        // lblHata
        //
        lblHata.ForeColor = Color.FromArgb(206, 54, 74);
        lblHata.Name = "lblHata";
        lblHata.Size = new Size(700, 17);
        lblHata.Spring = true;
        lblHata.TextAlign = ContentAlignment.MiddleRight;
        //
        // saniyeZamanlayici
        //
        saniyeZamanlayici.Interval = 1000;
        saniyeZamanlayici.Tick += saniyeZamanlayici_Tick;
        //
        // SunucuFormu
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(236, 242, 242);
        ClientSize = new Size(980, 640);
        Controls.Add(tblGovde);
        Controls.Add(pnlAlt);
        Controls.Add(durumCubugu);
        Controls.Add(pnlIstatistik);
        Controls.Add(pnlUst);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(860, 560);
        Name = "SunucuFormu";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sohbet Sunucusu - YMH219";
        FormClosing += SunucuFormu_FormClosing;
        pnlUst.ResumeLayout(false);
        pnlUst.PerformLayout();
        pnlIstatistik.ResumeLayout(false);
        tblGovde.ResumeLayout(false);
        kartIstemciler.ResumeLayout(false);
        pnlKartAlt.ResumeLayout(false);
        kartGunluk.ResumeLayout(false);
        pnlAlt.ResumeLayout(false);
        pnlAlt.PerformLayout();
        durumCubugu.ResumeLayout(false);
        durumCubugu.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Panel pnlUst;
    private Label lblBaslik;
    private Label lblAltBaslik;
    private Label lblPort;
    private TextBox txtPort;
    private ChatApp.Ui.YuvarlakButon btnBaslat;
    private ChatApp.Ui.YuvarlakButon btnDurdur;
    private TableLayoutPanel pnlIstatistik;
    private ChatApp.Ui.IstatistikKutusu kutuDurum;
    private ChatApp.Ui.IstatistikKutusu kutuKullanici;
    private ChatApp.Ui.IstatistikKutusu kutuMesaj;
    private ChatApp.Ui.IstatistikKutusu kutuSure;
    private TableLayoutPanel tblGovde;
    private ChatApp.Ui.Kart kartIstemciler;
    private ListView lvIstemciler;
    private ColumnHeader colKullanici;
    private ColumnHeader colAdres;
    private ColumnHeader colBaglandi;
    private ColumnHeader colMesaj;
    private Panel pnlKartAlt;
    private ChatApp.Ui.YuvarlakButon btnCikar;
    private ChatApp.Ui.Kart kartGunluk;
    private RichTextBox rtbGunluk;
    private Panel pnlAlt;
    private TextBox txtDuyuru;
    private ChatApp.Ui.YuvarlakButon btnDuyuru;
    private ChatApp.Ui.YuvarlakButon btnGunlukKaydet;
    private StatusStrip durumCubugu;
    private ToolStripStatusLabel lblAdresler;
    private ToolStripStatusLabel lblHata;
    private System.Windows.Forms.Timer saniyeZamanlayici;
}
