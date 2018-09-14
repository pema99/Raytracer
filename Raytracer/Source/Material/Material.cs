using System;
using System.Collections.Generic;

namespace Raytracer
{
    public abstract class Material
    {
        protected Dictionary<string, MaterialNode> Properties { get; set; }

        public Material()
        {
            this.Properties = new Dictionary<string, MaterialNode>();
        }

        public MaterialNodeValue GetProperty(string Property, Vector2 UV)
        {
            return Properties[Property].Evaluate(UV);
        }

        public bool HasProperty(string Property)
        {
            return Properties.ContainsKey(Property);
        }

        public abstract void Evaluate(Vector3 ViewDirection, Vector3 Normal, Vector2 UV, out Vector3 SampleDirection, out Vector3 Attenuation);
    }
}
