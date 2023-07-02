using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Minecart;

public class Building_RailDump : Building
{
    public bool Mode { get; set; } = true;

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var mode = new Command_Toggle
        {
            icon = TexCommand.Install,
            defaultLabel = Mode ? "MGHU.DumpMode".Translate() : "MGHU.LoadMode".Translate(),
            isActive = () => Mode,
            toggleAction = delegate { Mode = !Mode; }
        };
        yield return mode;
    }

    public override void Tick()
    {
        var minecart = Position.GetFirstThing<Building_Minecart>(Map);
        if (minecart == null)
        {
            return;
        }

        var compTransporter = minecart.GetComp<CompTransporter>();
        if (compTransporter != null)
        {
            if (Mode)
            {
                compTransporter.innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
                compTransporter.CancelLoad();
            }
            else
            {
                foreach (var cell in this.CellsAdjacent8WayAndInside())
                {
                    var thing = cell.GetFirstItem(Map);
                    if (thing != null
                        && thing.IsInValidStorage()
                        && compTransporter.innerContainer.Sum(t => t.GetStatValue(StatDefOf.Mass) * t.stackCount)
                        + (thing.GetStatValue(StatDefOf.Mass) * thing.stackCount) <
                        compTransporter.Props.massCapacity)
                    {
                        compTransporter.innerContainer.TryAdd(thing.SplitOff(thing.stackCount));
                    }
                }
            }
        }

        var compRefuelable = minecart.GetComp<CompRefuelable>();
        if (compRefuelable == null || Mode || !(compRefuelable.TargetFuelLevel - compRefuelable.Fuel > 1f))
        {
            return;
        }

        foreach (var cell in this.CellsAdjacent8WayAndInside())
        {
            var thing = cell.GetFirstItem(Map);
            if (thing != null
                && thing.IsInValidStorage())
            {
                compRefuelable.Refuel(new List<Thing> { thing });
            }
        }
    }
}