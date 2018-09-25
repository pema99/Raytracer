using System;

namespace Raytracer.Core
{
    public class TransparentMaterial : Material
    {
        public TransparentMaterial(Vector3 Albedo, Medium Medium = null)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));

            this.Medium = Medium;
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation)
        {
            Attenuation = GetProperty("albedo", UV);
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            PDF = 1;
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            SampleDirection = -ViewDirection;
            SampledLobe = LobeType.DiffuseTransmission;
        }
    }
}
