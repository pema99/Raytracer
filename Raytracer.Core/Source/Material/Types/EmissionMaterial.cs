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
            Attenuation = GetProperty("emission", UV).Color / Math.PI * Math.Max(Vector3.Dot(Normal, SampleDirection), 0);
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            PDF = (Vector3.Dot(SampleDirection, Normal) / Math.PI);
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            SampledLobe = LobeType.DiffuseReflection;

            Util.CreateCartesian(Normal, out Vector3 NT, out Vector3 NB);
            double R1 = Util.Random.NextDouble();
            double R2 = Util.Random.NextDouble();
            Vector3 Sample = Util.CosineSampleHemisphere(R1, R2);
            SampleDirection = new Vector3(
                Sample.X * NB.X + Sample.Y * Normal.X + Sample.Z * NT.X,
                Sample.X * NB.Y + Sample.Y * Normal.Y + Sample.Z * NT.Y,
                Sample.X * NB.Z + Sample.Y * Normal.Z + Sample.Z * NT.Z);
            SampleDirection.Normalize();
        }
    }
}
