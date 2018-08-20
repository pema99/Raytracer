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

        public Material(float Shininess)
        {
            this.Shininess = Shininess;
        }
    }
}
