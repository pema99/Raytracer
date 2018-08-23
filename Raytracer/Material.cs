using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Material
    {
        public float Shininess { get; set; }
        public float Reflectivity { get; set; }
        public float Transparency { get; set; }

        public Material(float Shininess, float Reflectivity, float Transparency)
        {
            this.Shininess = Shininess;
            this.Reflectivity = Reflectivity;
            this.Transparency = Transparency;
        }
    }
}
