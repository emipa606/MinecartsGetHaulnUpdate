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

    private bool IsInDumpMode
    {
        get => mode;
        set => mode = value;
    }

    private bool WillDumpWhereever
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

    private bool WillDumpOnFreeSpots
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

    private bool WillDumpInStorages
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
                            .CenteredOn(Position, MinecartMod.Instance.Settings.StorageRange).Where(vec3 =>
                                vec3.InBounds(Map) && vec3 != Position &&
                                vec3.GetRoom(Map) == currentRoom &&
                                vec3.GetFirstThing(Map, DefOfs.ThingRail) == null &&
                                vec3.GetFirstThing(Map, DefOfs.ThingPoweredRail) == null));
                        foreach (var validCell in validCells.ToList())
                        {
                            var buildings = validCell.GetThingList(Map).OfType<Building>().ToList();
                            foreach (var building in buildings)
                            {
                                if (building is Building_Storage bstorage)
                                {
                                    validCells.AddRange(bstorage.AllSlotCells());
                                }
                            }

                            if (validCell.GetZone(Map) is Zone_Stockpile stockpile)
                            {
                                validCells.AddRange(stockpile.Cells.Where(vec3 =>
                                    vec3.GetFirstThing(Map, DefOfs.ThingRail) == null &&
                                    vec3.GetFirstThing(Map, DefOfs.ThingPoweredRail) == null));
                            }
                        }

                        for (var i = 0; i < compTransporter.innerContainer.Count; i++)
                        {
                            var thing = compTransporter.innerContainer.GetAt(i);
                            foreach (var validStorageCell in
                                     validCells.Where(vec3 => vec3.IsValidStorageFor(Map, thing)))
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
                            .CenteredOn(Position, MinecartMod.Instance.Settings.FreeSpaceRange).Where(vec3 =>
                                vec3.InBounds(Map) && vec3 != Position &&
                                vec3.GetRoom(Map) == currentRoom &&
                                vec3.GetFirstThing(Map, DefOfs.ThingRail) == null &&
                                vec3.GetFirstThing(Map, DefOfs.ThingPoweredRail) == null));

                        var emptyCells = validCells.Where(vec3 => !vec3.GetThingList(Map).Any(thing =>
                                                                      thing.def.category == ThingCategory.Item &&
                                                                      !thing.def.EverHaulable) &&
                                                                  vec3.GetFirstBuilding(Map) == null &&
                                                                  vec3.GetZone(Map)?.GetType() !=
                                                                  typeof(Zone_Stockpile)).ToList();

                        if (emptyCells.Any())
                        {
                            for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                            {
                                foreach (var emptyCell in emptyCells)
                                {
                                    var thing = compTransporter.innerContainer.GetAt(index);
                                    var roomleft = emptyCell.GetItemStackSpaceLeftFor(Map, thing.def);
                                    if (roomleft <= 0)
                                    {
                                        continue;
                                    }

                                    var amountToPlace = thing.stackCount < roomleft ? thing.stackCount : roomleft;

                                    if (compTransporter.innerContainer.TryDrop(thing, emptyCell, Map,
                                            ThingPlaceMode.Direct, amountToPlace, out _))
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
                                    vec3.GetFirstThing(Map, DefOfs.ThingRail) == null &&
                                    vec3.GetFirstThing(Map, DefOfs.ThingPoweredRail) == null &&
                                    (vec3.GetFirstBuilding(Map) == null ||
                                     vec3.GetFirstBuilding(Map).CanBeSeenOver()) &&
                                    vec3.GetRoom(Map) == currentRoom)
                                .InRandomOrder()
                                .ToList();


                            if (cellsToTry.Any())
                            {
                                foreach (var cellToTry in cellsToTry)
                                {
                                    for (var index = 0; index < compTransporter.innerContainer.Count; index++)
                                    {
                                        var thing = compTransporter.innerContainer.GetAt(index);
                                        var roomleft = cellToTry.GetItemStackSpaceLeftFor(Map, thing.def);
                                        if (roomleft <= 0)
                                        {
                                            continue;
                                        }

                                        var amountToPlace = thing.stackCount <= roomleft ? thing.stackCount : roomleft;
                                        if (compTransporter.innerContainer.TryDrop(
                                                compTransporter.innerContainer.GetAt(index), cellToTry, Map,
                                                ThingPlaceMode.Direct,
                                                amountToPlace,
                                                out _))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            radius++;
                            if (radius <= MinecartMod.Instance.Settings.DropAllRange)
                            {
                                continue;
                            }

                            break;
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
                //Is in load mode
                var allCells = new HashSet<IntVec3>(CellRect.CenteredOn(Position, 1));
                var validCells = new HashSet<IntVec3>();
                foreach (var cell in allCells.ToList())
                {
                    var buildings = cell.GetThingList(Map).OfType<Building>().ToList();
                    foreach (var building in buildings)
                    {
                        if (building is Building_Storage storage)
                        {
                            validCells.AddRange(storage.AllSlotCells());
                        }
                    }

                    if (cell.GetZone(Map) is Zone_Stockpile)
                    {
                        validCells.Add(cell);
                    }
                }

                foreach (var cell in validCells)
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