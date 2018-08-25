﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Disc : Shape
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public float Radius { get; set; }
        public override Material Material { get; set; }

        public Disc(Material Material, Vector3 Origin, Vector3 Normal, float Radius)
        {
            this.Material = Material;
            this.Origin = Origin;
            this.Normal = Normal;
            this.Radius = Radius;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;

            float Denom = Vector3.Dot(-this.Normal, Ray.Direction);
            if (Denom > 1e-6)
            {
                Vector3 RayToPlane = Origin - Ray.Origin;
                float T = Vector3.Dot(RayToPlane, -this.Normal) / Denom;
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
    }
}