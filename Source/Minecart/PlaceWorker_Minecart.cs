using Verse;

namespace Minecart;

public class PlaceWorker_Minecart : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef bcheckingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var def = bcheckingDef as ThingDef_Minecart;
        if (!Building_Minecart.IsClear(loc, map, def, ignoreRails: true))
        {
            return new AcceptanceReport("MGHU.Blocked".Translate());
        }

        if (!Building_Minecart.IsClear(loc, map, def))
        {
            return new AcceptanceReport("MGHU.NoRail".Translate());
        }

        return !Building_Minecart.IsClear(loc + rot.FacingCell, map, def, true)
            ? new AcceptanceReport("MGHU.PathNotClear".Translate())
            : AcceptanceReport.WasAccepted;
    }
}