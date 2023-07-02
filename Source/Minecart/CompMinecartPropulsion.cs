using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Minecart;

public class CompMinecartPropulsion : ThingComp
{
    private Texture2D cachedCommandTex;

    public bool Throttle { get; set; }

    public CompProperties_MinecartPropulsion Props => props as CompProperties_MinecartPropulsion;

    public Building_Minecart Parent => parent as Building_Minecart;

    private Texture2D CommandTex
    {
        get
        {
            if (cachedCommandTex == null)
            {
                cachedCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower");
            }

            return cachedCommandTex;
        }
    }

    public override void Initialize(CompProperties props)
    {
        this.props = props;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        var throttleToggle = new Command_Toggle
        {
            toggleAction = () => Throttle = !Throttle,
            isActive = () => Throttle,
            defaultLabel = "MGHU.Throttle".Translate(),
            defaultDesc = "MGHU.ThrottleTT".Translate(),
            icon = CommandTex
        };

        yield return throttleToggle;
    }

    public override void CompTick()
    {
        var refuelable = Parent.GetComp<CompRefuelable>();
        var breakdownable = Parent.GetComp<CompBreakdownable>();
        var transporter = Parent.GetComp<CompTransporter>();
        var flickable = Parent.GetComp<CompFlickable>();

        if (Throttle)
        {
            if (
                (refuelable == null || refuelable.Fuel > 0f) &&
                (!breakdownable?.BrokenDown ?? true) &&
                (flickable?.SwitchIsOn ?? true)
            )
            {
                FleckMaker.ThrowAirPuffUp(Parent.DrawPos + new Vector3(0.0f, 0.5f), Parent.Map);
                refuelable?.Notify_UsedThisTick();

                Parent.Push(Props.pushSpeed, Props.pushAccel, Props.pushPower);
            }
        }
        else
        {
            Parent.Brake(Props.pushAccel, Props.pushPower);
        }

        if (transporter != null
            && refuelable != null
            && parent.IsHashIntervalTick(GenTicks.TickRareInterval)
            && refuelable.Props.fuelCapacity - refuelable.Fuel > 1f
            && transporter.innerContainer.Any(t => refuelable.Props.fuelFilter.Allows(t)))
        {
            refuelable.Refuel(
                new List<Thing>
                {
                    transporter.innerContainer.First(
                        t => refuelable.Props.fuelFilter.Allows(t))
                });
        }
    }
}