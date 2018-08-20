using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public abstract class Shape
    {
        public abstract Material Material { get; set; }
        public abstract bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal);
    }
}
