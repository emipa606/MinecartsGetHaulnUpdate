using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Minecart;

[StaticConstructorOnStartup]
public class CompFlickableRailSwitch : CompFlickable
{
    //new private CompProperties_FlickableRailSwitch Props => (CompProperties_FlickableRailSwitch)props;
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        bool IconShouldFaceRight;
        if (SwitchIsOn && !WantsFlick() || !SwitchIsOn && WantsFlick())
        {
            IconShouldFaceRight = true;
        }
        else
        {
            IconShouldFaceRight = false;
        }

        yield return new Command_Toggle
        {
            icon = IconShouldFaceRight ? Textures.RailDirectionRight : Textures.RailDirectionLeft,
            defaultLabel = "MGHU.ToggleRailSwitch".Translate(),
            defaultDesc = "MGHU.ToggleRailSwitchTT".Translate(),
            isActive = () => !WantsFlick(),
            toggleAction = delegate
            {
                wantSwitchOn = !wantSwitchOn;
                FlickUtility.UpdateFlickDesignation(parent);
            }
        };
    }
}