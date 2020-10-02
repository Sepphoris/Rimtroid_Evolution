using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DD
{
    public class JobGiver_HostilityResponse : ThinkNode_JobGiver
    {
        private enum ResponseType { Melee, Ranged, ForcedMelee, ForcedRanged }

        public HostilityResponseMode responseType;

        private static List<Thing> tmpThreats = new List<Thing>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (GetJobs(pawn, JobDefOf.Wait_MaintainPosture).Count() > 0)
            {
                //Waits.
                //pawn.jobs.ClearQueuedJobs();
                return null;
            }

            if (GetJobs(pawn, JobDefOf.AttackMelee).Concat(GetJobs(pawn, JobDefOf.AttackStatic)).Count() > 1)
            {
                //return pawn.CurJob;
                return null;
            }

            if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
            {
                return null;
            }

            switch (responseType)
            {
                case HostilityResponseMode.Attack:
                    return TryGetAttackNearbyEnemyJob(pawn);
                case HostilityResponseMode.Flee:
                    return TryGetFleeJob(pawn);
                case HostilityResponseMode.Ignore:
                default:
                    return null;
            }
        }

        private Job TryGetAttackNearbyEnemyJob(Pawn pawn)
        {
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                //Can't attack.
                return null;
            }

            List<Thing> ignoreList = new List<Thing>();

            Thing target;
            Verb verb;
            do
            {
                target = GetAttackTarget(pawn, ignoreList);
                if (target == null)
                {
                    //No target (No targets at all)
                    return null;
                }

                verb = GetAttackVerb(pawn, target);
                if (verb == null)
                {
                    //We can't attack this target, so select another target
                    ignoreList.Add(target);
                }
            } while (target == null || verb == null);
            //When we've figured out both a target and a verb to attack it with.

            JobDef def = null;
            bool forced = false;

            ResponseType response = AssessSituation(pawn, verb, target);
            switch (response)
            {
                case ResponseType.Melee:
                    def = JobDefOf.AttackMelee;
                    break;
                case ResponseType.Ranged:
                    def = JobDefOf.AttackStatic;
                    break;
                case ResponseType.ForcedMelee:
                    def = JobDefOf.AttackMelee;
                    forced = true;
                    break;
                case ResponseType.ForcedRanged:
                    def = JobDefOf.AttackStatic;
                    forced = true;
                    break;
            }

            if (def == null)
            {
                //We have no way of responding to this situation.
                return null;
            }

            return CreateOrUpdateJob(pawn, forced, def, target);
        }

        private bool IsInDanger(Pawn pawn)
        {
            return GetAttackTarget(pawn, Enumerable.Empty<Thing>()) != null;
        }

        private Thing GetAttackTarget(Pawn pawn, IEnumerable<Thing> ignoreList)
        {
            //Check range.
            IEnumerable<float> ranges = VerbUtils.GetVerbs(pawn).Select(verb => verb.verbProps.range);
            float range = Mathf.Clamp((ranges.EnumerableNullOrEmpty() ? 12f : ranges.Max()), 12f, 24f); //Adjust values here? (12f, range, 50f)
            Func<Thing, bool> InRange = t => pawn.Position.DistanceTo(t.Position) <= range;

            //(Ignore things we've evaluated before) AND (thing is not destroyed) AND (target in range) AND (not a pawn), OR (is a pawn AND its not downed).
            Func<Thing, bool> IsValid = t => !ignoreList.Contains(t) && !t.Destroyed && InRange(t) && !(t is Pawn) || (t is Pawn && !(t as Pawn).Downed);

            Thing target = null;

            //Try to get an attack target. (Manually checking the attacker cache)
            IEnumerable<Thing> targets = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn).ToList().Select(t => t.Thing);
            if(!targets.EnumerableNullOrEmpty())
            {
                targets = targets.Where(IsValid);

                //Can see the target.
                targets = targets.Where(t => pawn.CanSee(t));
                
                //Not forbidden and its not standing in a forbidden location.
                targets = targets.Where(t => !t.IsForbidden(pawn) && !t.Position.IsForbidden(pawn));

                targets.TryMinBy(t => (pawn.Position - t.Position).LengthHorizontalSquared, out target);
            }

            if(target == null)
            {
                //Try to get an attack target. (Normal method)
                target = AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedActiveThreat | TargetScanFlags.LOSBlockableByGas | TargetScanFlags.NeedAutoTargetable, t => IsValid(t)) as Thing;
            }

            return target;
        }

        private Verb GetAttackVerb(Pawn pawn, Thing target)
        {
            Verb verb = pawn.TryGetAttackVerb(target, !pawn.IsColonist); //Can return null.
            if (verb.ApparelPreventsShooting(pawn.Position, target))
            {
                return null;
            }
            return verb;
        }

        private ResponseType AssessSituation(Pawn pawn, Verb verb, Thing target)
        {
            Dictionary<JobDef, IEnumerable<Job>> jobs = GetExistingAttackJobs(pawn);
            if (jobs[JobDefOf.AttackStatic].Count() > 0 && jobs[JobDefOf.AttackMelee].Count() <= 0)
            {
                //Has a ranged job but no melee jobs.
                return ResponseType.Melee;
            }

            if (verb.IsMeleeAttack || pawn.CanReachImmediate(target, PathEndMode.Touch))
            {
                //Selected a melee verb or is in melee range.
                return ResponseType.Melee;
            }

            Pawn pawnTarget = target as Pawn;
            if (pawnTarget != null)
            {
                //If they're trying to goto somewhere.
                Job targetGotoJob = GetJob(pawnTarget, JobDefOf.Goto);
                if (targetGotoJob != null)
                {
                    LocalTargetInfo destCellInfo = targetGotoJob.GetDestination(pawnTarget);

                    //They're going to an exit, so switch to melee.
                    if (destCellInfo != null && pawnTarget.Map.exitMapGrid.IsExitCell(destCellInfo.Cell))
                    {
                        Log.Message(pawnTarget.ToStringSafe() + " is trying to run away!");
                        return ResponseType.ForcedMelee;
                    }
                }
            }

            if(!verb.CanHitTargetFrom(pawn.Position, target))
            {
                //Can't hit target with the selected verb.
                return ResponseType.ForcedMelee;
            }

            return ResponseType.Ranged;
        }

        private Job GetJob(Pawn pawn, JobDef def)
        {
            return GetJobs(pawn, def).FirstOrFallback();
        }

        private IEnumerable<Job> GetJobs(Pawn pawn, JobDef def)
        {
            IEnumerable<Job> jobs = pawn.jobs.jobQueue.Select(qjob => qjob.job);
            if (pawn.jobs.curJob != null)
            {
                jobs = jobs.Prepend(pawn.jobs.curJob);
            }
            jobs = jobs.Where(job => job.def == def);
            List<Job> dbg = jobs.ToList();
            return dbg;
        }

        private Dictionary<JobDef, IEnumerable<Job>> GetExistingAttackJobs(Pawn pawn)
        {
            Dictionary<JobDef, IEnumerable<Job>> dict = new Dictionary<JobDef, IEnumerable<Job>>();

            dict[JobDefOf.AttackMelee] = GetJobs(pawn, JobDefOf.AttackMelee);
            dict[JobDefOf.AttackStatic] = GetJobs(pawn, JobDefOf.AttackStatic);

            return dict;
        }

        private Job CreateOrUpdateJob(Pawn pawn, bool forced, JobDef def, Thing target)
        {
            Dictionary<JobDef, IEnumerable<Job>> prevJobs = GetExistingAttackJobs(pawn);
            IEnumerable<Job> allPrevJobs = prevJobs.Values.Aggregate((list1, list2) => list1.Concat(list2));

            if (prevJobs[def].Count() <= 0)
            {
                //No existing jobs of same type; create a new job.
                Job job = JobMaker.MakeJob(def, target);

                job.expiryInterval = 300;
                job.killIncappedTarget = false;
                job.playerForced = forced;

                if (def == JobDefOf.AttackMelee)
                {
                    job.maxNumMeleeAttacks = 1;
                }
                else if (def == JobDefOf.AttackStatic)
                {
                    job.maxNumStaticAttacks = 1;
                    job.endIfCantShootInMelee = true;
                    job.endIfCantShootTargetFromCurPos = true;
                }
                
                return job;
            }
            else
            {
                //We have a job that we can update.
                Job job = prevJobs[def].First();
                if (job.GetTarget(TargetIndex.A) != target)
                {
                    //Another target was specified.
                    job.SetTarget(TargetIndex.A, target);
                }

                return null;
            }
        }

        private Job TryGetFleeJob(Pawn pawn)
        {
            if (!SelfDefenseUtility.ShouldStartFleeing(pawn))
            {
                return null;
            }

            IntVec3 intVec3;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
            {
                intVec3 = pawn.CurJob.targetA.Cell;
            }
            else
            {
                tmpThreats.Clear();
                List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
                for (int index = 0; index < potentialTargetsFor.Count; ++index)
                {
                    Thing thing = potentialTargetsFor[index].Thing;
                    if (SelfDefenseUtility.ShouldFleeFrom(thing, pawn, false, false))
                    {
                        tmpThreats.Add(thing);
                    }
                }
                List<Thing> thingList1 = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
                for (int index = 0; index < thingList1.Count; ++index)
                {
                    Thing t = thingList1[index];
                    if (SelfDefenseUtility.ShouldFleeFrom(t, pawn, false, false))
                    {
                        tmpThreats.Add(t);
                    }
                }
                if (!tmpThreats.Any<Thing>())
                {
                    Log.Error(pawn.LabelShort + " decided to flee but there is not any threat around.", false);
                    Region region = pawn.GetRegion(RegionType.Set_Passable);
                    if (region == null)
                    {
                        return null;
                    }
                    RegionTraverser.BreadthFirstTraverse(region, (from, reg) => reg.door == null || reg.door.Open, reg =>
                    {
                        List<Thing> thingList = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
                        for (int index = 0; index < thingList.Count; ++index)
                        {
                            Thing t = thingList[index];
                            if (SelfDefenseUtility.ShouldFleeFrom(t, pawn, false, false))
                            {
                                tmpThreats.Add(t);
                                Log.Warning(string.Format("  Found a viable threat {0}; tests are {1}, {2}, {3}", t.LabelShort, t.Map.attackTargetsCache.Debug_CheckIfInAllTargets(t as IAttackTarget), t.Map.attackTargetsCache.Debug_CheckIfHostileToFaction(pawn.Faction, t as IAttackTarget), t is IAttackTarget), false);
                            }
                        }
                        return false;
                    }, 9, RegionType.Set_Passable);
                    if (!tmpThreats.Any<Thing>())
                    {
                        return null;
                    }
                }
                intVec3 = CellFinderLoose.GetFleeDest(pawn, tmpThreats, 23f);
                tmpThreats.Clear();
            }
            return JobMaker.MakeJob(JobDefOf.FleeAndCower, intVec3);
        }
    }
}
