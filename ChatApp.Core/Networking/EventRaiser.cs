namespace ChatApp.Core.Networking;

/// <summary>
/// Ağ işlemleri arka plan thread'lerinde çalışıyor ama WinForms kontrolleri sadece
/// kendi (UI) thread'lerinden güncellenebilir. Bu sınıf nesne oluşturulurken
/// SynchronizationContext'i yakalıyor ve olayları o thread'e gönderiyor.
/// Testlerde context olmadığı için olay doğrudan çağrılıyor.
/// </summary>
internal sealed class EventRaiser
{
    private readonly SynchronizationContext? context = SynchronizationContext.Current;

    public void Raise(Action action)
    {
        if (context is null || context == SynchronizationContext.Current)
            action();
        else
            context.Post(_ => action(), null);
    }
}
