using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace RT_Core
{
	public class ThoughtWorker_FoodTarget : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.IsPrisoner && (p.GetComp<CompPrisonerFeed>()?.canBeEaten ?? false);
		}
	}
}

