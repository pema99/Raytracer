using System;

namespace Raytracer.Core
{
    public class PBRMaterial : Material
    {
        public PBRMaterial(Vector3 Albedo, double Metalness, double Roughness)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("metalness", new MaterialConstantNode(Metalness));
            Properties.Add("roughness", new MaterialConstantNode(Roughness));
        }

        public PBRMaterial(MaterialNode Albedo, MaterialNode Metalness, MaterialNode Roughness, MaterialNode Normal, MaterialNode AmbientOcclusion)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("metalness", Metalness);
            Properties.Add("roughness", Roughness);
            if (Normal != null)
            {
                Properties.Add("normal", Normal);
            }
            Properties.Add("ambientocclusion", AmbientOcclusion);
        }

        public PBRMaterial(string Name)
        {
            Properties.Add("albedo", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_basecolor.png", true)));
            Properties.Add("metalness", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_metallic.png")));
            Properties.Add("roughness", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_roughness.png")));
            Properties.Add("normal", new MaterialNormalNode(new Texture("Assets/Materials/" + Name + "_normal.png")));
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            double DiffuseSpecularRatio = 0.5 + (0.5 * GetProperty("metalness", UV));

            if (Util.Random.NextDouble() > DiffuseSpecularRatio)
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

            //Glossy
            else
            {
                SampledLobe = LobeType.SpecularReflection;

                double Roughness = MathHelper.Clamp(GetProperty("roughness", UV), 0.001, 1);

                Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, Normal);
                double R1 = Util.Random.NextDouble();
                double R2 = Util.Random.NextDouble();
                SampleDirection = Util.SampleGGX(R1, R2, ReflectionDirection, Roughness);
            }
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            if (SampledLobe == LobeType.DiffuseReflection)
            {
                PDF = (Vector3.Dot(SampleDirection, Normal) / Math.PI);
            }   
            else
            {
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);
                double D = Util.GGXDistribution(Normal, Halfway, GetProperty("roughness", UV));
                PDF = (D * Vector3.Dot(Normal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001);
            }
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation)
        {
            Vector3 Albedo = GetProperty("albedo", UV);
            double Metalness = GetProperty("metalness", UV);
            Vector3 F0 = Vector3.Lerp(new Vector3(0.04), Albedo, Metalness);

            double DiffuseSpecularRatio = 0.5 + (0.5 * Metalness);

            double CosTheta = Math.Max(Vector3.Dot(Normal, SampleDirection), 0);
            Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);
            Vector3 Ks = Util.FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

            if (SampledLobe == LobeType.DiffuseReflection)
            {
                Vector3 Kd = Vector3.One - Ks;

                Kd *= 1.0 - Metalness;
                Vector3 Diffuse = Kd * Albedo / Math.PI;

                Attenuation = Diffuse * CosTheta / (1 - DiffuseSpecularRatio);
            }
            else
            {
                double Roughness = GetProperty("roughness", UV);

                double D = Util.GGXDistribution(Normal, Halfway, Roughness);
                double G = Util.GeometrySmith(Normal, ViewDirection, SampleDirection, Roughness);
                Vector3 SpecularNumerator = D * G * Ks;
                double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(Normal, ViewDirection), 0.0) * CosTheta + 0.001;
                Vector3 Specular = SpecularNumerator / SpecularDenominator;

                Attenuation = Specular * CosTheta / DiffuseSpecularRatio;
            }
        }
    }
}
