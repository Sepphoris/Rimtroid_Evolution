using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
	public class JobDriver_PlaceProximityBomb : JobDriver_PlaceBomb
    {
        protected override ThingDef BombToPlace => RT_DefOf.RT_MetroidBomb;
    }
	public class JobDriver_PlaceTimedBomb : JobDriver_PlaceBomb
	{
		protected override ThingDef BombToPlace => RT_DefOf.RT_MetroidBombTimed;
	}
	public class JobDriver_PlaceHostileOnlyBomb : JobDriver_PlaceBomb
	{
		protected override ThingDef BombToPlace => RT_DefOf.RT_MetroidBombHostileOnly;
	}
	public class JobDriver_PlaceBomb : JobDriver
	{
		private float placingWorkDone;

		protected virtual ThingDef BombToPlace { get; }
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil placeBomb = new Toil();
			placeBomb.initAction = delegate
			{
				placingWorkDone = 0f;
			};
			placeBomb.tickAction = delegate
			{
				placingWorkDone += 1f;
				if (placingWorkDone > 60)
				{
					var alphaBombComp = this.pawn.TryGetComp<CompAlphaBomb>();
					alphaBombComp.SpawnTrap(BombToPlace);
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
