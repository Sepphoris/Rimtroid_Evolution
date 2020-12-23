using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
	public class CompEvolutionTime : ThingComp
	{
		public CompProperties_EvolutionTime Props
		{
			get
			{
				return (CompProperties_EvolutionTime)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.evolutionTick, "evolutionTick", 0, false);
		}

		public Pawn Metroid => this.parent as Pawn;

		public override string CompInspectStringExtra()
		{
			for (var i = 0; i < Metroid.def.race.lifeStageAges.Count; i++)
            {
				Log.Message("Life: " + Metroid.def.race.lifeStageAges[i] + " - " + i);
            }
				
			Log.Message("Metroid.ageTracker.CurLifeStageIndex: " + Metroid.ageTracker.CurLifeStageIndex);
			if (Metroid.ageTracker.CurLifeStageIndex < Metroid.def.race.lifeStageAges.Count - 1)
			{
				float num4 = Metroid.def.race.lifeStageAges[Metroid.ageTracker.CurLifeStageIndex + 1].minAge - Metroid.ageTracker.AgeBiologicalYearsFloat;
				int num5 = (Current.ProgramState == ProgramState.Playing) ? Find.TickManager.TicksGame : 0;
				var nextLifeStageChangeTick = num5 + (long)Mathf.Ceil(num4 * 3600000f);
				return ((int)nextLifeStageChangeTick).ToStringTicksToDays();
			}
			else
			{
				return "Max evolution";
			}
		}

		public int evolutionTick;

		public int rareTicksInAYear = 14400;
	}
}