using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class LinearMedium : Medium
    {
        public LinearMedium(Vector3 AbsorptionColor, double AbsorptionDistance)
            : base(AbsorptionColor, AbsorptionDistance)
        {
        }

        public override Vector3 SampleDirection(Vector3 InDirection)
        {
            return InDirection;
        }

        public override double SampleDistance(double MaxDistance)
        {
            return MaxDistance;
        }

        public override Vector3 Transmission(double Distance)
        {
            Vector3 Att = -AbsorptionCoefficient * Distance;
            return new Vector3(Math.Exp(Att.X), Math.Exp(Att.Y), Math.Exp(Att.Z));
        }
    }
}
