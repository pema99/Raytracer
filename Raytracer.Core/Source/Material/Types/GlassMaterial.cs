using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class GlassMaterial : Material
    {
        public GlassMaterial(Vector3 Albedo, double RefractiveIndex, double Roughness, Medium Medium = null)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("ior", new MaterialConstantNode(RefractiveIndex));
            Properties.Add("roughness", new MaterialConstantNode(Roughness));

            this.Medium = Medium;
        }

        public GlassMaterial(MaterialNode Albedo, MaterialNode RefractiveIndex, MaterialNode Roughness, MaterialNode Normal, Medium Medium = null)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("ior", RefractiveIndex);
            Properties.Add("roughness", Roughness);
            Properties.Add("normal", Normal);
            this.Medium = Medium;
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation)
        {
            if (SampledLobe == LobeType.SpecularReflection)
            {
                Attenuation = Vector3.One;
            }
            else
            {
                Attenuation = GetProperty("albedo", UV);
            }
        }

        public override void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF)
        {
            PDF = 1;
        }

        public override void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe)
        {
            double RefractiveIndex = GetProperty("ior", UV);
            double Roughness = GetProperty("roughness", UV);

            //Reflection
            if (Util.Random.NextDouble() <= Util.FresnelReal(MathHelper.Clamp(Vector3.Dot(-ViewDirection, Normal), -1, 1), RefractiveIndex))
            {
                SampledLobe = LobeType.SpecularReflection;
                SampleDirection = Vector3.Reflect(-ViewDirection, Normal);               
            }
            else
            {
                SampledLobe = LobeType.SpecularTransmission;
                SampleDirection = Vector3.Refract(-ViewDirection, Normal, RefractiveIndex);         
            }

            if (Roughness > 0)
            {
                SampleDirection = Util.SampleGGX(Util.Random.NextDouble(), Util.Random.NextDouble(), SampleDirection, Roughness);
            }
        }
    }
}
