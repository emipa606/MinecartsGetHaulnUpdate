using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Minecart;

public class Building_RailDump : Building
{
    private bool dumpMode = true;
    private bool mode = true;

    public bool Mode
    {
        get => mode;
        set => mode = value;
    }

    public bool DumpMode
    {
        get => dumpMode;
        set => dumpMode = value;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Toggle
        {
            icon = TexCommand.Install,
            defaultLabel = Mode ? "MGHU.DumpMode".Translate() : "MGHU.LoadMode".Translate(),
            isActive = () => Mode,
            toggleAction = delegate { Mode = !Mode; }
        };

        if (!Mode)
        {
            yield break;
        }

        yield return new Command_Toggle
        {
            icon = TexButton.SelectOverlappingNext,
            defaultLabel = DumpMode ? "MGHU.DumpAll".Translate() : "MGHU.DumpFree".Translate(),
            defaultDesc = DumpMode ? "MGHU.DumpAllDesc".Translate() : "MGHU.DumpFreeDesc".Translate(),
            isActive = () => DumpMode,
            toggleAction = delegate { DumpMode = !DumpMode; }
        };
    }

    public override void Tick()
    {
        var minecart = Position.GetFirstThing<Building_Minecart>(Map);

        var compTransporter = minecart?.GetComp<CompTransporter>();
        if (compTransporter != null)
        {
            if (Mode)
            {
                var transporters = compTransporter.TransportersInGroup(Map);
                if (transporters?.Any(transporter => transporter.innerContainer.Any) == true)
                {
                    var validCells = this.CellsAdjacent8WayAndInside().Where(vec3 =>
                        vec3 != Position && vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null &&
                        !vec3.GetThingList(Map).Any(thing =>
                            thing.def.category == ThingCategory.Item && thing.def.EverHaulable)).ToList();
                    var currentCell = 0;
                    foreach (var transporter in transporters)
                    {
                        if (!validCells.Any())
                        {
                            break;
                        }

                        for (var index = 0; index < transporter.innerContainer.Count; index++)
                        {
                            if (validCells.Count <= currentCell)
                            {
                                break;
                            }

                            var thing = transporter.innerContainer[index];
                            transporter.innerContainer.TryDrop(thing, validCells[currentCell], Map,
                                ThingPlaceMode.Direct,
                                thing.stackCount, out _);
                            currentCell++;
                        }
                    }

                    if (!transporters.Any(transporter => transporter.innerContainer.Any) || DumpMode)
                    {
                        compTransporter.CancelLoad();
                    }
                }
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

        var compRefuelable = minecart?.GetComp<CompRefuelable>();
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
                compRefuelable.Refuel([thing]);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref mode, "mode", true);
        Scribe_Values.Look(ref dumpMode, "dumpMode", true);
    }
}