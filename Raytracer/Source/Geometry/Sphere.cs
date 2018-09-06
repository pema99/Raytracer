using System;

namespace Raytracer
{
    public class Sphere : Shape
    {
        public Vector3 Origin { get; set; }
        public double Radius { get; set; }
        public override Material Material { get; set; }

        public Sphere(Material Material, Vector3 Origin, double Radius)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Radius = Radius;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            double A = 1.0;
            double B = 2.0 * Vector3.Dot(Ray.Direction, (Ray.Origin - Origin));
            double C = (Ray.Origin - Origin).LengthSquared() - (Radius * Radius);

            double T0, T1;
            if (!Util.SolveQuadratic(A, B, C, out T0, out T1))
            {
                return false;
            }

            if (T0 > T1)
            {
                double Temp = T1;
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

            //float TexX = (1 + (float)Math.Atan2(Normal.Y, Normal.X) / MathHelper.Pi) * 0.5f;
            //float TexY = (float)Math.Acos(Normal.Z) / MathHelper.Pi;
            UV = new Vector2((1.0 + Math.Atan2(Normal.Z, Normal.X) / MathHelper.Pi) * 0.5, Math.Acos(Normal.Y) / MathHelper.Pi);

            return true;
        }
    }
}
