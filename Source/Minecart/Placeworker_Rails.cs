using Verse;
using RimWorld;

namespace Minecart;

public class PlaceWorker_Rails : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef bcheckingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        if (loc.Impassable(map))
        {
            return new AcceptanceReport("MGHU.Blocked".Translate());
        }

        return 
            loc.GetFirstThing(map, ThingDefOf.ThingRail) == null &&
            loc.GetFirstThing(map, ThingDefOf.ThingPoweredRail) == null &&
            loc.GetFirstThing(map, ThingDefOf.ThingRail.blueprintDef) == null &&
            loc.GetFirstThing(map, ThingDefOf.ThingPoweredRail.blueprintDef) == null ?
        AcceptanceReport.WasAccepted : new AcceptanceReport("MGHU.RailAlreadyExists".Translate());
    }
}