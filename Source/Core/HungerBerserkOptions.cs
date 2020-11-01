using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
	public class HungerBerserkOptions : DefModExtension
	{
		public Dictionary<float, float> hungerBerserkChanges;
		public float chanceToBecomeWildIfBerserkAndTamed;
	}
}
