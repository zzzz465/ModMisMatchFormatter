using System.Diagnostics;

namespace Madeline.ModMismatchFormatter;

internal static class ModLog
{
    private const string Prefix = "[ModMismatchFormatter] ";

    /// <summary>Verbose trace; compiled out of release builds.</summary>
    [Conditional("DEBUG")]
    public static void Debug(string message) => Verse.Log.Message(Prefix + message);

    public static void Error(string message) => Verse.Log.Error(Prefix + message);
}
