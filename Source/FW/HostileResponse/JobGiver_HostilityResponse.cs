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
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompHostileResponse comp = pawn.GetComp<CompHostileResponse>();

            if (comp == null)
            {
                //Needs CompHostileResponse.
                return null;
            }

            if (comp.Targets.EnumerableNullOrEmpty())
            {
                //No targets.
                return null;
            }

            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                //Can't attack.
                return null;
            }

            if (pawn.IsFighting() || !pawn.jobs.IsCurrentJobPlayerInterruptible())
            {
                //Is fighting, or is doing an uninterruptable job.
                return null;
            }

            if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
            {
                //Job is uninterruptable or pawn is on fire.
                return null;
            }

            return TryGetAttackNearbyEnemyJob(pawn);
        }

        private Job TryGetAttackNearbyEnemyJob(Pawn pawn)
        {
            CompHostileResponse comp = pawn.GetComp<CompHostileResponse>();

            //Get a list of verbs that can hit any of our targets.
            IEnumerable<Verb> verbs = VerbUtils.GetPossibleVerbs(pawn).Where(v => comp.Targets.Any(t => v.CanHitTarget(t.Thing) || (v.IsMeleeAttack && pawn.CanReachImmediate(t.Thing, PathEndMode.Touch))));

            //Pick the first target that we can hit.
            IAttackTarget target = comp.TargetsPreferredOrder.FirstOrFallback(t => verbs.Any(v => v.CanHitTarget(t.Thing)));
            if (target == null)
            {
                //No verbs can hurt any of the pawn in the list.
                return null;
            }

            Verb verb = verbs.RandomElementByWeightWithFallback(v => v.verbProps.commonality);
            if (verb == null)
            {
                //Can't pick a verb?
                return null;
            }

            Job job;

            if (verb.IsMeleeAttack)
            {
                job = JobMaker.MakeJob(JobDefOf.AttackMelee, target.Thing);
                job.maxNumMeleeAttacks = 1;
            }
            else
            {
                job = JobMaker.MakeJob(JobDefOf.AttackStatic, target.Thing);
                job.maxNumStaticAttacks = 1;
                job.endIfCantShootInMelee = verb.verbProps.EffectiveMinRange(target.Thing, pawn) > 1.0f;
            }

            job.verbToUse = verb;
            job.expireRequiresEnemiesNearby = true;
            job.killIncappedTarget = comp.Type == HostilityResponseType.Aggressive;
            job.playerForced = true;

            return job;
        }
    }
}
