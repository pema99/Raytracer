using System;

namespace Raytracer
{
    public class Sphere : Shape
    {
        public Vector3 Origin { get; set; }
        public double Radius { get; set; }
        public override Material Material { get; set; }

        private double RadiusSquared { get; set; }

        public Sphere(Material Material, Vector3 Origin, double Radius)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Radius = Radius;
            this.RadiusSquared = Radius * Radius;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            Vector3 SphereToRay = Origin - Ray.Origin;

            //On the edge, 0 = edge
            double AdjacentUnscaled = Vector3.Dot(SphereToRay, Ray.Direction);
            if (AdjacentUnscaled < 0)
            {
                return false;
            }

            //Outside of circle, opposite larger than radius
            double OppositeSquared = Vector3.Dot(SphereToRay, SphereToRay) - AdjacentUnscaled * AdjacentUnscaled;
            if (OppositeSquared > RadiusSquared)
            {
                return false;
            }

            //Pythagoras
            double Adjacent = Math.Sqrt(RadiusSquared - OppositeSquared);

            //The 2 intersection points
            double TMin = AdjacentUnscaled - Adjacent;
            double TMax = AdjacentUnscaled + Adjacent;

            //If we are closer to second intersection, swap them
            if (TMin > TMax)
            {
                double Temp = TMax;
                TMax = TMin;
                TMin = Temp;
            }

            //If scalar is under 0 we can never hit
            if (TMin < 0)
            {
                TMin = TMax;
                if (TMin < 0)
                {
                    return false;
                }
            }

            Hit = Ray.Origin + TMin * Ray.Direction;

            Vector3 N = Hit - Origin;
            N.Normalize();

            UV = new Vector2((1.0 + Math.Atan2(N.Z, N.X) / MathHelper.Pi) * 0.5, Math.Acos(N.Y) / MathHelper.Pi);

            if (Material.HasNormal())
            {
                Vector3 Tangent = new Vector3(1, 1, (N.X + N.Y) / (-N.Z));
                Tangent.Normalize();

                Vector3 TangentSpaceNormal = Material.GetNormal(UV);
                Matrix TBN = Matrix.CreateWorld(Vector3.Zero, Tangent, N);
                Normal = Vector3.Transform(TangentSpaceNormal, TBN);
            }
            else
            {
                Normal = N;
            }

            return true;
        }
    }
}
