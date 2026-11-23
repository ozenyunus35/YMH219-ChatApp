namespace ChatApp.Server;

internal static class Program
{
    /// <summary>
    /// Sohbet sunucusu. Önce bu uygulama açılıp "Sunucuyu başlat" denmeli,
    /// sonra istemciler bağlanabilir.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SunucuFormu());
    }
}
