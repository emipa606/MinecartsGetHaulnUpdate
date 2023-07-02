using System.Linq;
using RimWorld;
using Verse;

namespace Minecart;

// Stored mass stat
public class StatPart_StoredMass : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        var minecart = req.Thing as Building_Minecart;
        var compTransporter = minecart?.GetComp<CompTransporter>();
        if (compTransporter != null)
        {
            return "MGHU.TotalMass".Translate(compTransporter.innerContainer
                .Sum(t => t.GetStatValue(StatDefOf.Mass) * t.stackCount).ToStringWithSign());
        }

        return "";
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        var minecart = req.Thing as Building_Minecart;
        var compTransporter = minecart?.GetComp<CompTransporter>();
        if (compTransporter != null)
        {
            val += compTransporter.innerContainer.Sum(t => t.GetStatValue(StatDefOf.Mass) * t.stackCount);
        }
    }
}