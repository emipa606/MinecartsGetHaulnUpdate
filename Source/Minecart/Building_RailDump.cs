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
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

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
                if (compTransporter.innerContainer.Any)
                {
                    var validCells = this.CellsAdjacent8WayAndInside().Where(vec3 =>
                        vec3 != Position && vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null);
                    for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                    {
                        var thing = compTransporter.innerContainer[index];
                        var storageCell = validCells.FirstOrDefault(vec3 => vec3.IsValidStorageFor(Map, thing));

                        if (storageCell == default)
                        {
                            continue;
                        }

                        compTransporter.innerContainer.TryDrop(thing, storageCell, Map,
                            ThingPlaceMode.Direct, thing.stackCount, out _);
                    }


                    if (compTransporter.innerContainer.Any)
                    {
                        var emptyCells = validCells.Where(vec3 => !vec3.GetThingList(Map).Any(thing =>
                            thing.def.category == ThingCategory.Item && thing.def.EverHaulable)).ToList();
                        var currentCell = 0;
                        if (emptyCells.Any())
                        {
                            for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                            {
                                if (emptyCells.Count <= currentCell)
                                {
                                    break;
                                }

                                var thing = compTransporter.innerContainer[index];
                                compTransporter.innerContainer.TryDrop(thing, emptyCells[currentCell], Map,
                                    ThingPlaceMode.Direct,
                                    thing.stackCount, out _);
                                currentCell++;
                            }
                        }
                    }

                    if (compTransporter.innerContainer.Any && DumpMode)
                    {
                        compTransporter.innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
                    }

                    var transporters = compTransporter.TransportersInGroup(Map);
                    if (transporters == null || !transporters.Any(transporter => transporter.innerContainer.Any))
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