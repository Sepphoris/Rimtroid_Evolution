using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace RT_Core
{
    public class CompProperties_LatchedMetroid : CompProperties
    {
        public CompProperties_LatchedMetroid()
        {
            this.compClass = typeof(CompLatchedMetroid);
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public class Pawn_Kill_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            var comp = __instance.TryGetComp<CompLatchedMetroid>();
            if (comp != null)
            {
                comp.hediff_LatchedMetroid.pawn.health.RemoveHediff(comp.hediff_LatchedMetroid);
            }
        }
    }

    public class CompLatchedMetroid : ThingComp
    {
        public CompProperties_LatchedMetroid Props => (CompProperties_LatchedMetroid)props;
        public Pawn latchedMetroid;
        public int drainStunDuration;
        public int drainOverlayDuration;
        public float drainFoodGain;
        public float drainAgeFactor;
        public float drainSicknessSeverity;
        public int startLatchingTick;
        public int drainEnergyProcessing;
        public Hediff_LatchedMetroid hediff_LatchedMetroid;

        public override void PostDraw()
        {
            base.PostDraw();
            if (hediff_LatchedMetroid.pawn.Rotation == Rot4.North)
            {
                hediff_LatchedMetroid.pawn.Rotation = Rot4.South;
            }
            if (this.parent is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                var drawPos = this.parent.DrawPos;
                drawPos.y++;
                Vector3 a = drawPos;
                if (pawn.Rotation != Rot4.North)
                {
                    a.y += 6f / 245f;
                }
                else
                {
                    a.y += 3f / 140f;
                }
                Quaternion quaternion = Quaternion.AngleAxis(pawn.Drawer.renderer.BodyAngle(), Vector3.up);
                Vector3 b = quaternion * pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation);
                latchedMetroid.Drawer.DrawAt(a + b);
            }
            else
            {
                var drawPos = this.parent.DrawPos;
                drawPos.y++;
                latchedMetroid.Drawer.DrawAt(drawPos);
            }
            latchedMetroid.Rotation = hediff_LatchedMetroid.pawn.Rotation;
            Log.Message(latchedMetroid + " - " + latchedMetroid.Rotation.ToStringHuman() + " - " + hediff_LatchedMetroid.pawn + " - " + hediff_LatchedMetroid.pawn.Rotation.ToStringHuman());
        }

        public override void CompTick()
        {
            base.CompTick();
            CheckSpawnLatchedMetroid();
        }
        public override void CompTickLong()
        {
            base.CompTickLong();
            CheckSpawnLatchedMetroid();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            CheckSpawnLatchedMetroid();
        }

        private void CheckSpawnLatchedMetroid()
        {
            if (Find.TickManager.TicksGame > startLatchingTick + drainOverlayDuration)
            {
                hediff_LatchedMetroid.pawn.health.RemoveHediff(hediff_LatchedMetroid);
            }
        }
    }
}