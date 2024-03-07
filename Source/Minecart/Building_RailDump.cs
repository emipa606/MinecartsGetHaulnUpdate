using System;
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
                    var currentRoom = Position.GetRoom(Map);
                    if (WillDumpInStorages)

                    {
                        var validCells = new HashSet<IntVec3>(CellRect
                            .CenteredOn(Position, MinecartMod.instance.Settings.StorageRange).Where(vec3 =>
                                vec3.InBounds(Map) && vec3 != Position &&
                                vec3.GetRoom(Map) == currentRoom &&
                                vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null));
                        foreach (var validCell in validCells.ToList())
                        {
                            if (validCell.GetFirstBuilding(Map) is Building_Storage storage)
                            {
                                validCells.AddRange(storage.AllSlotCells());
                                continue;
                            }

                            if (validCell.GetZone(Map) is Zone_Stockpile stockpile)
                            {
                                validCells.AddRange(stockpile.Cells.Where(vec3 =>
                                    vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null));
                            }
                        }

                        for (var i = 0; i < compTransporter.innerContainer.Count; i++)
                        {
                            var thing = compTransporter.innerContainer.GetAt(i);
                            foreach (var validStorageCell in validCells.Where(
                                         vec3 => vec3.IsValidStorageFor(Map, thing)))
                            {
                                if (compTransporter.innerContainer.TryDrop(thing, validStorageCell, Map,
                                        ThingPlaceMode.Direct, thing.stackCount, out _))
                                {
                                    break;
                                }
                            }
                        }
                    }


                    if (WillDumpOnFreeSpots || WillDumpWhereever)
                    {
                        var validCells = new HashSet<IntVec3>(CellRect
                            .CenteredOn(Position, MinecartMod.instance.Settings.FreeSpaceRange).Where(vec3 =>
                                vec3.InBounds(Map) && vec3 != Position &&
                                vec3.GetRoom(Map) == currentRoom &&
                                vec3.GetFirstThing(Map, ThingDefOf.ThingRail) == null));

                        var emptyCells = validCells.Where(vec3 => !vec3.GetThingList(Map).Any(thing =>
                                                                      thing.def.category == ThingCategory.Item &&
                                                                      !thing.def.EverHaulable) &&
                                                                  vec3.GetFirstBuilding(Map) == null &&
                                                                  vec3.GetZone(Map)?.GetType() !=
                                                                  typeof(Zone_Stockpile)).ToList();

                        Main.LogMessage($"RailDump Empty cells: {emptyCells.Count}");

                        if (emptyCells.Any())
                        {
                            for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                            {
                                foreach (var emptyCell in emptyCells)
                                {
                                    var thing = compTransporter.innerContainer.GetAt(index);
                                    if (compTransporter.innerContainer.TryDrop(thing, emptyCell, Map,
                                            ThingPlaceMode.Direct, thing.stackCount, out _))
                                    {
                                        break;
                                    }
                                }
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
                                    (vec3.GetFirstBuilding(Map) == null ||
                                     vec3.GetFirstBuilding(Map).CanBeSeenOver()) &&
                                    vec3.GetRoom(Map) == currentRoom)
                                .InRandomOrder()
                                .ToList();
                            Main.LogMessage($"RailDump tries dumping into {cellsToTry.Count} cells at radius {radius}");

                            if (cellsToTry.Any())
                            {
                                for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                                {
                                    foreach (var cellToTry in cellsToTry)
                                    {
                                        if (compTransporter.innerContainer.TryDrop(
                                                compTransporter.innerContainer.GetAt(index), cellToTry, Map,
                                                ThingPlaceMode.Direct,
                                                compTransporter.innerContainer.GetAt(index).stackCount,
                                                out _))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            radius++;

                            if (radius > MinecartMod.instance.Settings.DropAllRange)
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
                    var currentMassLeft = compTransporter.Props.massCapacity -
                                          compTransporter.innerContainer.Sum(t =>
                                              t.GetStatValue(StatDefOf.Mass) * t.stackCount);

                    if (currentMassLeft <= 0)
                    {
                        break;
                    }

                    var thing = cell.GetFirstItem(Map);
                    if (thing == null || !thing.IsInValidStorage())
                    {
                        continue;
                    }

                    var massPerItem = thing.GetStatValue(StatDefOf.Mass);
                    if (massPerItem == 0)
                    {
                        continue;
                    }

                    var maxStack = (int)Math.Min(Math.Floor(currentMassLeft / massPerItem), thing.stackCount);
                    if (maxStack == 0)
                    {
                        continue;
                    }

                    compTransporter.innerContainer.TryAdd(thing.SplitOff(maxStack));
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