using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DD
{
    [HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
    public static class DD_AttackTargetFinder_BestAttackTarget
    {
        public static void Postfix(ref IAttackTarget __result, IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator, float minDist, float maxDist, IntVec3 locus, float maxTravelRadiusFromLocus, bool canBash, bool canTakeTargetsCloserThanEffectiveMinRange)
        {
            if (__result == null && searcher != null)
            {
                //Only if it hasn't selected anything.
                Pawn pawn = searcher.Thing as Pawn;

                if (pawn != null && pawn.kindDef.HasModExtension<VerbSettingExtension>() && pawn.kindDef.GetModExtension<VerbSettingExtension>().verbControl)
                {
                    //Rerun the search, but just try to shoot from your current position.
                    List<IAttackTarget> candidates = searcher.Thing.Map.mapPawns.AllPawnsSpawned.OfType<IAttackTarget>().Where(target => target != null && target.Thing != null && (minDist <= target.Thing.Position.DistanceTo(pawn.Position) && target.Thing.Position.DistanceTo(pawn.Position) <= maxDist) && Match(searcher, validator, flags, target)).ToList();
                    if (!candidates.NullOrEmpty())
                    {
                        MethodInfo method = AccessTools.Method(typeof(AttackTargetFinder), "GetAvailableShootingTargetsByScore", new Type[] { typeof(List<IAttackTarget>), typeof(IAttackTargetSearcher), typeof(Verb) });
                        List<Pair<IAttackTarget, float>> result = method.Invoke(null, new object[] { candidates, searcher, searcher.CurrentEffectiveVerb }) as List<Pair<IAttackTarget, float>>;

                        if (!result.NullOrEmpty())
                        {
                            __result = result.RandomElementByWeight(e => e.Second).First;
                        }
                    }
                }
            }
        }

        private static bool Match(IAttackTargetSearcher searcher, Predicate<Thing> validator, TargetScanFlags flags, IAttackTarget target)
        {
            if (searcher == null || searcher.Thing == null)
            {
                return false;
            }

            if (target == null || target.Thing == null)
            {
                return false;
            }

            if (searcher.Thing.Faction != null && target.Thing.Faction != null && !target.Thing.Faction.IsPlayer)
            {
                return false;
            }

            if (flags.HasFlag(TargetScanFlags.NeedThreat) || flags.HasFlag(TargetScanFlags.NeedActiveThreat))
            {
                if (target.ThreatDisabled(searcher))
                {
                    return false;
                }

                if (!searcher.Thing.HostileTo(target.Thing))
                {
                    return false;
                }

                if (flags.HasFlag(TargetScanFlags.NeedActiveThreat))
                {
                    if (searcher.Thing.Faction != null && !GenHostility.IsActiveThreatTo(target, searcher.Thing.Faction))
                    {
                        return false;
                    }
                }
            }

            if (flags.HasFlag(TargetScanFlags.NeedReachable) || flags.HasFlag(TargetScanFlags.NeedReachableIfCantHitFromMyPos))
            {
                if (target.Thing is Pawn pawn)
                {
                    if (!pawn.CanReach(target.Thing, PathEndMode.Touch, Danger.Deadly))
                    {
                        return false;
                    }
                }
            }

            if (flags.HasFlag(TargetScanFlags.NeedLOSToPawns) || flags.HasFlag(TargetScanFlags.NeedLOSToNonPawns))
            {
                if (flags.HasFlag(TargetScanFlags.NeedLOSToPawns))
                {
                    if (target.Thing is Pawn)
                    {
                        if (!searcher.Thing.CanSee(target.Thing))
                        {
                            return false;
                        }
                    }
                }

                if (flags.HasFlag(TargetScanFlags.NeedLOSToNonPawns))
                {
                    if (!(target.Thing is Pawn))
                    {
                        if (!searcher.Thing.CanSee(target.Thing))
                        {
                            return false;
                        }
                    }
                }
            }

            if (!searcher.Thing.CanSee(target.Thing))
            {
                if (searcher.Thing is Pawn pawn && !pawn.CanReach(target.Thing, PathEndMode.Touch, Danger.Deadly))
                {
                    //Can't see, can't reach; Doesn't exist.
                    return false;
                }
            }

            if (validator != null)
            {
                if (!validator(target.Thing))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
