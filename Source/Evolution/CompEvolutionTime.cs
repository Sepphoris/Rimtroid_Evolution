using System;
using RimWorld;
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


		public override string CompInspectStringExtra()
		{
			int num = (int)(((float)this.rareTicksInAYear * this.Props.timeInYears - (float)this.evolutionTick) * 250f);
			return TranslatorFormattedStringExtensions.Translate(this.Props.reportString, GenDate.ToStringTicksToPeriod(num, false, false, false, true));
		}

		public int evolutionTick;

		public int rareTicksInAYear = 14400;
	}
}