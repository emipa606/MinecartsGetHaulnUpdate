using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Minecart;

public class Building_Minecart : Building
{
    private bool autoLaunch;

    private CompTransporter cartTransporter;
    private bool isLoading;
    private Building_Minecart leadingMinecart;
    private float recursiveMass = 1;

    private float speed;

    // Public values
    private float subtile;
    private Building_Minecart trailingMinecart;

    public CompTransporter CartTransporter
    {
        get
        {
            cartTransporter ??= GetComp<CompTransporter>();

            return cartTransporter;
        }
    }

    public float Subtile
    {
        get => HeadMinecart.subtile;
        set => HeadMinecart.subtile = value;
    }

    public float Speed
    {
        get => HeadMinecart.speed;
        private set => HeadMinecart.speed = value;
    }

    // Queriable
    private Building_Minecart HeadMinecart => leadingMinecart == null ? this : leadingMinecart.HeadMinecart;

    public List<Building_Minecart> Train
    {
        get
        {
            var minecarts = new List<Building_Minecart> { HeadMinecart };
            while (true)
            {
                if (minecarts.Last().trailingMinecart == null)
                {
                    break;
                }

                minecarts.Add(minecarts.Last().trailingMinecart);
            }

            return minecarts;
        }
    }

    // Miscellaneous overrides
    private ThingDef_Minecart Def => def as BuildableDef as ThingDef_Minecart;

    public override Vector3 DrawPos => (Rotation.FacingCell.ToVector3() * subtile) + this.TrueCenter();

    private IntVec3 Forward => Position + Rotation.FacingCell;
    private IntVec3 Right => Position + Rotation.Rotated(RotationDirection.Clockwise).FacingCell;
    private IntVec3 Left => Position + Rotation.Rotated(RotationDirection.Counterclockwise).FacingCell;

    //public override void DrawAt(Vector3 drawLoc, bool flip = false)
    //{
    //    DrawAt(drawLoc);
    //    base.DrawAt(drawLoc, flip);
    //    base.Draw();
    //}

    public override void Print(SectionLayer layer)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref autoLaunch, "autoLaunch");
        Scribe_Values.Look(ref isLoading, "isLoading");
        Scribe_Values.Look(ref speed, "speed");
        Scribe_Values.Look(ref subtile, "subtile");
        Scribe_References.Look(ref trailingMinecart, "trailingMinecart");
        Scribe_References.Look(ref leadingMinecart, "leadingMinecart");
    }

    // Update trailing minecart method
    private float UpdateTrailingMinecart()
    {
        if (Spawned)
        {
            if (trailingMinecart is { Spawned: true })
            {
                trailingMinecart.speed = speed;
                trailingMinecart.subtile = subtile;
                recursiveMass = trailingMinecart.UpdateTrailingMinecart() + this.GetStatValue(StatDefOf.Mass);
            }
            else
            {
                recursiveMass = this.GetStatValue(StatDefOf.Mass);
            }

            if (leadingMinecart is { Spawned: true })
            {
                var position = leadingMinecart.Position;
                var rotation = leadingMinecart.Rotation;
                if (Forward.GetFirstThing<Building_Minecart>(Map) == null || !leadingMinecart.Spawned)
                {
                    leadingMinecart.trailingMinecart = null;
                    leadingMinecart = null;
                }

                if (!(subtile > 1))
                {
                    return recursiveMass;
                }

                Position = position;
                Rotation = rotation;
                subtile--;
            }
            else
            {
                leadingMinecart = null;
            }
        }
        else
        {
            if (trailingMinecart is { Spawned: false })
            {
                trailingMinecart.speed = speed;
                trailingMinecart.subtile = subtile;
                recursiveMass = trailingMinecart.UpdateTrailingMinecart() + this.GetStatValue(StatDefOf.Mass);
            }
            else
            {
                recursiveMass = this.GetStatValue(StatDefOf.Mass);
            }
        }

        return recursiveMass;
    }

    // Gizmos
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (leadingMinecart == null)
        {
            yield return new Command_Toggle
            {
                icon = CompLaunchable.LaunchCommandTex,
                defaultLabel = "MGHU.AutoLaunch".Translate(),
                defaultDesc = "MGHU.AutoLaunchDesc".Translate(),
                isActive = () => autoLaunch,
                toggleAction = delegate { autoLaunch = !autoLaunch; }
            };
        }

        if (!Prefs.DevMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            action = () => HeadMinecart.Launch(),
            defaultLabel = "MGHU.Launch".Translate(),
            defaultDesc = "MGHU.LaunchTT".Translate()
        };
    }

    // Float menu
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.GetFloatMenuOptions(selPawn))
        {
            yield return option;
        }

        yield return new FloatMenuOption("MGHU.LaunchTT".Translate(), () => selPawn.jobs.StartJob(
            new Job(DefOfs.LaunchCart,
                new LocalTargetInfo(this))));
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(base.GetInspectString());

        sb.AppendLine("MGHU.Speed".Translate(Speed.ToStringWithSign("0.00'c'")));
        sb.Append("MGHU.Mass".Translate(this.GetStatValue(StatDefOf.Mass).ToString("0.00'kg'"),
            recursiveMass.ToString("0.00'kg'")));

        return sb.ToString();
    }


    // Spawn setup
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (respawningAfterLoad)
        {
            return;
        }

        var minecart = Forward.GetFirstThing<Building_Minecart>(map);
        if (minecart != null)
        {
            leadingMinecart = minecart;
            minecart.trailingMinecart = this;
        }

        minecart = (Position + Rotation.Opposite.FacingCell).GetFirstThing<Building_Minecart>(map);
        if (minecart == null)
        {
            return;
        }

        trailingMinecart = minecart;
        minecart.leadingMinecart = this;
    }

    // Tick
    public override void Tick()
    {
        base.Tick();

        if (Spawned)
        {
            if (leadingMinecart == null)
            {
                if (isLoading)
                {
                    if (!CartTransporter.LoadingInProgressOrReadyToLaunch ||
                        !CartTransporter.AnyInGroupHasAnythingLeftToLoad)
                    {
                        isLoading = false;
                        if (autoLaunch)
                        {
                            Launch();
                        }
                    }
                }
                else
                {
                    if (CartTransporter.LoadingInProgressOrReadyToLaunch &&
                        CartTransporter.AnyInGroupHasAnythingLeftToLoad)
                    {
                        isLoading = true;
                    }
                }

                if (IsClear(Forward))
                {
                    subtile += speed / GenTicks.TicksPerRealSecond;
                    speed *= Def.frictionCoef;
                }
                else if (IsClear(Right, true) && !IsClear(Left, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                }
                else if (IsClear(Left, true) && !IsClear(Right, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                }
                else
                {
                    speed = 0f;
                    subtile = 0f;
                }

                UpdateTrailingMinecart();

                if (!(subtile > 1))
                {
                    return;
                }

                DoRailStep();

                subtile--;
            }
            else
            {
                if (leadingMinecart.Spawned)
                {
                    return;
                }

                leadingMinecart.trailingMinecart = null;
                leadingMinecart = null;
            }
        }
        else
        {
            if (leadingMinecart != null)
            {
                return;
            }

            speed *= Def.frictionCoef;
            UpdateTrailingMinecart();
        }
    }

    // Clearance
    private bool IsClear(IntVec3 cell, bool ignoreMinecarts = false, bool ignoreRails = false)
    {
        return IsClear(cell, Map, Def, ignoreMinecarts, ignoreRails);
    }

    public static bool IsClear(IntVec3 cell, Map map, ThingDef_Minecart def, bool ignoreMinecarts = false,
        bool ignoreRails = false)
    {
        if (!cell.Standable(map) && (!ignoreMinecarts || cell.GetFirstThing<Building_Minecart>(map) == null))
        {
            return false;
        }

        if (cell.GetDoor(map) is not { } door || door.Open)
        {
            return cell.GetFirstThing(map, def.railDef) != null ||
                   cell.GetFirstThing(map, def.railPoweredDef) != null || ignoreRails;
        }

        if (door.DoorPowerOn && !door.IsForbidden(Faction.OfPlayerSilentFail))
        {
            door.openInt = true;
            door.lastFriendlyTouchTick = Find.TickManager.TicksGame;
        }
        else
        {
            return false;
        }

        return cell.GetFirstThing(map, def.railDef) != null ||
               cell.GetFirstThing(map, def.railPoweredDef) != null || ignoreRails;
    }

    // Summary:
    //  Applies a force of the given paramaters into the minecart's physics model
    public void Push(float pushSpeed,
        float pushAccel, float pushPower)
    {
        Speed = pushSpeed + ((Speed - pushSpeed) * (1 - (pushAccel / (1 + (recursiveMass / pushPower)))));
    }

    //Summary:
    //  Instantly sets the minecarts speed to the default launch speed
    public void Launch()
    {
        if (trailingMinecart == null && leadingMinecart == null && !IsClear(Position + Rotation.FacingCell))
        {
            Rotation = Rotation.Opposite;
        }

        Speed = Def.launchSpeed;
    }

    public void Brake(float pushAccel, float pushPower)
    {
        Speed *= 1 - (pushAccel / (1 + (recursiveMass / pushPower)));
    }

    // Summary:
    //  Instantly sets the minecarts speed to the parameters launch speed
    public void Launch(float launchSpeed)
    {
        if (trailingMinecart == null && leadingMinecart == null && !IsClear(Position + Rotation.FacingCell))
        {
            Rotation = Rotation.Opposite;
        }

        speed = launchSpeed;
    }

    // Rail step
    private void DoRailStep()
    {
        foreach (var thing in Forward.GetThingList(Map).ToList())
        {
            if (thing.Faction.HostileTo(Faction))
            {
                thing.TakeDamage(new DamageInfo(DamageDefOf.Blunt,
                    Speed * recursiveMass, instigator: this));
            }
        }

        if (IsClear(Forward, true))
        {
            Position = Forward;
        }

        var railSwitch = Position.GetFirstThing<Building_RailSwitch>(Map);

        if (railSwitch != null)
        {
            if (railSwitch.GetComp<CompFlickable>().SwitchIsOn)
            {
                if (IsClear(Right, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                }
                else if (IsClear(Forward, true))
                {
                }
                else if (IsClear(Left, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                }
            }
            else
            {
                if (IsClear(Left, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                }
                else if (IsClear(Forward, true))
                {
                }
                else if (IsClear(Right, true))
                {
                    Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                }
            }

            if (railSwitch.AutoSwitch)
            {
                railSwitch.FlickSwitch();
            }
        }
        else
        {
            if (IsClear(Forward, true))
            {
            }
            else if (IsClear(Right, true) && !IsClear(Left, true))
            {
                Rotation = Rotation.Rotated(RotationDirection.Clockwise);
            }
            else if (IsClear(Left, true) && !IsClear(Right, true))
            {
                Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
            }
        }
    }
}