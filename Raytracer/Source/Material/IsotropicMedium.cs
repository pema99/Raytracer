using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class IsotropicMedium : Medium
    {
        public double ScatteringCoefficient { get; set; }

        public IsotropicMedium(Vector3 AbsorptionColor, double AbsorptionDistance, double ScatteringDistance)
            : base(AbsorptionColor, AbsorptionDistance)
        {
            this.ScatteringCoefficient = 1 / ScatteringDistance;
        }

        public override Vector3 SampleDirection(Vector3 InDirectio, out double PDFn)
        {
            PDFn = 1.0 / (4.0 * Math.PI);

            return Util.UniformSampleSphere(Util.Random.NextDouble(), Util.Random.NextDouble());
        }

        public override double SampleDistance(double MaxDistance, out double PDF)
        {
            double Distance = -Math.Log(Util.Random.NextDouble()) / ScatteringCoefficient;

            if (Distance >= MaxDistance)
            {
                PDF = 1.0f;
                return MaxDistance;
            }
            else
            {
                PDF = Math.Exp(-ScatteringCoefficient * Distance);
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
