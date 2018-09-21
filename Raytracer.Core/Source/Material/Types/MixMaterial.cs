using System;

namespace Raytracer.Core
{
    public class MixMaterial : Material
    {
        public Material A { get; set; }
        public Material B { get; set; }

        public MixMaterial(Material A, Material B, double Ratio)
        {
            this.A = A;
            this.B = B;
            Properties.Add("mixratio", new MaterialConstantNode(Ratio));
        }

        public MixMaterial(Material A, Material B, MaterialNode Ratio)
        {
            this.A = A;
            this.B = B;
            Properties.Add("mixratio", Ratio);
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            if (Util.Random.NextDouble() > MathHelper.Clamp(GetProperty("ratio", UV), 0, 1))
            {
                A.Evaluate(ViewDirection, Normal, UV, out SampleDirection, out SampledLobe, out Attenuation);
            }
            else
            {
                B.Evaluate(ViewDirection, Normal, UV, out SampleDirection, out SampledLobe, out Attenuation);
            }
        }
    }
}
