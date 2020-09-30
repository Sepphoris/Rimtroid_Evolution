using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DD
{
    public class Ability_AutoCastableVerb : Ability
    {
        public Ability_AutoCastableVerb(Pawn pawn) : base(pawn)
        {
        }

        public Ability_AutoCastableVerb(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public override bool CanCast => (CompOfType<AbilityComp_AttackVerb>()?.Status ?? true) && base.CanCast;

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (base.Activate(target, dest))
            {
                return true;
            }
            return false;
        }

        public override bool CanApplyOn(LocalTargetInfo target) => true;

        public override void ExposeData()
        {
            base.ExposeData();

            AbilityComp_AttackVerb comp = CompOfType<AbilityComp_AttackVerb>();
            if (comp != null)
            {
                Scribe_Values.Look(ref comp.status, "status", true);
            }

        }

        public override IEnumerable<Command> GetGizmos()
        {
            if (CompOfType<AbilityComp_AttackVerb>() is AbilityComp_AttackVerb comp)
            {
                yield return comp.Gizmo;
            }

            if(Prefs.DevMode)
            {
                foreach (Command cmd in base.GetGizmos())
                {
                    if(!cmd.defaultLabel.StartsWith("Debug"))
                    {
                        cmd.defaultLabel = "Debug: " + cmd.defaultLabel;
                    }
                    
                    yield return cmd;
                }

                yield return new Command_Action()
                {
                    defaultLabel = "Debug: Clear Jobs",
                    defaultDesc = "Clear the queued jobs.",
                    action = () =>
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                };
            }
        }
    }
}
