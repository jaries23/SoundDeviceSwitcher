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

    public static void SignalExistingInstance(int attempts = 1, int delayMilliseconds = 0)
    {
        attempts = Math.Max(1, attempts);

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                using var restoreEvent = EventWaitHandle.OpenExisting(RestoreEventName);
                restoreEvent.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
            }

            if (attempt < attempts - 1 && delayMilliseconds > 0)
            {
                Thread.Sleep(delayMilliseconds);
            }
        }
    }
}
