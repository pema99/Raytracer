using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Triangle : Shape
    {
        public Vector3 V0 { get; set; }
        public Vector3 V1 { get; set; }
        public Vector3 V2 { get; set; }
        public Vector3 Normal { get; private set; }
        public override Material Material { get; set; }

        public Triangle(Material Material, Vector3 V0, Vector3 V1, Vector3 V2)
        {
            this.Material = Material;
            this.V0 = V0;
            this.V1 = V1;
            this.V2 = V2;

            this.Normal = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            
            Vector3 A = V1 - V0;
            Vector3 B = V2 - V0;
            Vector3 N = this.Normal;

            //Backface culling
            if (Vector3.Dot(N, Ray.Direction) > 0)
            {
                return false;
            }

            //Triangle's plane intersection
            double Denom = Vector3.Dot(-N, Ray.Direction);
            if (Denom <= 1e-6)
            {
                return false;
            }
            Vector3 RayToPlane = V0 - Ray.Origin;
            double T = Vector3.Dot(RayToPlane, -N) / Denom;

            //Triangle behind ray
            if (T < 0)
            {
                return false;
            }

            //Find hit point
            Vector3 CurrentHit = Ray.Origin + T * Ray.Direction;

            //Check if hit is in triangle
            double U, V;

            //Edge A
            Vector3 EdgeA = V1 - V0;
            Vector3 VPA = CurrentHit - V0;
            Vector3 C = Vector3.Cross(EdgeA, VPA);
            if (Vector3.Dot(N, C) < 0)
            {
                return false;
            }

            //Edge B
            Vector3 EdgeB = V2 - V1;
            Vector3 VPB = CurrentHit - V1;
            C = Vector3.Cross(EdgeB, VPB);
            if ((U = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            //Edge C
            Vector3 EdgeC = V0 - V2;
            Vector3 VPC = CurrentHit - V2;
            C = Vector3.Cross(EdgeC, VPC);
            if ((V = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            //Scale UV's
            double UVScale = Vector3.Dot(N, N);
            U /= UVScale;
            V /= UVScale;

            Hit = CurrentHit;
            Normal = this.Normal;

            return true;
        }
    }
}
