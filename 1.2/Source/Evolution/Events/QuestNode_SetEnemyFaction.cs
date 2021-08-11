using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using RimWorld.QuestGen;

namespace RT_Rimtroid
{
	public class QuestNode_SetEnemyFaction : QuestNode
	{
		public SlateRef<FactionDef> enemyFactionDef;
		protected override bool TestRunInt(Slate slate)
		{
			return TrySetVars(slate);
		}
		protected override void RunInt()
		{
			if (!TrySetVars(QuestGen.slate))
			{
				Log.Error("Could not resolve site parts.");
			}
		}

		private bool TrySetVars(Slate slate)
		{
			var faction = Find.FactionManager.FirstFactionOfDef(enemyFactionDef.GetValue(slate));
			if (faction != null)
            {
				slate.Set<Faction>("enemyFaction", faction);
				return true;
			}
			return false;
		}
	}
}