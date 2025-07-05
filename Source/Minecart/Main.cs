using Verse;

namespace Minecart;

public static class Main
{
    public static void LogMessage(string message)
    {
        if (MinecartMod.Instance.Settings.VerboseLogging)
        {
            Log.Message($"[Minecarts]: {message}");
        }
    }
}