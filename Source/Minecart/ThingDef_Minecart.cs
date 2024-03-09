using Verse;

namespace Minecart;

// Minecart ThingDef class
public class ThingDef_Minecart : ThingDef
{
    public float frictionCoef; // Speed is multiplied by this value every tick, simulating friction on the ground
    public float launchSpeed; // Speed is set to this value when the minecart is launched by hand

    public ThingDef railDef = ThingDefOf.ThingRail;
    public ThingDef railPoweredDef = ThingDefOf.ThingPoweredRail;

    public override void ResolveReferences()
    {
        if (railDef == null)
        {
            railDef = ThingDefOf.ThingRail;
        }
        if (railPoweredDef == null)
        {
            railPoweredDef = ThingDefOf.ThingPoweredRail;
        }
    }
}