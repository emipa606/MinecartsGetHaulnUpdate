using Verse;

namespace Minecart;

public static class Main
{
    public static void LogMessage(string message)
    {
        if (MinecartMod.instance.Settings.VerboseLogging)
        {
            Log.Message($"[Minecarts]: {message}");
        }
    }
}