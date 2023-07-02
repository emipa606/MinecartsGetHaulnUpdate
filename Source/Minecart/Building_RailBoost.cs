using RimWorld;
using Verse;

namespace Minecart;

public class Building_RailBoost : Building
{
    public ThingDef_RailBoost Def => def as ThingDef_RailBoost;

    public bool isActive()
    {
        var flickable = GetComp<CompFlickable>();
        var power = GetComp<CompPowerTrader>();
        return (flickable?.SwitchIsOn ?? true) && (power?.PowerOn ?? true);
    }

    public override void Tick()
    {
        var minecart = Position.GetFirstThing<Building_Minecart>(Map);
        if (minecart == null)
        {
            return;
        }

        if (isActive())
        {
            minecart.Push(Def.pushSpeed, Def.pushAccel, Def.pushPower);
        }
        else
        {
            minecart.Brake(Def.pushAccel, Def.pushPower);
        }
    }
}