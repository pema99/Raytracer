using System;

namespace Raytracer.Core
{
    public class Disc : Shape
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public double Radius { get; set; }
        public override Material Material { get; set; }

        public Disc(Material Material, Vector3 Origin, Vector3 Normal, double Radius)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Normal = Normal;
            this.Radius = Radius;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            double Denom = Vector3.Dot(-this.Normal, Ray.Direction);
            if (Denom > 1e-6)
            {
                Vector3 RayToPlane = Origin - Ray.Origin;
                double T = Vector3.Dot(RayToPlane, -this.Normal) / Denom;
                if (T >= 0)
                {
                    Hit = Ray.Origin + Ray.Direction * T;
                    Normal = this.Normal;

                    Vector3 P = Ray.Origin + Ray.Direction * T;
                    Vector3 V = P - Origin;
                    return V.Length() < Radius;
                    //return Math.Abs(V.X) <= Radius && Math.Abs(V.Y) <= Radius && Math.Abs(V.Z) <= Radius;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public Vector3 Sample()
        {
            Vector2 DiscSample = Util.UniformSampleDisc(Util.Random.NextDouble(), Util.Random.NextDouble());
            Util.CreateCartesian(Normal, out Vector3 NT, out Vector3 NB);
            return Origin + NT * DiscSample.X + NB * DiscSample.Y;
        }

        public double Area()
        {
            return Radius * Radius * Math.PI;
        }
    }
}
