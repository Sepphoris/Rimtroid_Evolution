using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace RT_Rimtroid
{
    public class Ability_MetroidBomb : RT_Core.Ability_Base
    {
        private List<Thing> traps = new List<Thing>();

        private int spawnMax = 6;

        private IntRange spawnCount = new IntRange(1, 1);

        public Ability_MetroidBomb(Pawn pawn) : base(pawn) { }
        public Ability_MetroidBomb(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamed("RT_MetroidBomb");
            int count = spawnCount.RandomInRange;
            for (int c = 0; c < count; c++)
            {
                SpawnTrap(def);
            }

            return base.Activate(target, dest);
        }

        private void SpawnTrap(ThingDef def)
        {
            traps.RemoveAll(t => t.DestroyedOrNull());

            Thing pTrap = ThingMaker.MakeThing(def);
            pTrap.SetFactionDirect(pawn.Faction);

            GenPlace.TryPlaceThing(pTrap, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            traps.Add(pTrap);

            while (traps.Count >= spawnMax)
            {
                traps[0].Destroy();
                traps.RemoveAt(0);
                Messages.Message("Oldest bomb replaced", MessageTypeDefOf.NeutralEvent);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

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