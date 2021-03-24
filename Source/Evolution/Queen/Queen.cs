using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RT_Rimtroid
{
    public class Metroid : Pawn
    {

        //public override void SpawnSetup(Map map, bool respawningAfterLoad)
        //{
        //    base.SpawnSetup(map, respawningAfterLoad);
        //    if (!respawningAfterLoad)
        //    {
        //        this.SetFaction();
        //    }
        //}
    }

    public class SpawnPool : IExposable
    {
        public static List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>
        {
            PawnKindDef.Named("RT_MetroidLarvae"),
            PawnKindDef.Named("RT_BanteeMetroid")
        };
        public List<Pawn> spawnedPawns;
        public List<Pawn> despawnedPawns;

        public int generatedLastTick;
        public SpawnPool()
        {
            spawnedPawns = new List<Pawn>();
            despawnedPawns = new List<Pawn>();
        }

        public List<Pawn> SpawnedPawns => spawnedPawns.Where(x => pawnKindDefs.Contains(x.kindDef) && !x.Dead).ToList();

        public List<Pawn> TotalPawns => SpawnedPawns.Concat(this.despawnedPawns).ToList();
        public bool CanSpawnPawn(out string reason)
        {
            if (!despawnedPawns.Any())
            {
                reason = "RT_CannotSpawnPawn".Translate();
                return false;
            }
            reason = "";
            return true;
        }
        public void SpawnPawn(Queen parent)
        {
            var oldPawn = despawnedPawns.First();
            despawnedPawns.Remove(oldPawn);
            GenSpawn.Spawn(oldPawn, parent.Position, parent.Map);
            var compDrone = oldPawn.TryGetComp<QueenDroneComp>();
            compDrone.AssignToQueen(parent);
            spawnedPawns.Add(oldPawn);
        }

        public void Restock(Queen parent)
        {
            var startingPawnCount = Rand.RangeInclusive(4, 6);
            for (var i = 0; i < startingPawnCount; i++)
            {
                var newPawn = PawnGenerator.GeneratePawn(pawnKindDefs.RandomElement(), parent.Faction);
                GenSpawn.Spawn(newPawn, parent.Position, parent.Map);
                var compDrone = newPawn.TryGetComp<QueenDroneComp>();
                compDrone.AssignToQueen(parent);
                spawnedPawns.Add(newPawn);
            }
        }
        public void RecallAll(Queen parent)
        {
            for (int num = spawnedPawns.Count - 1; num >= 0; num--)
            {
                var pawn = spawnedPawns[num];
                if (!pawn.Dead && !pawn.Downed && !pawn.IsPrisoner)
                {
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RT_DefOf.RT_GoToQueenToDespawn, parent));
                }
            }
        }

        public void AbsorbPawn(Pawn pawn)
        {
            spawnedPawns.Remove(pawn);
            despawnedPawns.Add(pawn);
            pawn.DeSpawn();
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref despawnedPawns, "despawnedPawns", LookMode.Reference);
            Scribe_Values.Look(ref generatedLastTick, "generatedLastTick");
        }
    }
    public class Queen : Metroid
    {
        public SpawnPool spawnPool;
        public static bool preventFactionLeaderSpawn;
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (spawnPool.despawnedPawns.Any())
            {
                foreach (var pawn in spawnPool.despawnedPawns)
                {
                    GenSpawn.Spawn(pawn, this.Position, this.Map);
                }
            }
            preventFactionLeaderSpawn = true;
            base.Kill(dinfo, exactCulprit);
            preventFactionLeaderSpawn = false;
            Find.LetterStack.ReceiveLetter("LetterLabelMetroidQueenKilled".Translate(), "LetterMetroidQueenKilled".Translate(this.Named("PAWN")), LetterDefOf.PositiveEvent, this);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, Find.World);
            parms.target = Find.World;
            Find.Storyteller.incidentQueue.Add(RT_DefOf.RT_QueenSpotted, (int)(GenDate.TicksPerDay * Rand.Range(45f, 60f)), parms);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                spawnPool = new SpawnPool();
                spawnPool.Restock(this);
            }
        }

        public Lord GetCustomLord()
        {
            var lord = this.spawnPool.spawnedPawns?.Select(x => x.GetLord()).Where(x => x?.LordJob is LordJob_DefendQueen).FirstOrDefault();
            if (lord is null)
            {
                lord = LordMaker.MakeNewLord(this.Faction, new LordJob_DefendQueen(this), this.Map);
            }
            return lord;
        }
        public override void PostMake()
        {
            base.PostMake();
            this.Name = new NameSingle(Utils.GenerateTextFromRule(RT_DefOf.RT_QueenNames, "r_name", this.thingIDNumber));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref spawnPool, "spawnPool");
        }
    }
}
