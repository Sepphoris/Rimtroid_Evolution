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
    public class AbilityComp_AttackVerb : CompAbilityEffect
    {
        public bool status = true;
        private Command gizmo;
        private Texture2D iconOn, iconOff;

        public AbilityCompProperties_AttackVerb VProps => props as AbilityCompProperties_AttackVerb;

        public bool Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    parent.pawn.jobs.ClearQueuedJobs();
                }
            }
        }

        public Command Gizmo
        {
            get
            {
                if (gizmo == null)
                {
                    //Load images
                    if (VProps.gizmoOnIconPath != null)
                    {
                        iconOn = ContentFinder<Texture2D>.Get(VProps.gizmoOnIconPath);
                    }
                    if (VProps.gizmoOffIconPath != null)
                    {
                        iconOff = ContentFinder<Texture2D>.Get(VProps.gizmoOffIconPath);
                    }

                    if (iconOn == null)
                    {
                        //If one is unset, use the same image for both.
                        iconOn = iconOff;
                    }
                    if (iconOff == null)
                    {
                        //If one is unset, use the same image for both.
                        iconOff = iconOn;
                    }

                    gizmo = new Command_Toggle()
                    {
                        defaultDesc = VProps.gizmoDesc,
                        isActive = () => Status,
                        activateIfAmbiguous = true,
                        toggleAction = () => Status = !Status
                    };
                }

                gizmo.defaultLabel = Status ? VProps.gizmoOnText : VProps.gizmoOffText;
                gizmo.icon = Status ? iconOn : iconOff;

                gizmo.disabled = GizmoDisabled(out String reason);
                if (gizmo.disabled)
                {
                    gizmo.disabledReason = reason;
                }

                return gizmo;
            }
        }

        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            if (!parent.VerbTracker.AllVerbs.NullOrEmpty())
            {
                foreach (IAttackVerb verb in parent.VerbTracker.AllVerbs.OfType<IAttackVerb>())
                {
                    verb.Ability = parent;
                }
            }
        }
    }
}
