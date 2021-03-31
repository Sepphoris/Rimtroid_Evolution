using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace RT_Rimtroid
{
    public class IncidentWorker_OmegaBecomesQueen : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var metroidFaction = Find.FactionManager.FirstFactionOfDef(RT_DefOf.RT_Metroids);
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            Log.Message("metroidFaction.leader: " + metroidFaction.leader);
            Log.Message("comp.currentQueen is null || comp.currentQueen.Dead: " + (comp.currentQueen is null || comp.currentQueen.Dead));
            Log.Message("PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()): " + PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()));
            Log.Message("comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15): " + (comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15)));
            return (metroidFaction.leader is null || metroidFaction.leader.Dead) && (comp.currentQueen is null || comp.currentQueen.Dead)
                && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Any(x => x.IsOmegaMetroid()) 
                && comp.lastQueenDeathTick != 0 && Find.TickManager.TicksGame > comp.lastQueenDeathTick + (GenDate.TicksPerDay * 15);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var omegaToQueen = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.Where(x => x.IsOmegaMetroid()).RandomElement();
            SpawnPool.doNotRestock = true;
            var queen = TransformPawn(DefDatabase<PawnKindDef>.GetNamed("RT_QueenMetroid"), omegaToQueen);
            SpawnPool.doNotRestock = false;
            Find.LetterStack.ReceiveLetter("LetterLabelOmegaBecomesQueen".Translate(queen.Named("PAWN")), "LetterOmegaBecomesQueen".Translate(queen.Named("PAWN")), LetterDefOf.PositiveEvent, queen);
            var comp = Current.Game.GetComponent<RimtroidEvolutionTracker>();
            comp.currentQueen = queen;
            queen.spawnPool = new SpawnPool();
            return true;
        }

        private Queen TransformPawn(PawnKindDef kindDef, Pawn pawn, bool changeDef = true, bool keep = false)
        {
            Faction faction = pawn.Faction;
            var queen = PawnGenerator.GeneratePawn(kindDef, faction);
            long ageB = pawn.ageTracker.AgeBiologicalTicks;
            long ageC = pawn.ageTracker.AgeChronologicalTicks;
            queen.ageTracker = new Pawn_AgeTracker(pawn);
            queen.ageTracker.AgeBiologicalTicks = ageB;
            queen.ageTracker.AgeChronologicalTicks = ageC;
            queen.Name = pawn.Name;
            queen.training = new Pawn_TrainingTracker(queen);
            foreach (var td in DefDatabase<TrainableDef>.AllDefs)
            {
                if (pawn.training.HasLearned(td))
                {
                    queen.training.Train(td, pawn.playerSettings?.Master);
                }
            }
            foreach (var td in DefDatabase<TrainableDef>.AllDefs)
            {
                if (pawn.training.GetWanted(td))
                {
                    queen.training.SetWantedRecursive(td, true);
                }
            }
            var position = pawn.Position;
            var map = pawn.Map;
            pawn.Destroy(DestroyMode.Vanish);
            GenSpawn.Spawn(queen, position, map);
            return queen as Queen;
        }
    }
}