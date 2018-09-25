using System;

namespace Raytracer.Core
{
    public class VelvetMaterial : Material
    {
        public VelvetMaterial(Vector3 Albedo, double Sigma)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("sigma", new MaterialConstantNode(Sigma));
        }

        public VelvetMaterial(MaterialNode Albedo, MaterialNode Sigma, MaterialNode Normal)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("sigma", Sigma);
            Properties.Add("normal", Normal);
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation)
        {
            double InverseSigmaSquared = 1.0 / Math.Pow(GetProperty("sigma", UV), 2);
            double NdotV = Vector3.Dot(Normal, ViewDirection);
            double NdotL = Vector3.Dot(Normal, SampleDirection);
            if (NdotV > 0 && NdotL > 0)
            {
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);

                double NdotH = Vector3.Dot(Normal, Halfway);
                double VdotH = Math.Abs(Vector3.Dot(ViewDirection, Halfway));

                if (Math.Abs(NdotH) < 0.99999 && VdotH > 0.00001)
                {
                    double NdotHdivVdotH = NdotH / VdotH;
                    NdotHdivVdotH = Math.Max(NdotHdivVdotH, 0.00001);

                    double FactorView = 2 * Math.Abs(NdotHdivVdotH * NdotV);
                    double FactorLight = 2 * Math.Abs(NdotHdivVdotH * NdotL);

                    double SinNdotHSquared = 1 - NdotH * NdotH;
                    double SinNdotHCubed = SinNdotHSquared * SinNdotHSquared;
                    double CoTangentSquared = (NdotH * NdotH) / SinNdotHSquared;

                    double D = Math.Exp(-CoTangentSquared * InverseSigmaSquared) * InverseSigmaSquared * (1.0 / Math.PI) / SinNdotHCubed;
                    double G = Math.Min(1.0, Math.Min(FactorView, FactorLight));

                    double BSDFEval = 0.25 * (D * G) / NdotV;

                    //BSDF attenuation * Albedo * Uniform hemisphere PDF
                    Attenuation = new Vector3(BSDFEval, BSDFEval, BSDFEval) * GetProperty("albedo", UV).Color;
                }
                else
                {
                    Attenuation = Vector3.Zero;
                }
            }
            else
            {
                Attenuation = Vector3.Zero;
            }
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            PDF = (1.0 / (2.0 * Math.PI));
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            Util.CreateCartesian(Normal, out Vector3 NT, out Vector3 NB);
            Vector3 Sample = Util.CosineSampleHemisphere(Util.Random.NextDouble(), Util.Random.NextDouble());
            SampleDirection = new Vector3(
                Sample.X * NB.X + Sample.Y * Normal.X + Sample.Z * NT.X,
                Sample.X * NB.Y + Sample.Y * Normal.Y + Sample.Z * NT.Y,
                Sample.X * NB.Z + Sample.Y * Normal.Z + Sample.Z * NT.Z);
            SampleDirection.Normalize();

            SampledLobe = LobeType.DiffuseReflection;
        }
    }
}
