﻿using System;

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

        public override void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation)
        {
            throw new Exception("EmissionMaterial should not be evaluated as a BXDF.");
        }
    }
}