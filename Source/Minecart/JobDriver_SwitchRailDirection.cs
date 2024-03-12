using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Minecart
{
    public class WorkGiver_SwitchRailDirection : WorkGiver_Scanner
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)

        {
            return new Job(JobDefOf.Job_SwitchRailDirection, new LocalTargetInfo(t));
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            IEnumerable<Building> switchList = pawn.Map.listerBuildings.allBuildingsColonist.Where(building => building is Building_RailSwitch) ;
            foreach (Building_RailSwitch rswitch in switchList)
            {
                if (rswitch.wantsFlick() && ReservationUtility.CanReserve(pawn, rswitch))
                {
                    yield return rswitch;
                }
            }
        }
    }

    internal class JobDriver_SwitchRailDirection : JobDriver_Flick
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(delegate
            {
                var thing = TargetThingA as Building_RailSwitch;
                return thing.wantsFlick() ? false : true;
            });
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Do(() => {
                (TargetA.Thing as Building_RailSwitch)?.ToggleDirection();
            });
        }
    }
}



