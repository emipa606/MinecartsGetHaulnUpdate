using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Minecart;

public class JobDriver_LaunchCart : JobDriver_Flick
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.WaitWith(TargetIndex.A, 20, true, true);
        yield return Toils_General.Do(() => { (TargetA.Thing as Building_Minecart)?.Launch(); });
    }
}