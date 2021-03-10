using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace RT_Rimtroid
{
	public class CompProperties_AlphaBomb : CompProperties
	{
		public CompProperties_AlphaBomb()
		{
			this.compClass = typeof(CompAlphaBomb);
		}
	}

	public class CompAlphaBomb : ThingComp
	{
		public Pawn Metroid => this.parent as Pawn;
		public List<Thing> traps;
        public static int spawnMax = 3;
		public override string CompInspectStringExtra()
		{
            return "RT_AvailableBombs".Translate(traps != null ? spawnMax - traps.Count : spawnMax);
		}
        public void SpawnTrap(ThingDef def)
        {
            if (traps is null)
                traps = new List<Thing>();

            traps.RemoveAll(t => t.DestroyedOrNull());

            Thing pTrap = ThingMaker.MakeThing(def);
            pTrap.SetFactionDirect(Metroid.Faction);

            GenPlace.TryPlaceThing(pTrap, Metroid.Position, Metroid.Map, ThingPlaceMode.Near);
            traps.Add(pTrap);

            while (traps.Count > spawnMax)
            {
                traps[0].Destroy();
                traps.RemoveAt(0);
                Messages.Message("Oldest bomb replaced", MessageTypeDefOf.NeutralEvent);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref traps, "trapsSpawned", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (traps == null)
                {
                    traps = new List<Thing>();
                }
            }
        }

    }
}
