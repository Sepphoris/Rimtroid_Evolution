using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
	public class JobDriver_GoToQueenToDespawn : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil despawn = new Toil();
			despawn.initAction = delegate
			{
				var queen = this.TargetA.Thing as Queen;
				queen.spawnPool.AbsorbPawn(pawn);
			};
			yield return despawn;
		}
    }
}
