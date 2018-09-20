using System;

namespace Raytracer
{
    public class PBRMaterial : Material
    {
        public PBRMaterial(Vector3 Albedo, double Metalness, double Roughness)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("metalness", new MaterialConstantNode(Metalness));
            Properties.Add("roughness", new MaterialConstantNode(Roughness));
        }

        public PBRMaterial(MaterialNode Albedo, MaterialNode Metalness, MaterialNode Roughness, MaterialNode Normal, MaterialNode AmbientOcclusion, MaterialNode Transparency, MaterialNode RefractiveIndex)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("metalness", Metalness);
            Properties.Add("roughness", Roughness);
            Properties.Add("normal", Normal);
            Properties.Add("ambientocclusion", AmbientOcclusion);
        }

        public PBRMaterial(string Name)
        {
            Properties.Add("albedo", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_basecolor.png", true)));
            Properties.Add("metalness", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_metallic.png")));
            Properties.Add("roughness", new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_roughness.png")));
            Properties.Add("normal", new MaterialNormalNode(new Texture("Assets/Materials/" + Name + "_normal.png")));
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            Vector3 Albedo = GetProperty("albedo", UV);
            double Metalness = GetProperty("metalness", UV);
            Vector3 F0 = Vector3.Lerp(new Vector3(0.04), Albedo, Metalness);

            double DiffuseSpecularRatio = 0.5 + (0.5 * Metalness);

            //Diffuse
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

                double CosTheta = Math.Max(Vector3.Dot(Normal, SampleDirection), 0);
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);

                Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);
                Vector3 Kd = Vector3.One - Ks;

                Kd *= 1.0 - Metalness;
                Vector3 Diffuse = Kd * Albedo;

                //for uniform: return SampleRadiance * (2 * Diffuse * CosTheta) / (1 - DiffuseSpecularRatio);
                Attenuation = (Diffuse * CosTheta) / Math.Sqrt(R1) / (1 - DiffuseSpecularRatio);
            }

            //Glossy
            else
            {
                SampledLobe = LobeType.SpecularReflection;

                double Roughness = MathHelper.Clamp(GetProperty("roughness", UV), 0.001, 1);

                Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, Normal);
                double R1 = Util.Random.NextDouble();
                double R2 = Util.Random.NextDouble();
                SampleDirection = SampleGGX(R1, R2, ReflectionDirection, Roughness);

                double CosTheta = Math.Max(Vector3.Dot(Normal, SampleDirection), 0);
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);

                Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

                double D = GGXDistribution(Normal, Halfway, Roughness);
                double G = GeometrySmith(Normal, ViewDirection, SampleDirection, Roughness);
                Vector3 SpecularNumerator = D * G * Ks;
                double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(Normal, ViewDirection), 0.0) * CosTheta + 0.001;
                Vector3 Specular = SpecularNumerator / SpecularDenominator;

                Attenuation = Specular * CosTheta / (D * Vector3.Dot(Normal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001) / DiffuseSpecularRatio;
            }
        }

        private Vector3 SampleGGX(double R1, double R2, Vector3 ReflectionDirection, double Roughness)
        {
            double A = Math.Pow(Roughness, 2.0);

            //Generate spherical
            double Phi = 2.0 * Math.PI * R1;
            double CosTheta = Math.Sqrt((1.0 - R2) / (1.0 + (A * A - 1.0) * R2));
            double SinTheta = Math.Sqrt(1.0 - CosTheta * CosTheta);

            //Spherical to cartesian
            Vector3 H = new Vector3(Math.Cos(Phi) * SinTheta, Math.Sin(Phi) * SinTheta, CosTheta);

            //Tangent-space to world-space
            Vector3 Up = Math.Abs(ReflectionDirection.Z) < 0.999 ? new Vector3(0.0, 0.0, 1.0) : new Vector3(1.0, 0.0, 0.0);
            Vector3 Tangent = Vector3.Normalize(Vector3.Cross(Up, ReflectionDirection));
            Vector3 BiTangent = Vector3.Cross(ReflectionDirection, Tangent);

            return Vector3.Normalize(Tangent * H.X + BiTangent * H.Y + ReflectionDirection * H.Z);
        }

        public double GGXDistribution(Vector3 Normal, Vector3 Halfway, double Roughness)
        {
            double Numerator = Math.Pow(Roughness, 2.0);
            double Denominator = Math.Pow(Math.Max(Vector3.Dot(Normal, Halfway), 0), 2) * (Numerator - 1.0) + 1.0;
            Denominator = Math.Max(Math.PI * Math.Pow(Denominator, 2.0), 1e-7);
            return Numerator / Denominator;
        }

        public double GeometrySchlickGGX(Vector3 Normal, Vector3 View, double Roughness)
        {
            double Numerator = Math.Max(Vector3.Dot(Normal, View), 0);
            double R = (Roughness * Roughness) / 8.0;
            double Denominator = Numerator * (1.0 - R) + R;
            return Numerator / Denominator;
        }

        public double GeometrySmith(Vector3 Normal, Vector3 View, Vector3 Light, double Roughness)
        {
            return GeometrySchlickGGX(Normal, View, Roughness) * GeometrySchlickGGX(Normal, Light, Roughness);
        }

        public Vector3 FresnelSchlick(double CosTheta, Vector3 F0)
        {
            return F0 + (Vector3.One - F0) * Math.Pow((1.0 - CosTheta), 5.0);
        }
    }
}
