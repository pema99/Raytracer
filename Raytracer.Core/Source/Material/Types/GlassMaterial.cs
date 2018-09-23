﻿using System;

namespace Raytracer.Core
{
    public class GlassMaterial : Material
    {
        public GlassMaterial(Vector3 Albedo, double RefractiveIndex, Medium Medium = null)
        {
            Properties.Add("albedo", new MaterialConstantNode(Albedo));
            Properties.Add("ior", new MaterialConstantNode(RefractiveIndex));

            this.Medium = Medium;
        }

        public GlassMaterial(MaterialNode Albedo, MaterialNode RefractiveIndex, MaterialNode Normal, Medium Medium = null)
        {
            Properties.Add("albedo", Albedo);
            Properties.Add("ior", RefractiveIndex);
            Properties.Add("normal", Normal);

            this.Medium = Medium;
        }

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            Vector3 Albedo = GetProperty("albedo", UV);
            double RefractiveIndex = GetProperty("ior", UV);

            Vector3 RayDirection = -ViewDirection;

            //Fresnel reflect or refract
            if (Util.Random.NextDouble() <= Util.FresnelReal(MathHelper.Clamp(Vector3.Dot(RayDirection, Normal), -1, 1), RefractiveIndex))
            {
                SampledLobe = LobeType.SpecularReflection;

                Vector3 ReflectionDirection = Vector3.Normalize(Vector3.Reflect(RayDirection, Normal));
                SampleDirection = ReflectionDirection;
                Attenuation = Vector3.One;
            }
            else
            {
                SampledLobe = LobeType.SpecularTransmission;

                double CosTheta = MathHelper.Clamp(Vector3.Dot(RayDirection, Normal), -1, 1);
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
                Vector3 RefractionDirection = RefractiveRatio * RayDirection + (RefractiveRatio * CosTheta - Math.Sqrt(1 - RefractiveRatio * RefractiveRatio * (1 - CosTheta * CosTheta))) * Normal;
                SampleDirection = RefractionDirection;
                Attenuation = Albedo;
            }
        }
    }
}
