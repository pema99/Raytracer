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

            Vector3 N = Vector3.Cross(A, B);
            double Denom = Vector3.Dot(N, N);

            //Parallel case
            double NdotRayDirection = Vector3.Dot(N, Ray.Direction);
            if (Math.Abs(NdotRayDirection) < 1e-8)
            {
                return false;
            }

            double D = Vector3.Dot(N, V0);
            double T = (Vector3.Dot(N, Ray.Origin) + D) / NdotRayDirection;

            //Triangle behind ray
            if (T < 0) return false;

            Hit = Ray.Origin + T * Ray.Direction;
            Normal = this.Normal;

            //Check if hit is in triangle
            Vector3 C;
            double U, V;

            //Edge A
            Vector3 EdgeA = V1 - V0;
            Vector3 VPA = Hit - V0;
            C = Vector3.Cross(EdgeA, VPA);
            if (Vector3.Dot(N, C) < 0)
            {
                return false;
            } 

            //Edge B
            Vector3 EdgeB = V2 - V1;
            Vector3 VPB = Hit - V1;
            C = Vector3.Cross(EdgeB, VPB);
            if ((U = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            //Edge C
            Vector3 EdgeC = V0 - V2;
            Vector3 VPC = Hit - V2;
            C = Vector3.Cross(EdgeC, VPC);
            if ((V = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            //TODO: Use UV's
            U /= Denom;
            V /= Denom;

            return true;
        }
    }
}
