﻿using System;
using System.Collections.Generic;

namespace Raytracer.Core
{
    public abstract class Material
    {
        public Medium Medium { get; set; }
        protected Dictionary<string, MaterialNode> Properties { get; set; }

        public Material(Medium Medium = null)
        {
            this.Medium = Medium;
            this.Properties = new Dictionary<string, MaterialNode>();
        }

        public MaterialNodeValue GetProperty(string Property, Vector2 UV)
        {
            return Properties[Property].Evaluate(UV);
        }

        public bool HasProperty(string Property)
        {
            if (Properties.ContainsKey(Property))
            {
                return Properties[Property] != null;
            }
            return false;
        }

        public abstract void Sample(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out LobeType SampledLobe);
        public abstract void PDF(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out double PDF);
        public abstract void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, out Vector3 Attenuation);
    }
}
