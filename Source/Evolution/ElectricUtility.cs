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

		public static float ChanceToStartElectricIn(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			float num = (!c.TerrainFlammableNow(map)) ? 0f : c.GetTerrain(map).GetStatValueAbstract(StatDefOf.Flammability, null);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is Fire)
				{
					return 0f;
				}
				if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
				{
					num = Mathf.Max(num, thing.GetStatValue(StatDefOf.Flammability, true));
				}
			}
			if (num > 0f)
			{
				Building edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
				{
					return 0f;
				}
				List<Thing> thingList2 = c.GetThingList(map);
				for (int j = 0; j < thingList2.Count; j++)
				{
					if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
					{
						return 0f;
					}
				}
			}
			return num;
		}

		public static bool TryStartElectricIn(IntVec3 c, Map map, float ElectricSize)
		{
			float num = ElectricUtility.ChanceToStartElectricIn(c, map);
			if (num <= 0f)
			{
				return false;
			}
			Log.Message("Making RT_Electricity in ElectricUtility.");
			electricDischarge electricDischarge = (electricDischarge)ThingMaker.MakeThing(RT_DefOf.RT_Electricity, null);
			electricDischarge.ElectricSize = ElectricSize;
			GenSpawn.Spawn(electricDischarge, c, map, Rot4.North, WipeMode.Vanish, false);
			return true;
		}

	}
}
