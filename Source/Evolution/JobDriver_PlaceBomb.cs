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
				pawn.stances.SetStance(new Stance_Warmup(1000, null, job.ability.verb));
				Log.Message("ability.verb.WarmingUp: " + job.ability.verb.WarmingUp);
			};
			yield return initWarmup;
			Toil placeBomb = new Toil();
			placeBomb.tickAction = delegate
			{
				if (!(pawn.stances.curStance is Stance_Warmup))
				{
					var ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("RT_AlphaBomb"));
					ability.StartCooldown(1000);
					var alphaBombComp = this.pawn.TryGetComp<CompAlphaBomb>();
					alphaBombComp.SpawnTrap(RT_DefOf.RT_MetroidBomb);
					ReadyForNextToil();
				}
			};
			placeBomb.defaultCompleteMode = ToilCompleteMode.Never;
			yield return placeBomb;
		}

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref placingWorkDone, "placingWorkDone");
        }
    }
}
