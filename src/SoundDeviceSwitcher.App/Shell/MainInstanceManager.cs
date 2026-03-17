using System.Threading;

namespace SoundDeviceSwitcher.App.Shell;

internal static class MainInstanceManager
{
    private const string MainMutexName = @"Local\SoundDeviceSwitcher.Main";
    private const string RestoreEventName = @"Local\SoundDeviceSwitcher.Main.Restore";

    public static Mutex CreateMutex(out bool createdNew)
    {
        return new Mutex(true, MainMutexName, out createdNew);
    }

    public static EventWaitHandle CreateRestoreEvent()
    {
        return new EventWaitHandle(false, EventResetMode.AutoReset, RestoreEventName);
    }

    public static void SignalExistingInstance()
    {
        try
        {
            using var restoreEvent = EventWaitHandle.OpenExisting(RestoreEventName);
            restoreEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }
}
