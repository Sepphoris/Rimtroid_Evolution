using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DD
{
    public class CompHostileResponse : ThingComp
    {
        public CompProperties_HostileResponse Props => (CompProperties_HostileResponse)props;
        public IAttackTargetSearcher Self => (IAttackTargetSearcher)parent;
        public Pawn SelfPawn => (Pawn)parent;

        private Dictionary<IAttackTarget, HostilityStatisticRecord> stats = new Dictionary<IAttackTarget, HostilityStatisticRecord>();

        private HostilityResponseType type;
        public HostilityResponseType Type
        {
            get
            {
                if (parent is Pawn pawn)
                {
                    if (pawn.InAggroMentalState || pawn.Faction.HostileTo(Faction.OfPlayer))
                    {
                        return HostilityResponseType.Aggressive;
                    }

                    if (pawn.InMentalState)
                    {
                        return Props.initialHostility;
                    }
                }
                return type;
            }
            set
            {
                type = value;
            }
        }

        public bool CanInteractGizmo => parent.Faction != null && parent.Faction.IsPlayer && parent is Pawn pawn && pawn.InMentalState;

        private Dictionary<HostilityResponseType, Command_Action> gizmos = new Dictionary<HostilityResponseType, Command_Action>();
        public Command_Action Gizmo
        {
            get
            {
                Command_Action cmd = null;

                List<HostileResponseOption> options = Props.options;
                int index = options.FindIndex(o => o.type == Type);
                HostileResponseOption option = options[index];

                if (!gizmos.ContainsKey(Type))
                {
                    //Not created yet, so create it.
                    cmd = new Command_Action()
                    {
                        defaultLabel = option.label,
                        defaultDesc = option.description,
                        icon = option.Texture,
                        action = () =>
                        {
                            Type = options[(index + 1) % options.Count].type;
                        }
                    };
                    gizmos.Add(Type, cmd);
                }
                else
                {
                    //Load it from the cache.
                    cmd = gizmos[Type];
                }

                //Check whether it needs to be updated.
                if (CanInteractGizmo == !cmd.disabled)
                {
                    //If you should be able to interact with the gizmo, then it shouldn't be disabled and vice versa.
                    cmd.disabled = false;

                    if (!CanInteractGizmo)
                    {
                        cmd.Disable(option.disableMessage);
                    }
                }

                return cmd;
            }
        }

        public IEnumerable<IAttackTarget> Targets => stats.Keys.Where(target =>
        {
            if (target == null)
            {
                return false;
            }

            if (target.Thing.DestroyedOrNull())
            {
                return false;
            }

            if (target.Thing.Map != parent.Map)
            {
                //Ignore off-map
                return false;
            }

            if (target.ThreatDisabled(Self))
            {
                return false;
            }

            if (target.Thing is Pawn pawn)
            {
                //Aggressive goes for the kill.
                if (pawn.Downed && Type != HostilityResponseType.Aggressive)
                {
                    return false;
                }

                if (pawn.Dead)
                {
                    return false;
                }
            }
            return true;
        });

        public IEnumerable<IAttackTarget> TargetsPreferredOrder => stats.OrderByDescending(entry => entry.Value.CalculatePoints(Type)).Select(entry => entry.Key);

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            type = Props.initialHostility;
        }

        public override void PostExposeData()
        {
            //Save data
            Scribe_Values.Look(ref type, "responseType", HostilityResponseType.Passive);
            Scribe_Collections.Look(ref stats, "responseStats", LookMode.Reference, LookMode.Deep);

            if(stats == null)
            {
                stats = new Dictionary<IAttackTarget, HostilityStatisticRecord>();
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            //Remove all targets
            Reset();
        }

        public override void PostDeSpawn(Map map)
        {
            //Remove all targets
            Reset();
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            //Add target
            if (dinfo.Amount <= 0)
            {
                return;
            }

            if (dinfo.Instigator != null && dinfo.Instigator is IAttackTarget target)
            {
                if (!dinfo.Instigator.HostileTo(SelfPawn) && dinfo.IntendedTarget != Self)
                {
                    //Ignore unintended damage.
                    return;
                }

                if (!stats.ContainsKey(target))
                {
                    stats.Add(target, new HostilityStatisticRecord(target));
                }

                stats[target].ProcessAttack(dinfo.Amount, dinfo.IntendedTarget == null || parent == dinfo.IntendedTarget);
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            //Remove target
            stats.Remove(pawn);
        }

        public override string CompInspectStringExtra()
        {
            //Show message when targets are available
            int count = Targets.Count();
            if(count > 0)
            {
                return "Hostility: " + Enum.GetName(typeof(HostilityResponseType), Props.options.First(o => o.type == Type).type) + " {" + count + "}";
            }
             else
            {
                return base.CompInspectStringExtra();
            }
        }

        public override void CompTickRare()
        {
            if (parent.DestroyedOrNull())
            {
                return;
            }

            if (!parent.Spawned || parent.Map == null)
            {
                return;
            }

            switch (type)
            {
                case HostilityResponseType.Aggressive:
                {
                    //Add all visible targets.
                    IEnumerable<IAttackTarget> potentialTargets = parent.Map.attackTargetsCache.GetPotentialTargetsFor((IAttackTargetSearcher)parent);
                    foreach (IAttackTarget target in potentialTargets.Select(target => target.Thing).Where(target => !target.DestroyedOrNull() && parent.CanSee(target)))
                    {
                        AddTarget(target);
                    }

                    IAttackTarget atarget = AttackTargetFinder.BestAttackTarget(Self,
                        TargetScanFlags.NeedThreat | TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachable | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.LOSBlockableByGas
                        );
                    if (atarget != null && atarget.Thing != null)
                    {
                        AddTarget(atarget);
                    }
                }
                break;

                case HostilityResponseType.Defensive:
                {
                    //Pick a target that is aiming at the pawn.
                    IAttackTarget target = AttackTargetFinder.BestAttackTarget(Self,
                        TargetScanFlags.NeedThreat | TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachable | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.LOSBlockableByGas,
                        thing =>
                        {
                            if (thing is IAttackTarget atkTarget)
                            {
                                LocalTargetInfo targetted = atkTarget.TargetCurrentlyAimingAt;
                                if (targetted.IsValid && targetted.HasThing)
                                {
                                    return targetted.Thing == parent;
                                }
                            }
                            return false;
                        });
                    if (target != null && target.Thing != null)
                    {
                        AddTarget(target);
                    }
                }
                break;

                case HostilityResponseType.Passive:
                    //Doesn't seek any extra targets.
                    break;
            }
        }

        public override void CompTickLong()
        {
            //Check targets and remove as needed
            stats.RemoveAll(entry => entry.Key.Thing.DestroyedOrNull() || (!SelfPawn.CanSee(entry.Key.Thing) && entry.Value.DamagePoints <= 0));

            switch (type)
            {
                case HostilityResponseType.Aggressive:
                    //Never forgive, never forget.
                    break;
                case HostilityResponseType.Defensive:
                    //Forgiving.
                    stats.RemoveAll(entry => !SelfPawn.CanSee(entry.Key.Thing) && entry.Value.IsOld);
                    break;
                case HostilityResponseType.Passive:
                    //Much more forgiving.
                    stats.RemoveAll(entry => !SelfPawn.CanSee(entry.Key.Thing) && !entry.Value.IsRecent);
                    break;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //Show state toggle gizmo
            if (Gizmo != null)
            {
                yield return Gizmo;
            }
        }

        public void AddTarget(IAttackTarget target)
        {
            if (target == null)
            {
                return;
            }

            if (!stats.ContainsKey(target))
            {
                stats.Add(target, new HostilityStatisticRecord(target));
            }

            stats[target].ProcessSighting();
        }

        public void Reset()
        {
            stats.Clear();
        }
    }
}
