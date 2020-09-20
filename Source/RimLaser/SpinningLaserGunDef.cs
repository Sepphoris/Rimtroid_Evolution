using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid
{
    public class SpinningLaserGunDef : LaserGunDef
    {
        public List<GraphicData> frames;
        public float rotationSpeed = 1.0f;
    }
}
