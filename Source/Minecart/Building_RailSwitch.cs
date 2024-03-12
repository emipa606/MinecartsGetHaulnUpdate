using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Minecart;

public class Building_RailSwitch : Building
{
    public bool AutoSwitch = false;
    public bool Direction = true; //Right is true, Left is false
    public bool DesiredDirection = true;
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.GetFloatMenuOptions(selPawn))
        {
            yield return option;
        }
        if (!wantsFlick()) { yield break; }
        var dir_text = DesiredDirection ? "MGHU.Right".Translate() : "MGHU.Left".Translate();      
        yield return new FloatMenuOption("MGHU.FlickTo".Translate(dir_text), () => selPawn.jobs.StartJob(
            new Job(JobDefOf.Job_SwitchRailDirection,
                new LocalTargetInfo(this))));
    }

    public bool wantsFlick()
    {
        if (Direction == DesiredDirection)
        {
            return false;
        }
        return true;
    }
    public void ToggleDirection()
    {
        Direction = !Direction;
        if (Direction)
        {
            DesiredDirection = true;
        }
        else
        {
            DesiredDirection = false;
        }
        SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(Position, Map, false));
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.Append("MGHU.Turn".Translate());
        sb.Append(Direction ? "MGHU.Right".Translate() : "MGHU.Left".Translate());
        if (wantsFlick())
        {
            sb.Append("MGHU.FlickingTo".Translate(DesiredDirection
                ? "MGHU.Right".Translate()
                : "MGHU.Left".Translate()));
        }
        return sb.ToString();
    }
    // Gizmos
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return new Command_Toggle
        {
            icon = DesiredDirection ? Textures.RailDirectionRight : Textures.RailDirectionLeft,
            defaultLabel = "MGHU.ToggleRailSwitch".Translate(),
            defaultDesc = "MGHU.ToggleRailSwitchTT".Translate(),
            isActive = () => Direction == DesiredDirection,
            toggleAction = delegate { DesiredDirection = !DesiredDirection;}
        };

        yield return new Command_Toggle
            {
                icon = Textures.AutoSwitch_UI,
                defaultLabel = "MGHU.AutoSwitch".Translate(),
                defaultDesc = "MGHU.AutoSwitchDesc".Translate(),
                isActive = () => AutoSwitch,
                toggleAction = delegate { AutoSwitch = !AutoSwitch; }
            };

        if (!Prefs.DevMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            action = () => ToggleDirection(),
            defaultLabel = "Flick Now",
            defaultDesc = "Flick the switch immediately"
        };
    }
}