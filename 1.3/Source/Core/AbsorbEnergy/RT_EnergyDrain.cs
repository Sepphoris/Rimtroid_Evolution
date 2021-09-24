using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class RT_EnergyDrain : DefModExtension
	{
		public IntRange drainStunDuration;
		public IntRange drainOverlayDuration;
		public FloatRange drainFoodGain;
		public float drainAgeFactor;
		public float drainSicknessSeverity;
		public IntRange drainEnergyProcessing;
	}
}
