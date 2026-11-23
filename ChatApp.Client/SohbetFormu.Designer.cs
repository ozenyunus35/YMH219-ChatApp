namespace ChatApp.Client;

partial class SohbetFormu
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
        pnlYan = new Panel();
        btnGecmisKaydet = new ChatApp.Ui.YuvarlakButon();
        lstKullanicilar = new ChatApp.Ui.KullaniciListesi();
        lblCevrimici = new Label();
        pnlBaglanti = new Panel();
        btnKes = new ChatApp.Ui.YuvarlakButon();
        btnBaglan = new ChatApp.Ui.YuvarlakButon();
        txtKullanici = new TextBox();
        lblKullanici = new Label();
        txtPort = new TextBox();
        lblPort = new Label();
        txtSunucu = new TextBox();
        lblSunucu = new Label();
        lblAltBaslik = new Label();
        lblBaslik = new Label();
        pnlUst = new Panel();
        durumRozeti = new ChatApp.Ui.DurumRozeti();
        lblOdaBilgi = new Label();
        lblOda = new Label();
        pnlUyari = new Panel();
        btnUyariKapat = new Button();
        lblUyari = new Label();
        mesajListesi = new ChatApp.Ui.MesajListesi();
        pnlAlt = new Panel();
        pnlGiris = new Panel();
        txtMesaj = new TextBox();
        btnOzelIptal = new Button();
        btnGonder = new ChatApp.Ui.YuvarlakButon();
        lblYaziyor = new Label();
        yaziyorZamanlayici = new System.Windows.Forms.Timer(components);
        ipucu = new ToolTip(components);
        pnlYan.SuspendLayout();
        pnlBaglanti.SuspendLayout();
        pnlUst.SuspendLayout();
        pnlUyari.SuspendLayout();
        pnlAlt.SuspendLayout();
        pnlGiris.SuspendLayout();
        SuspendLayout();
        //
        // pnlYan
        //
        pnlYan.BackColor = Color.FromArgb(14, 42, 47);
        pnlYan.Controls.Add(btnGecmisKaydet);
        pnlYan.Controls.Add(lstKullanicilar);
        pnlYan.Controls.Add(lblCevrimici);
        pnlYan.Controls.Add(pnlBaglanti);
        pnlYan.Controls.Add(lblAltBaslik);
        pnlYan.Controls.Add(lblBaslik);
        pnlYan.Dock = DockStyle.Left;
        pnlYan.Location = new Point(0, 0);
        pnlYan.Name = "pnlYan";
        pnlYan.Size = new Size(280, 660);
        pnlYan.TabIndex = 0;
        //
        // btnGecmisKaydet
        //
        btnGecmisKaydet.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnGecmisKaydet.Location = new Point(16, 612);
        btnGecmisKaydet.Name = "btnGecmisKaydet";
        btnGecmisKaydet.Size = new Size(248, 34);
        btnGecmisKaydet.Stil = ChatApp.Ui.ButonStili.Cerceve;
        btnGecmisKaydet.TabIndex = 5;
        btnGecmisKaydet.Text = "Geçmişi kaydet (.txt)";
        btnGecmisKaydet.Click += btnGecmisKaydet_Click;
        //
        // lstKullanicilar
        //
        lstKullanicilar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        lstKullanicilar.FormattingEnabled = true;
        lstKullanicilar.Location = new Point(12, 330);
        lstKullanicilar.Name = "lstKullanicilar";
        lstKullanicilar.Size = new Size(256, 270);
        lstKullanicilar.TabIndex = 4;
        lstKullanicilar.DoubleClick += lstKullanicilar_DoubleClick;
        //
        // lblCevrimici
        //
        lblCevrimici.AutoSize = true;
        lblCevrimici.Font = new Font("Segoe UI Semibold", 8.25F);
        lblCevrimici.ForeColor = Color.FromArgb(130, 170, 172);
        lblCevrimici.Location = new Point(18, 306);
        lblCevrimici.Name = "lblCevrimici";
        lblCevrimici.Size = new Size(90, 13);
        lblCevrimici.TabIndex = 3;
        lblCevrimici.Text = "ÇEVRİMİÇİ — 0";
        //
        // pnlBaglanti
        //
        pnlBaglanti.BackColor = Color.FromArgb(22, 60, 66);
        pnlBaglanti.Controls.Add(btnKes);
        pnlBaglanti.Controls.Add(btnBaglan);
        pnlBaglanti.Controls.Add(txtKullanici);
        pnlBaglanti.Controls.Add(lblKullanici);
        pnlBaglanti.Controls.Add(txtPort);
        pnlBaglanti.Controls.Add(lblPort);
        pnlBaglanti.Controls.Add(txtSunucu);
        pnlBaglanti.Controls.Add(lblSunucu);
        pnlBaglanti.Location = new Point(16, 84);
        pnlBaglanti.Name = "pnlBaglanti";
        pnlBaglanti.Size = new Size(248, 206);
        pnlBaglanti.TabIndex = 2;
        //
        // btnKes
        //
        btnKes.Enabled = false;
        btnKes.Location = new Point(12, 162);
        btnKes.Name = "btnKes";
        btnKes.Size = new Size(224, 32);
        btnKes.Stil = ChatApp.Ui.ButonStili.Cerceve;
        btnKes.TabIndex = 7;
        btnKes.Text = "Bağlantıyı Kes";
        btnKes.Click += btnKes_Click;
        //
        // btnBaglan
        //
        btnBaglan.Location = new Point(12, 120);
        btnBaglan.Name = "btnBaglan";
        btnBaglan.Size = new Size(224, 36);
        btnBaglan.TabIndex = 6;
        btnBaglan.Text = "Bağlan";
        btnBaglan.Click += btnBaglan_Click;
        //
        // txtKullanici
        //
        txtKullanici.BackColor = Color.FromArgb(14, 42, 47);
        txtKullanici.BorderStyle = BorderStyle.FixedSingle;
        txtKullanici.Font = new Font("Segoe UI", 10F);
        txtKullanici.ForeColor = Color.White;
        txtKullanici.Location = new Point(12, 82);
        txtKullanici.MaxLength = 20;
        txtKullanici.Name = "txtKullanici";
        txtKullanici.PlaceholderText = "ör. yunus";
        txtKullanici.Size = new Size(224, 25);
        txtKullanici.TabIndex = 5;
        txtKullanici.KeyDown += txtKullanici_KeyDown;
        //
        // lblKullanici
        //
        lblKullanici.AutoSize = true;
        lblKullanici.ForeColor = Color.FromArgb(160, 196, 198);
        lblKullanici.Location = new Point(12, 63);
        lblKullanici.Name = "lblKullanici";
        lblKullanici.Size = new Size(77, 15);
        lblKullanici.TabIndex = 4;
        lblKullanici.Text = "Kullanıcı adı";
        //
        // txtPort
        //
        txtPort.BackColor = Color.FromArgb(14, 42, 47);
        txtPort.BorderStyle = BorderStyle.FixedSingle;
        txtPort.Font = new Font("Segoe UI", 10F);
        txtPort.ForeColor = Color.White;
        txtPort.Location = new Point(172, 30);
        txtPort.MaxLength = 5;
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(64, 25);
        txtPort.TabIndex = 3;
        txtPort.Text = "5050";
        //
        // lblPort
        //
        lblPort.AutoSize = true;
        lblPort.ForeColor = Color.FromArgb(160, 196, 198);
        lblPort.Location = new Point(172, 11);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(29, 15);
        lblPort.TabIndex = 2;
        lblPort.Text = "Port";
        //
        // txtSunucu
        //
        txtSunucu.BackColor = Color.FromArgb(14, 42, 47);
        txtSunucu.BorderStyle = BorderStyle.FixedSingle;
        txtSunucu.Font = new Font("Segoe UI", 10F);
        txtSunucu.ForeColor = Color.White;
        txtSunucu.Location = new Point(12, 30);
        txtSunucu.Name = "txtSunucu";
        txtSunucu.Size = new Size(152, 25);
        txtSunucu.TabIndex = 1;
        txtSunucu.Text = "127.0.0.1";
        //
        // lblSunucu
        //
        lblSunucu.AutoSize = true;
        lblSunucu.ForeColor = Color.FromArgb(160, 196, 198);
        lblSunucu.Location = new Point(12, 11);
        lblSunucu.Name = "lblSunucu";
        lblSunucu.Size = new Size(83, 15);
        lblSunucu.TabIndex = 0;
        lblSunucu.Text = "Sunucu adresi";
        //
        // lblAltBaslik
        //
        lblAltBaslik.AutoSize = true;
        lblAltBaslik.ForeColor = Color.FromArgb(130, 170, 172);
        lblAltBaslik.Location = new Point(19, 50);
        lblAltBaslik.Name = "lblAltBaslik";
        lblAltBaslik.Size = new Size(146, 15);
        lblAltBaslik.TabIndex = 1;
        lblAltBaslik.Text = "YMH219 · TCP istemcisi";
        //
        // lblBaslik
        //
        lblBaslik.AutoSize = true;
        lblBaslik.Font = new Font("Segoe UI Semibold", 17F);
        lblBaslik.ForeColor = Color.White;
        lblBaslik.Location = new Point(14, 14);
        lblBaslik.Name = "lblBaslik";
        lblBaslik.Size = new Size(83, 31);
        lblBaslik.TabIndex = 0;
        lblBaslik.Text = "Sohbet";
        //
        // pnlUst
        //
        pnlUst.BackColor = Color.White;
        pnlUst.Controls.Add(durumRozeti);
        pnlUst.Controls.Add(lblOdaBilgi);
        pnlUst.Controls.Add(lblOda);
        pnlUst.Dock = DockStyle.Top;
        pnlUst.Location = new Point(280, 0);
        pnlUst.Name = "pnlUst";
        pnlUst.Size = new Size(720, 64);
        pnlUst.TabIndex = 1;
        pnlUst.Paint += pnlUst_Paint;
        //
        // durumRozeti
        //
        durumRozeti.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        durumRozeti.Location = new Point(560, 17);
        durumRozeti.Name = "durumRozeti";
        durumRozeti.Size = new Size(144, 30);
        durumRozeti.TabIndex = 2;
        durumRozeti.Text = "Bağlı değil";
        //
        // lblOdaBilgi
        //
        lblOdaBilgi.AutoSize = true;
        lblOdaBilgi.ForeColor = Color.FromArgb(104, 122, 128);
        lblOdaBilgi.Location = new Point(22, 37);
        lblOdaBilgi.Name = "lblOdaBilgi";
        lblOdaBilgi.Size = new Size(200, 15);
        lblOdaBilgi.TabIndex = 1;
        lblOdaBilgi.Text = "Bağlanmak için soldaki formu doldurun";
        //
        // lblOda
        //
        lblOda.AutoSize = true;
        lblOda.Font = new Font("Segoe UI Semibold", 13F);
        lblOda.ForeColor = Color.FromArgb(22, 34, 38);
        lblOda.Location = new Point(20, 10);
        lblOda.Name = "lblOda";
        lblOda.Size = new Size(64, 25);
        lblOda.TabIndex = 0;
        lblOda.Text = "# genel";
        //
        // pnlUyari
        //
        pnlUyari.BackColor = Color.FromArgb(253, 232, 235);
        pnlUyari.Controls.Add(btnUyariKapat);
        pnlUyari.Controls.Add(lblUyari);
        pnlUyari.Dock = DockStyle.Top;
        pnlUyari.Location = new Point(280, 64);
        pnlUyari.Name = "pnlUyari";
        pnlUyari.Size = new Size(720, 40);
        pnlUyari.TabIndex = 2;
        pnlUyari.Visible = false;
        //
        // btnUyariKapat
        //
        btnUyariKapat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnUyariKapat.FlatAppearance.BorderSize = 0;
        btnUyariKapat.FlatStyle = FlatStyle.Flat;
        btnUyariKapat.ForeColor = Color.FromArgb(206, 54, 74);
        btnUyariKapat.Location = new Point(680, 6);
        btnUyariKapat.Name = "btnUyariKapat";
        btnUyariKapat.Size = new Size(30, 28);
        btnUyariKapat.TabIndex = 1;
        btnUyariKapat.TabStop = false;
        btnUyariKapat.Text = "✕";
        btnUyariKapat.Click += btnUyariKapat_Click;
        //
        // lblUyari
        //
        lblUyari.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblUyari.Font = new Font("Segoe UI Semibold", 9F);
        lblUyari.ForeColor = Color.FromArgb(160, 30, 50);
        lblUyari.Location = new Point(20, 0);
        lblUyari.Name = "lblUyari";
        lblUyari.Size = new Size(650, 40);
        lblUyari.TabIndex = 0;
        lblUyari.Text = "Uyarı";
        lblUyari.TextAlign = ContentAlignment.MiddleLeft;
        //
        // mesajListesi
        //
        mesajListesi.Dock = DockStyle.Fill;
        mesajListesi.Location = new Point(280, 104);
        mesajListesi.Name = "mesajListesi";
        mesajListesi.Size = new Size(720, 478);
        mesajListesi.TabIndex = 3;
        //
        // pnlAlt
        //
        pnlAlt.BackColor = Color.FromArgb(236, 242, 242);
        pnlAlt.Controls.Add(pnlGiris);
        pnlAlt.Controls.Add(lblYaziyor);
        pnlAlt.Dock = DockStyle.Bottom;
        pnlAlt.Location = new Point(280, 582);
        pnlAlt.Name = "pnlAlt";
        pnlAlt.Size = new Size(720, 78);
        pnlAlt.TabIndex = 4;
        //
        // pnlGiris
        //
        pnlGiris.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlGiris.BackColor = Color.White;
        pnlGiris.Controls.Add(txtMesaj);
        pnlGiris.Controls.Add(btnOzelIptal);
        pnlGiris.Controls.Add(btnGonder);
        pnlGiris.Location = new Point(16, 22);
        pnlGiris.Name = "pnlGiris";
        pnlGiris.Padding = new Padding(12, 0, 0, 0);
        pnlGiris.Size = new Size(688, 44);
        pnlGiris.TabIndex = 1;
        pnlGiris.Paint += pnlGiris_Paint;
        //
        // txtMesaj
        //
        txtMesaj.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtMesaj.BorderStyle = BorderStyle.None;
        txtMesaj.ReadOnly = true;
        txtMesaj.Font = new Font("Segoe UI", 10.5F);
        txtMesaj.Location = new Point(14, 12);
        txtMesaj.MaxLength = 1000;
        txtMesaj.Name = "txtMesaj";
        txtMesaj.PlaceholderText = "Mesaj yazın...  (/w kullanıcı mesaj = özel mesaj)";
        txtMesaj.Size = new Size(560, 19);
        txtMesaj.TabIndex = 0;
        txtMesaj.TextChanged += txtMesaj_TextChanged;
        txtMesaj.KeyDown += txtMesaj_KeyDown;
        //
        // btnOzelIptal
        //
        btnOzelIptal.Anchor = AnchorStyles.Right;
        btnOzelIptal.BackColor = Color.FromArgb(243, 238, 252);
        btnOzelIptal.FlatAppearance.BorderSize = 0;
        btnOzelIptal.FlatStyle = FlatStyle.Flat;
        btnOzelIptal.Font = new Font("Segoe UI Semibold", 8.25F);
        btnOzelIptal.ForeColor = Color.FromArgb(124, 77, 196);
        btnOzelIptal.Location = new Point(470, 9);
        btnOzelIptal.Name = "btnOzelIptal";
        btnOzelIptal.Size = new Size(110, 26);
        btnOzelIptal.TabIndex = 2;
        btnOzelIptal.TabStop = false;
        btnOzelIptal.Text = "Özel: - ✕";
        btnOzelIptal.UseVisualStyleBackColor = false;
        btnOzelIptal.Visible = false;
        btnOzelIptal.Click += btnOzelIptal_Click;
        //
        // btnGonder
        //
        btnGonder.Anchor = AnchorStyles.Right;
        btnGonder.Enabled = false;
        btnGonder.Location = new Point(590, 5);
        btnGonder.Name = "btnGonder";
        btnGonder.Size = new Size(92, 34);
        btnGonder.TabIndex = 1;
        btnGonder.Text = "Gönder";
        btnGonder.Click += btnGonder_Click;
        //
        // lblYaziyor
        //
        lblYaziyor.AutoSize = true;
        lblYaziyor.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
        lblYaziyor.ForeColor = Color.FromArgb(104, 122, 128);
        lblYaziyor.Location = new Point(20, 3);
        lblYaziyor.Name = "lblYaziyor";
        lblYaziyor.Size = new Size(0, 13);
        lblYaziyor.TabIndex = 0;
        //
        // yaziyorZamanlayici
        //
        yaziyorZamanlayici.Interval = 3000;
        yaziyorZamanlayici.Tick += yaziyorZamanlayici_Tick;
        //
        // SohbetFormu
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(236, 242, 242);
        ClientSize = new Size(1000, 660);
        Controls.Add(mesajListesi);
        Controls.Add(pnlAlt);
        Controls.Add(pnlUyari);
        Controls.Add(pnlUst);
        Controls.Add(pnlYan);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(860, 560);
        Name = "SohbetFormu";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sohbet - YMH219";
        FormClosing += SohbetFormu_FormClosing;
        Shown += SohbetFormu_Shown;
        pnlYan.ResumeLayout(false);
        pnlYan.PerformLayout();
        pnlBaglanti.ResumeLayout(false);
        pnlBaglanti.PerformLayout();
        pnlUst.ResumeLayout(false);
        pnlUst.PerformLayout();
        pnlUyari.ResumeLayout(false);
        pnlAlt.ResumeLayout(false);
        pnlAlt.PerformLayout();
        pnlGiris.ResumeLayout(false);
        pnlGiris.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlYan;
    private Label lblBaslik;
    private Label lblAltBaslik;
    private Panel pnlBaglanti;
    private Label lblSunucu;
    private TextBox txtSunucu;
    private Label lblPort;
    private TextBox txtPort;
    private Label lblKullanici;
    private TextBox txtKullanici;
    private ChatApp.Ui.YuvarlakButon btnBaglan;
    private ChatApp.Ui.YuvarlakButon btnKes;
    private Label lblCevrimici;
    private ChatApp.Ui.KullaniciListesi lstKullanicilar;
    private ChatApp.Ui.YuvarlakButon btnGecmisKaydet;
    private Panel pnlUst;
    private Label lblOda;
    private Label lblOdaBilgi;
    private ChatApp.Ui.DurumRozeti durumRozeti;
    private Panel pnlUyari;
    private Label lblUyari;
    private Button btnUyariKapat;
    private ChatApp.Ui.MesajListesi mesajListesi;
    private Panel pnlAlt;
    private Label lblYaziyor;
    private Panel pnlGiris;
    private TextBox txtMesaj;
    private Button btnOzelIptal;
    private ChatApp.Ui.YuvarlakButon btnGonder;
    private System.Windows.Forms.Timer yaziyorZamanlayici;
    private ToolTip ipucu;
}
