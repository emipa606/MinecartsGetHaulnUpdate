using System.Text;
using RimWorld;
using Verse;

namespace Minecart;

public class Building_RailSwitch : Building
{
    public override string GetInspectString()
    {
        var flickable = GetComp<CompFlickable>();
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
}