using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Minecart;

public class Building_RailSwitch : Building
{
    public bool AutoSwitch = false;

    public void FlickSwitch()
    {
        var flickable = GetComp<CompFlickableRailSwitch>();
        flickable.DoFlick();
        if (flickable.SwitchIsOn == true)
        {
            flickable.wantSwitchOn = true;
        }
        else
        {
            flickable.wantSwitchOn = false;
        }
    }
    public override string GetInspectString()
    {
        var flickable = GetComp<CompFlickableRailSwitch>();
        var sb = new StringBuilder();
        sb.Append("MGHU.Turn".Translate());
        sb.Append(flickable.SwitchIsOn ? "MGHU.Right".Translate() : "MGHU.Left".Translate());
        if (flickable.WantsFlick())
        {
            sb.Append("MGHU.FlickingTo".Translate(flickable.SwitchIsOn
                ? "MGHU.Left".Translate()
                : "MGHU.Right".Translate()));
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
            action = () => FlickSwitch(),
            defaultLabel = "Flick Now",
            defaultDesc = "Flick the switch immediately"
        };
    }
}