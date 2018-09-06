using System;
using System.Diagnostics;
using System.Drawing;

namespace Raytracer
{
    public static class Util
    {
        #region General utility
        [ThreadStatic]
        private static Random __random;
        public static Random Random => __random ?? (__random = new Random());

        public static Vector3 ToVector3(this Color Color)
        {
            return new Vector3(Color.R / 255.0, Color.G / 255.0, Color.B / 255.0);
        }

        public static Color ToColor(this Vector3 Color)
        {
            return System.Drawing.Color.FromArgb((int)(Color.X * 255.0), (int)(Color.Y * 255.0), (int)(Color.Z * 255.0));
        }

        public static int BoolToInt(bool Boolean)
        {
            if (Boolean)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static bool IntToBool(int Integer)
        {
            return Integer > 0;
        }
        #endregion

        #region Geometry utility
        public static bool SolveQuadratic(double A, double B, double C, out double T0, out double T1)
        {
            double D = B * B - 4.0 * A * C;
            if (D < 0.0)
            {
                T0 = T1 = 0.0;
                return false;
            }
            else if (D == 0)
            {
                T0 = T1 = -0.5 * B / A;
            }
            else
            {
                double Q = 0.0;
                if (B > 0.0)
                {
                    Q = -0.5 * (B + Math.Sqrt(D));
                }
                else
                {
                    Q = -0.5 * (B - Math.Sqrt(D));
                }
                T0 = Q / A;
                T1 = C / Q;
            }
            if (T0 > T1)
            {
                double Temp = T1;
                T1 = T0;
                T0 = Temp;
            }
            return true;
        }

        public static bool IntersectAABB(Ray Ray, Vector3 AABBMin, Vector3 AABBMax, out Vector3 Hit, out double TMin, out double TMax)
        {
            Hit = Vector3.Zero;
            TMin = (AABBMin.X - Ray.Origin.X) / Ray.Direction.X;
            TMax = (AABBMax.X - Ray.Origin.X) / Ray.Direction.X;

            if (TMin > TMax)
            {
                double Temp = TMin;
                TMin = TMax;
                TMax = Temp;
            }

            double TYMax = (AABBMax.Y - Ray.Origin.Y) / Ray.Direction.Y;
            double TYMin = (AABBMin.Y - Ray.Origin.Y) / Ray.Direction.Y;

            if (TYMin > TYMax)
            {
                double Temp = TYMin;
                TYMin = TYMax;
                TYMax = Temp;
            }

            if ((TMin > TYMax) || (TYMin > TMax))
                return false;

            if (TYMin > TMin)
                TMin = TYMin;

            if (TYMax < TMax)
                TMax = TYMax;

            double TZMin = (AABBMin.Z - Ray.Origin.Z) / Ray.Direction.Z;
            double TZMax = (AABBMax.Z - Ray.Origin.Z) / Ray.Direction.Z;

            if (TZMin > TZMax)
            {
                double Temp = TZMin;
                TZMin = TZMax;
                TZMax = Temp;
            }

            if ((TMin > TZMax) || (TZMin > TMax))
                return false;

            if (TZMin > TMin)
                TMin = TZMin;

            if (TZMax < TMax)
                TMax = TZMax;

            Hit = Ray.Origin + Ray.Direction * TMin;

            return true;
        }

        public static bool IntersectAABB(Ray Ray, Vector3 AABBMin, Vector3 AABBMax, out double TMin, out double TMax)
        {
            return IntersectAABB(Ray, AABBMin, AABBMax, out Vector3 Hit, out TMin, out TMax);
        }

        public static bool IntersectAABB(Ray Ray, Vector3 AABBMin, Vector3 AABBMax, out Vector3 Hit)
        {
            return IntersectAABB(Ray, AABBMin, AABBMax, out Hit, out double TMin, out double TMax);
        }

        public static bool IntersectAABB(Ray Ray, Vector3 AABBMin, Vector3 AABBMax)
        {
            return IntersectAABB(Ray, AABBMin, AABBMax, out Vector3 Hit, out double TMin, out double TMax);
        }

        public static void CalculateTriangleAABB(Vector3[] Verts, out Vector3 AABBMin, out Vector3 AABBMax)
        {
            double MinX = double.MaxValue, MinY = double.MaxValue, MinZ = double.MaxValue;
            double MaxX = double.MinValue, MaxY = double.MinValue, MaxZ = double.MinValue;
            foreach (Vector3 Vert in Verts)
            {
                if (Vert.X < MinX)
                {
                    MinX = Vert.X;
                }
                if (Vert.X > MaxX)
                {
                    MaxX = Vert.X;
                }
                if (Vert.Y < MinY)
                {
                    MinY = Vert.Y;
                }
                if (Vert.Y > MaxY)
                {
                    MaxY = Vert.Y;
                }
                if (Vert.Z < MinZ)
                {
                    MinZ = Vert.Z;
                }
                if (Vert.Z > MaxZ)
                {
                    MaxZ = Vert.Z;
                }
            }
            AABBMin = new Vector3(MinX, MinY, MinZ);
            AABBMax = new Vector3(MaxX, MaxY, MinZ);
        }
        #endregion
    }
}

