using System;

namespace Raytracer
{
    public class VelvetMaterial : Material
    {
        public VelvetMaterial(double Sigma, Vector3 Albedo)
        {
            Properties.Add("sigma", new MaterialConstantNode(Sigma));
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
        }

        public VelvetMaterial(MaterialNode Sigma, MaterialNode Albedo, MaterialNode Normal)
        {
            Properties.Add("sigma", Sigma);
            Properties.Add("albedo", Albedo);
            Properties.Add("normal", Normal);
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 N, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            Util.CreateCartesian(N, out Vector3 NT, out Vector3 NB);
            Vector3 Sample = Util.CosineSampleHemisphere(Util.Random.NextDouble(), Util.Random.NextDouble());
            SampleDirection = new Vector3(
                Sample.X * NB.X + Sample.Y * N.X + Sample.Z * NT.X,
                Sample.X * NB.Y + Sample.Y * N.Y + Sample.Z * NT.Y,
                Sample.X * NB.Z + Sample.Y * N.Z + Sample.Z * NT.Z);
            SampleDirection.Normalize();

            SampledLobe = LobeType.DiffuseReflection;

            double InverseSigmaSquared = 1.0 / Math.Pow(GetProperty("sigma", UV), 2);
            double NdotV = Vector3.Dot(N, ViewDirection);
            double NdotL = Vector3.Dot(N, SampleDirection);
            if (NdotV > 0 && NdotL > 0)
            {
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);

                double NdotH = Vector3.Dot(N, Halfway);
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
                    Attenuation = new Vector3(BSDFEval, BSDFEval, BSDFEval) * GetProperty("albedo", UV).Color / (1.0 / (2.0 * Math.PI));
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
    }
}
