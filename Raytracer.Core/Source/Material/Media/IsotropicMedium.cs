using System;

namespace Raytracer.Core
{
    public class IsotropicMedium : Medium
    {
        public double ScatteringCoefficient { get; set; }

        public IsotropicMedium(Vector3 AbsorptionColor, double AbsorptionDistance, double ScatteringDistance)
            : base(AbsorptionColor, AbsorptionDistance)
        {
            this.ScatteringCoefficient = 1 / ScatteringDistance;
        }

        public override Vector3 SampleDirection(Vector3 InDirection)
        {
            return Util.UniformSampleSphere(Util.Random.NextDouble(), Util.Random.NextDouble());
        }

        public override double SampleDistance(double MaxDistance)
        {
            double Distance = -Math.Log(Util.Random.NextDouble()) / ScatteringCoefficient;

            //If we go outside of the medium
            if (Distance >= MaxDistance)
            {
                return MaxDistance;
            }
            else
            {
                return Distance;
            }
        }

        public override Vector3 Transmission(double Distance)
        {
            Vector3 Att = -AbsorptionCoefficient * Distance;
            return new Vector3(Math.Exp(Att.X), Math.Exp(Att.Y), Math.Exp(Att.Z));
        }
    }
}
