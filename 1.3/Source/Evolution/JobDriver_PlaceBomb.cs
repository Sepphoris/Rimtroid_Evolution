using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
	public class JobDriver_PlaceAlphaBomb : JobDriver
	{
		private float placingWorkDone;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil initWarmup = new Toil();
			initWarmup.initAction = delegate
			{
				placingWorkDone = 0;
				//pawn.stances.SetStance(new Stance_Warmup(60, null, job.ability.verb));
			};
			yield return initWarmup;
			Toil placeBomb = new Toil();
			placeBomb.tickAction = delegate
			{
				placingWorkDone++;
				if (placingWorkDone > 60) //!(pawn.stances.curStance is Stance_Warmup))
				{
					job.ability.StartCooldown(1000);
					var alphaBombComp = this.pawn.TryGetComp<CompAlphaBomb>();
					alphaBombComp.SpawnTrap(RT_DefOf.RT_MetroidBomb);
					ReadyForNextToil();
				}
			};
			placeBomb.defaultCompleteMode = ToilCompleteMode.Never;
			placeBomb.WithProgressBar(TargetIndex.A, () => placingWorkDone / 60, interpolateBetweenActorAndTarget: true);
			yield return placeBomb;
		}

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref placingWorkDone, "placingWorkDone");
        }
    }
}
