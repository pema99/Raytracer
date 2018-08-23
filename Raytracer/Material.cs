using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Material
    {
        public Vector3 Color { get; set; }
        public float Shininess { get; set; }
        public float Reflection { get; set; }
        public float Transparency { get; set; }
        public float RefractiveIndex { get; set; }

        public Material(Vector3 Color, float Shininess, float Reflectivity, float Transparency, float RefractiveIndex)
        {
            this.Color = Color;
            this.Shininess = Shininess;
            this.Reflection = Reflectivity;
            this.Transparency = Transparency;
            this.RefractiveIndex = RefractiveIndex;
        }
    }
}
