using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
	[StaticConstructorOnStartup]
	public static class AddPrisonerComp
	{
		static AddPrisonerComp()
		{
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDef.race?.Humanlike ?? false)
				{
					thingDef.comps.Add(new CompProperties_PrisonerFeed());
				}
			}
		}
	}
}
