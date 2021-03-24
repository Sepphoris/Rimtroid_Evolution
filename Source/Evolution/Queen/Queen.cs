using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

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

        public bool CanSpawnNewPawn()
        {
            if (Find.TickManager.TicksGame + (12 * GenDate.TicksPerHour) > generatedLastTick)
            {
                var totalPawns = spawnedPawns.Where(x => pawnKindDefs.Contains(x.kindDef) && !x.Dead).Concat(despawnedPawns);
                if (totalPawns.Count() < 6)
                {
                    return true;
                }
            }
            return false;
        }
        public void SpawnPawn(Queen parent)
        {
            var newPawn = PawnGenerator.GeneratePawn(pawnKindDefs.RandomElement(), parent.Faction);
            GenSpawn.Spawn(newPawn, parent.Position, parent.Map);
            var compDrone = newPawn.TryGetComp<QueenDroneComp>();
            compDrone.AssignToQueen(parent);
            spawnedPawns.Add(newPawn);
            this.generatedLastTick = Find.TickManager.TicksGame;
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
                var startingPawnCount = Rand.RangeInclusive(4, 6);
                for (var i = 0; i < startingPawnCount; i++)
                {
                    spawnPool.SpawnPawn(this);
                }
            }
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
