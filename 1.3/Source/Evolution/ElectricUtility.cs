using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Rimtroid
{
	public static class ElectricUtility
	{
		public static bool TryStartElectricIn(IntVec3 c, Map map, float ElectricSize)
		{
			Log.Message("Making RT_Electricity in ElectricUtility.");
			ElectricDischarge electricDischarge = (ElectricDischarge)ThingMaker.MakeThing(RT_RimtroidDefOf.RT_Electricity, null);
			electricDischarge.ElectricSize = ElectricSize;
			GenSpawn.Spawn(electricDischarge, c, map, Rot4.North, WipeMode.Vanish, false);
			return true;
		}

	}
}
