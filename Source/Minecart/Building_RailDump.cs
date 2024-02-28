using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Minecart;

public class Building_RailDump : Building
{
    private bool dumpMode = true;
    private bool freeSpots;
    private bool mode = true;
    private bool storages;

    public bool IsInDumpMode
    {
        get => mode;
        set => mode = value;
    }

    public bool WillDumpWhereever
    {
        get => dumpMode;
        set
        {
            dumpMode = value;
            if (!value)
            {
                return;
            }

            freeSpots = false;
            storages = false;
        }
    }

    public bool WillDumpOnFreeSpots
    {
        get
        {
            if (!freeSpots && !dumpMode && !storages)
            {
                freeSpots = true;
            }

            return freeSpots;
        }
        set
        {
            freeSpots = value;
            if (!value)
            {
                return;
            }

            dumpMode = false;
            storages = false;
        }
    }

    public bool WillDumpInStorages
    {
        get => storages;
        set
        {
            storages = value;
            if (!value)
            {
                return;
            }

            freeSpots = false;
            dumpMode = false;
        }
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
            defaultLabel = IsInDumpMode ? "MGHU.DumpMode".Translate() : "MGHU.LoadMode".Translate(),
            isActive = () => IsInDumpMode,
            toggleAction = delegate { IsInDumpMode = !IsInDumpMode; }
        };

        if (!IsInDumpMode)
        {
            yield break;
        }

        yield return new Command_Toggle
        {
            icon = TexButton.SelectOverlappingNext,
            defaultLabel = "MGHU.DumpAll".Translate(),
            defaultDesc = "MGHU.DumpAllDesc".Translate(),
            isActive = () => WillDumpWhereever,
            toggleAction = delegate { WillDumpWhereever = !WillDumpWhereever; }
        };
        yield return new Command_Toggle
        {
            icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn"),
            defaultLabel = "MGHU.DumpFree".Translate(),
            defaultDesc = "MGHU.DumpFreeDesc".Translate(),
            isActive = () => WillDumpOnFreeSpots,
            toggleAction = delegate { WillDumpOnFreeSpots = !WillDumpOnFreeSpots; }
        };
        yield return new Command_Toggle
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter"),
            defaultLabel = "MGHU.UseStorage".Translate(),
            defaultDesc = "MGHU.UseStorageDesc".Translate(),
            isActive = () => WillDumpInStorages,
            toggleAction = delegate { WillDumpInStorages = !WillDumpInStorages; }
        };
    }

    public override void Tick()
    {
        var minecart = Position.GetFirstThing<Building_Minecart>(Map);

        var compTransporter = minecart?.CartTransporter;
        if (compTransporter != null)
        {
            if (IsInDumpMode)
            {
                if (compTransporter.innerContainer.Any)
                {
                    var validCells = this.CellsAdjacent8WayAndInside().Where(vec3 =>
                        vec3.InBounds(Map) &&
                        vec3 != Position && vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null);

                    if (WillDumpInStorages)
                    {
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
                    }


                    if (WillDumpOnFreeSpots || WillDumpWhereever)
                    {
                        var emptyCells = validCells.Where(vec3 => !vec3.GetThingList(Map).Any(thing =>
                                                                      thing.def.category == ThingCategory.Item &&
                                                                      thing.def.EverHaulable ||
                                                                      thing is Building_Storage) &&
                                                                  vec3.GetZone(Map)?.GetType() !=
                                                                  typeof(Zone_Stockpile)).ToList();
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

                    if (compTransporter.innerContainer.Any && WillDumpWhereever)
                    {
                        var radius = 1;

                        while (compTransporter.innerContainer.Count > 0)
                        {
                            var rectToCheck = CellRect.CenteredOn(Position, radius);
                            var cellsToTry = rectToCheck.EdgeCells.Where(vec3 => vec3.InBounds(Map) &&
                                    vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null &&
                                    !vec3.GetThingList(Map).Any(thing =>
                                        thing.def.category == ThingCategory.Item && thing.def.EverHaulable))
                                .InRandomOrder()
                                .ToList();
                            if (cellsToTry.Any())
                            {
                                for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                                {
                                    if (cellsToTry.Count() <= index)
                                    {
                                        break;
                                    }

                                    var thing = compTransporter.innerContainer[index];
                                    compTransporter.innerContainer.TryDrop(thing, cellsToTry[index], Map,
                                        ThingPlaceMode.Direct,
                                        thing.stackCount, out _);
                                }
                            }

                            radius++;
                            if (radius > 10)
                            {
                                break;
                            }
                        }
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
        if (compRefuelable == null || IsInDumpMode || !(compRefuelable.TargetFuelLevel - compRefuelable.Fuel > 1f))
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
        Scribe_Values.Look(ref freeSpots, "freeSpots");
        Scribe_Values.Look(ref storages, "storages");
    }
}