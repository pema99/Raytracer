using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Sphere : Shape
    {
        public Vector3 Origin { get; set; }
        public float Radius { get; set; }
        public override Material Material { get; set; }

        public Sphere(Material Material, Vector3 Origin, float Radius)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Radius = Radius;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;

            float A = 1;
            float B = 2 * Vector3.Dot(Ray.Direction, (Ray.Origin - Origin));
            float C = (Ray.Origin - Origin).LengthSquared() - (Radius * Radius);

            float T0, T1;
            if (!Util.SolveQuadratic(A, B, C, out T0, out T1))
            {
                return false;
            }

            if (T0 > T1)
            {
                float Temp = T1;
                T1 = T0;
                T0 = Temp;
            }

            if (T0 < 0)
            {
                T0 = T1;
                if (T0 < 0)
                {
                    return false;
                }
            }

            //T = T0
            Hit = Ray.Origin + T0 * Ray.Direction;

            Normal = Hit - Origin;
            Normal.Normalize();

            return true;
        }
    }
}
