using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class EmissionMaterial : Material
    {
        public EmissionMaterial(Vector3 Emission)
        {
            Properties.Add("emission", new MaterialConstantNode(Emission));
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            throw new Exception("EmissionMaterial should not be evaluated as a BXDF.");
        }
    }
}
