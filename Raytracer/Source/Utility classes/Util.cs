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
        public static void CreateCartesian(Vector3 Normal, out Vector3 NT, out Vector3 NB)
        {
            if (Math.Abs(Normal.X) > Math.Abs(Normal.Y))
            {
                NT = Vector3.Normalize(new Vector3(Normal.Z, 0, -Normal.X));
            }
            else
            {
                NT = Vector3.Normalize(new Vector3(0, -Normal.Z, Normal.Y));
            }
            NB = Vector3.Cross(Normal, NT);
        }

        public static Vector3 UniformSampleHemisphere(double R1, double R2)
        {
            double SinTheta = Math.Sqrt(1 - R1 * R1);
            double Phi = 2 * Math.PI * R2;
            double X = SinTheta * Math.Cos(Phi);
            double Z = SinTheta * Math.Sin(Phi);
            return new Vector3(X, R1, Z);
        }

        public static Vector3 CosineSampleHemisphere(double R1, double R2)
        {
            double Theta = Math.Acos(Math.Sqrt(R1));
            double Phi = 2.0 * Math.PI * R2;

            return new Vector3(Math.Sin(Theta) * Math.Cos(Phi), Math.Cos(Theta), Math.Sin(Theta) * Math.Sin(Phi));
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

