using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{
	[StaticConstructorOnStartup]
	public class CompElectricOverlay : ThingComp
	{
		public static readonly Graphic ElectricGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Electricity", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

		public CompProperties_ElectricOverlay Props
		{
			get
			{
				return (CompProperties_ElectricOverlay)this.props;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 drawPos = this.parent.DrawPos;
			drawPos.y += 0.046875f;
			CompElectricOverlay.ElectricGraphic.Draw(drawPos, Rot4.North, this.parent, 0f);
		}
		public override void CompTick()
		{
			base.CompTick();
			List<Thing> thingsInRange = this.parent.Position.GetThingList(this.parent.Map);
			if (!thingsInRange.NullOrEmpty())
			{
				foreach (Thing thing in thingsInRange.Where(thing => thing != this.parent))
				{
					thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 6f));
					if (thing is Pawn pawn && Rand.Chance(0.5f))
					{
						pawn.stances.stunner.StunFor(150, this.parent);
					}
				}
			}
		}
	}
}