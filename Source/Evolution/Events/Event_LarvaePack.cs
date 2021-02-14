using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace RT_Rimtroid
{
    public class Event_LarvaePack : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDef.Named("RT_MetroidLarvae")) && this.TryFindEntryCell(map, out intVec);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            if (!this.TryFindEntryCell(map, out intVec))
            {
                return false;
            }
            PawnKindDef metroidlarvae = PawnKindDef.Named("RT_MetroidLarvae");

            float num = StorytellerUtility.DefaultThreatPointsNow(map) / 4;
            int num2 = GenMath.RoundRandom(num / metroidlarvae.combatPower);
            int min = Rand.RangeInclusive(3, 4);
            int max = Rand.RangeInclusive(4, 9);
            num2 = Mathf.Clamp(num2, 1, max);
            int num3 = Rand.RangeInclusive(150000, 210000);
            IntVec3 invalid = IntVec3.Invalid;
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out invalid))
            {
                invalid = IntVec3.Invalid;
            }
            Pawn pawn = null;
            for (int i = 0; i < num2; i++)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
                pawn = PawnGenerator.GeneratePawn(metroidlarvae, null);
                GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num3;
                if (invalid.IsValid)
                {
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10, null);
                }
            }
            Find.LetterStack.ReceiveLetter("LetterLabelLarvaePack".Translate(metroidlarvae.label.CapitalizeFirst()), "LetterLarvaePack".Translate(metroidlarvae.label), LetterDefOf.ThreatBig, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
        }
    }
}