using System;

namespace Raytracer.Core
{
    public abstract class Medium
    {
        public Vector3 AbsorptionCoefficient { get; set; }

        public Medium(Vector3 AbsorptionColor, double AbsorptionDistance)
        {
            this.AbsorptionCoefficient = -new Vector3(Math.Log(AbsorptionColor.X), Math.Log(AbsorptionColor.Y), Math.Log(AbsorptionColor.Z)) / AbsorptionDistance;
        }

        public abstract double SampleDistance(double MaxDistance);
        public abstract Vector3 SampleDirection(Vector3 InDirection);
        public abstract Vector3 Transmission(double Distance);
    }
}
