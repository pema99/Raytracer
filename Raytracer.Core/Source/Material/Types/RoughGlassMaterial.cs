using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class RoughGlassMaterial : Material
    {
        public RoughGlassMaterial(Vector3 Albedo, double RefractiveIndex, double Roughness, Medium Medium = null)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("ior", new MaterialConstantNode(RefractiveIndex));
            Properties.Add("roughness", new MaterialConstantNode(Roughness));

            this.Medium = Medium;
        }

        public RoughGlassMaterial(MaterialNode Albedo, MaterialNode RefractiveIndex, MaterialNode Roughness, MaterialNode Normal, Medium Medium = null)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("ior", RefractiveIndex);
            Properties.Add("roughness", Roughness);
            Properties.Add("normal", Normal);
            this.Medium = Medium;
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            Vector3 Albedo = GetProperty("albedo", UV);
            double RefractiveIndex = GetProperty("ior", UV);
            double Roughness = GetProperty("roughness", UV);

            //Reflection
            double FresnelRatio = Util.FresnelReal(MathHelper.Clamp(Vector3.Dot(-ViewDirection, Normal), -1, 1), RefractiveIndex);
            if (Util.Random.NextDouble() <= FresnelRatio)
            {
                SampledLobe = LobeType.SpecularReflection;

                Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, Normal);
                double R1 = Util.Random.NextDouble();
                double R2 = Util.Random.NextDouble();
                SampleDirection = Util.SampleGGX(R1, R2, ReflectionDirection, Roughness);

                double CosTheta = Math.Abs(Vector3.Dot(Normal, SampleDirection));
                Vector3 Halfway = Vector3.Normalize(SampleDirection + ViewDirection);

                Vector3 Ks = Util.FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), new Vector3(Math.Pow((1 - RefractiveIndex) / (1 + RefractiveIndex), 2)));

                double D = Util.GGXDistribution(Normal, Halfway, Roughness);
                double G = Util.GeometrySmith(Normal, ViewDirection, SampleDirection, Roughness);
                Vector3 SpecularNumerator = D * G * Ks;
                double SpecularDenominator = 4.0 * Math.Abs(Vector3.Dot(Normal, ViewDirection)) * CosTheta + 0.001;
                Vector3 Specular = SpecularNumerator / SpecularDenominator;

                Attenuation = Specular * CosTheta / (D * Vector3.Dot(Normal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001) / FresnelRatio;// divide by Fresnel Ratio???;
            }

            //Refraction
            else
            {
                SampledLobe = LobeType.SpecularTransmission;

                double CosTheta = MathHelper.Clamp(Vector3.Dot(-ViewDirection, Normal), -1, 1);
                double RefractiveIndexA = 1;
                double RefractiveIndexB = RefractiveIndex;
                if (CosTheta < 0)
                {
                    CosTheta = -CosTheta;
                }
                else
                {
                    var Temp = RefractiveIndexA;
                    RefractiveIndexA = RefractiveIndexB;
                    RefractiveIndexB = Temp;
                    Normal = -Normal;
                }
                double RefractiveRatio = RefractiveIndexA / RefractiveIndexB;
                Vector3 RefractionDirection = RefractiveRatio * (-ViewDirection) + (RefractiveRatio * CosTheta - Math.Sqrt(1 - RefractiveRatio * RefractiveRatio * (1 - CosTheta * CosTheta))) * Normal;

                SampleDirection = Util.SampleGGX(Util.Random.NextDouble(), Util.Random.NextDouble(), RefractionDirection, Roughness);

                Attenuation = Albedo / (1 - FresnelRatio);
            }
        }
    }
}
