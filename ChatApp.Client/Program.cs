namespace ChatApp.Client;

internal static class Program
{
    /// <summary>
    /// İstemci uygulaması. Aynı bilgisayarda birden fazla kez açılabilir.
    /// İsteğe bağlı: ChatApp.Client.exe [kullanıcı] [sunucu] [port]
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SohbetFormu(args));
    }
}
