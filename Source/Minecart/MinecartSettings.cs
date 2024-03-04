using Verse;

namespace Minecart;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class MinecartSettings : ModSettings
{
    public int DropAllRange = 10;
    public int FreeSpaceRange = 1;
    public int StorageRange = 1;
    public bool VerboseLogging;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging", true);
        Scribe_Values.Look(ref DropAllRange, "DropAllRange", 10);
        Scribe_Values.Look(ref FreeSpaceRange, "FreeSpaceRange", 1);
        Scribe_Values.Look(ref StorageRange, "StorageRange", 1);
    }

    public void Reset()
    {
        VerboseLogging = false;
        DropAllRange = 10;
        FreeSpaceRange = 1;
        StorageRange = 1;
    }
}