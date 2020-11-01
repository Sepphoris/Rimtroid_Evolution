using Verse;
using RimWorld;
using HarmonyLib;

namespace RT_Rimtroid
{
    public class DisableTrainingDegredation : DefModExtension
    {
    }

    [HarmonyPatch(typeof(TrainableUtility), "DegradationPeriodTicks")]
    public static class RT_TrainableUtility_DegradationPeriodTicks
    {
        public static void Postfix(ThingDef def, ref int __result)
        {
            if (def.HasModExtension<DisableTrainingDegredation>())
            {
                __result = Find.TickManager.TicksGame + 1; //Cheeky.
            }
        }
    }

    [HarmonyPatch(typeof(TrainableUtility), "TamenessCanDecay")]
    public static class RT_TrainableUtility_TamenessCanDecay
    {
        public static void Postfix(ThingDef def, ref bool __result)
        {
            if (def.HasModExtension<DisableTrainingDegredation>())
            {
                __result = false; //Tameness never decays
            }
        }
    }
}