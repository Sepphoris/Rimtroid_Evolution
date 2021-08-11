using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace RT_Rimtroid
{
	public class CompProperties_AlphaBomb : CompProperties
	{
        public int spawnMax;
        public CompProperties_AlphaBomb()
		{
			this.compClass = typeof(CompAlphaBomb);
		}
	}

	public class CompAlphaBomb : ThingComp
	{
		public Pawn Metroid => this.parent as Pawn;
		public List<Alpha_Bomb> traps;

        public CompProperties_AlphaBomb Props => (CompProperties_AlphaBomb)props;

        public override string CompInspectStringExtra()
		{
            var trapsCount = traps != null ? traps.Count : 0;
            return "RT_AvailableBombs".Translate(Props.spawnMax - trapsCount, Props.spawnMax);
		}
        public void SpawnTrap(ThingDef def)
        {
            if (traps is null)
                traps = new List<Alpha_Bomb>();

            traps.RemoveAll(t => t.DestroyedOrNull());

            Alpha_Bomb pTrap = ThingMaker.MakeThing(def) as Alpha_Bomb;
            pTrap.parent = Metroid;
            pTrap.SetFactionDirect(Metroid.Faction);

            GenPlace.TryPlaceThing(pTrap, Metroid.Position, Metroid.Map, ThingPlaceMode.Direct);
            traps.Add(pTrap);

            while (traps.Count > Props.spawnMax)
            {
                var trap = traps[0];
                trap.Destroy();
                traps.Remove(trap);
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
                    traps = new List<Alpha_Bomb>();
                }
            }
        }

    }
}
