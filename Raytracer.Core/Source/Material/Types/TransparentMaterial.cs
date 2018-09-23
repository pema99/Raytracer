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

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            SampleDirection = -ViewDirection;
            SampledLobe = LobeType.DiffuseTransmission;
            Attenuation = GetProperty("albedo", UV);
        }
    }
}
