using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Minecart;

public class CompProperties_MinecartPropulsion : CompProperties
{
    public readonly float pushAccel; // How fast the minecart of 1kg will speed up per tick
    public readonly float pushPower; // Effective mass is divided by this
    public readonly float pushSpeed; // Maximum theoretical speed under it's own power

    public CompProperties_MinecartPropulsion()
    {
        compClass = typeof(CompMinecartPropulsion);
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
        foreach (var s in base.SpecialDisplayStats(req))
        {
            yield return s;
        }

        yield return new StatDrawEntry(StatCategoryDefOf.Building, "MGHU.Acceleration".Translate(),
            pushAccel.ToString("0.00"),
            string.Empty, 0);
        yield return new StatDrawEntry(StatCategoryDefOf.Building, "MGHU.Power".Translate(), pushPower.ToString("0.00"),
            string.Empty,
            0);
        yield return new StatDrawEntry(StatCategoryDefOf.Building, "MGHU.SpeedStat".Translate(),
            pushSpeed.ToString("0.00'c'"), string.Empty,
            0);
    }
}