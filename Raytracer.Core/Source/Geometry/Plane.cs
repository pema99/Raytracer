namespace Raytracer.Core
{
    public class Plane : Shape
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public override Material Material { get; set; }

        public Plane(Material Material, Vector3 Origin, Vector3 Normal)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Normal = Normal;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV)
        {
            double Denom = Vector3.Dot(-this.Normal, Ray.Direction);
            if (Denom > 1e-6)
            {
                Vector3 RayToPlane = Origin - Ray.Origin;
                double T = Vector3.Dot(RayToPlane, -this.Normal) / Denom;
                if (T >= 0)
                {
                    Hit = Ray.Origin + Ray.Direction * T;
                    Normal = this.Normal;
                    UV = Vector2.Zero;
                    return true;
                }
            }

            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            return false;
        }
    }
}
