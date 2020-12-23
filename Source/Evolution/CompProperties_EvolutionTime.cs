using RimWorld;
using Verse;
using System;

namespace RT_Rimtroid
{
	public class CompProperties_EvolutionTime : CompProperties

	{
		public CompProperties_EvolutionTime()
		{
			this.compClass = typeof(CompEvolutionTime);
		}

		public float timeInYears;
		
		public string reportString = "RT_TimeToEvolution";
	}
}
