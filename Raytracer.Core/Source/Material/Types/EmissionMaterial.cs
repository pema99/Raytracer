using System;

namespace Raytracer.Core
{
    public class EmissionMaterial : Material
    {
        public EmissionMaterial(Vector3 Emission)
        {
            Properties.Add("emission", new MaterialConstantNode(Emission));
        }

        public EmissionMaterial(MaterialNode Emission)
        {
            Properties.Add("emission", Emission);
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation)
        {
            throw new Exception("EmissionMaterial should not be evaluated as a BXDF.");
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            throw new Exception("EmissionMaterial should not be evaluated as a BXDF.");
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            throw new Exception("EmissionMaterial should not be evaluated as a BXDF.");
        }
    }
}
