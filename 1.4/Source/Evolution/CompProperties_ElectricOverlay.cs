using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{
	public class CompProperties_ElectricOverlay : CompProperties
	{
		public float ElectricSize = 1f;

		public Vector3 offset;

		public CompProperties_ElectricOverlay()
		{
			this.compClass = typeof(CompElectricOverlay);
		}

		public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
		{
			Graphic graphic = GhostUtility.GhostGraphicFor(CompElectricOverlay.ElectricGraphic, thingDef, ghostCol);
			graphic.DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0f);
		}
	}
}