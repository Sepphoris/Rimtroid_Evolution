using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Core
{
	public class JobDriver_AbsorbEnergyFinal : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				base.Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
				pawn.pather.StopDead();
			};
			toil.tickAction = delegate
			{
				if (job.expiryInterval == -1 && job.def == JobDefOf.Wait_Combat && !pawn.Drafted)
				{
					Log.Error(string.Concat(pawn, " in eternal WaitCombat without being drafted."));
					ReadyForNextToil();
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			yield return toil;
		}
	}
}