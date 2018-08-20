using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Light
    {
        public Vector3 Origin { get; set; }
        public float Intensity { get; set; }
        public Vector3 Color { get; set; }
        
        public Light(Vector3 Origin, float Intensity, Vector3 Color)
        {
            this.Origin = Origin;
            this.Intensity = Intensity;
            this.Color = Color;
        }

        public Light(Vector3 Origin, float Intensity, Color Color)
        {
            this.Origin = Origin;
            this.Intensity = Intensity;
            this.Color = Color.ToVector3();
        }
    }
}
