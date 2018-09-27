using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class Quad : Shape
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 Size { get; set; }
        public override Material Material { get; set; }

        public Quad(Material Material, Vector3 Origin, Vector3 Normal, Vector2 Size)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Normal = Normal;
            this.Size = Size;
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

                    Util.CreateCartesian(Normal, out Vector3 NT, out Vector3 NB);
                    Vector3 THit = Vector3.Transform(Hit, Matrix.Invert(Matrix.CreateWorld(Origin, NT, Normal)));
                    if (Math.Abs(THit.X) > Size.X*0.5 || Math.Abs(THit.Z) > Size.Y*0.5)
                    {
                        return false;
                    }

                    return true;
                }
            }

            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            return false;
        }

        public override Vector3 Sample()
        {
            Vector2 RectSample = new Vector2(Util.Random.NextDouble()*2-1 * (Size.X*0.5), Util.Random.NextDouble()*2-1 * (Size.Y*0.5));
            Util.CreateCartesian(Normal, out Vector3 NT, out Vector3 NB);
            return Origin + NT * RectSample.X + NB * RectSample.Y;
        }

        public override double Area()
        {
            return Size.X * Size.Y;
        }
    }
}
