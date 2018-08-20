using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Ray
    {
        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; }

        public Ray(Vector3 Origin, Vector3 Direction)
        {
            this.Origin = Origin;
            this.Direction = Direction;
        }
    }
}
