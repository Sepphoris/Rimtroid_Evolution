using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RT_Rimtroid
{
	public static class Utils
	{
		public static void MakeFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
		{
			Job job = null;
			IntVec3 intVec;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
			{
				intVec = pawn.CurJob.targetA.Cell;
			}
			else
			{
				intVec = CellFinderLoose.GetFleeDest(pawn, dangers, 24f);
			}

			if (intVec == pawn.Position)
			{
				intVec = GenRadial.RadialCellsAround(pawn.Position, radius <= 50 ? radius : 50, radius * 2 <= 50 ? radius * 2 : 50).RandomElement();
			}
			if (intVec != pawn.Position)
			{
				job = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
			}
			if (job != null)
			{
				//Log.Message(pawn + " flee");
				pawn.jobs.TryTakeOrderedJob(job);
			}
		}
	}

	[DefOf]
    public static class RT_RimtroidDefOf
    {
		public static ThingDef RT_BanteeMetroid;
		public static ThingDef RT_MetroidLarvae;
        public static ThingDef RT_AlphaMetroid;
		public static ThingDef RT_GammaMetroid;
		public static ThingDef RT_OmegaMetroid;
		public static ThingDef RT_QueenMetroid;
		public static ThingDef RT_ZetaMetroid;

		public static FactionDef RT_Metroids;

		//public static ThingDef RT_GammaShockwaveMote;

		public static ThingDef RT_Electricity;
		public static ThingDef RT_ExplosionTest;

		public static SoundDef RT_ElectricBurning;

		public static HediffDef RT_SedativeBuildup;

		public static ThingDef RT_ProtusSphere;

		public static JobDef RT_PlaceAlphaBomb;
		public static ThingDef RT_MetroidBomb;

		public static RulePackDef RT_QueenNames;
		//public static IncidentDef RT_QueenSpotted;
		public static DutyDef RT_FollowQueen;
		public static JobDef RT_GoToQueenToDespawn;
		public static HediffDef RT_HealingBonus;

		public static JobDef RT_EatFromStation;
		public static JobDef RT_AbsorbingEnergy;


		public static ThingDef RT_FeedingStationSE;
		public static ThingDef RT_FeedingStationSF;
		public static ThingDef RT_FeedingStationLF;
		public static ThingDef RT_FeedingStationLE;

		public static HediffDef RT_LifeDrainSickness;
		public static HediffDef RT_LatchedMetroid;

		public static JobDef RT_AbsorbEnergyFinal;
		public static HediffDef RT_MetroidAbsorbEnergy;
		public static ThingDef RT_ProcessingEnergyMote;
		public static ThoughtDef RT_FeedOn;
	}
}
