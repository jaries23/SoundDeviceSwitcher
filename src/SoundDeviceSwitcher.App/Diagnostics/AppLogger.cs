using System.Text;

namespace SoundDeviceSwitcher.App.Diagnostics;

internal static class AppLogger
{
    private static readonly object SyncRoot = new();

    public static string LogDirectoryPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SoundDeviceSwitcher",
            "logs");

    public static string LatestLogPath => Path.Combine(LogDirectoryPath, "latest.log");

    public static void LogException(string source, Exception exception)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}");
        builder.AppendLine(exception.ToString());
        builder.AppendLine();

        Write(builder.ToString());
    }

    public static void LogInfo(string message)
    {
        Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static void Write(string content)
    {
        lock (SyncRoot)
        {
            Directory.CreateDirectory(LogDirectoryPath);
            File.AppendAllText(LatestLogPath, content);
        }
    }
}
