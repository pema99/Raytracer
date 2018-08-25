using System;
using System.Drawing;

namespace Raytracer
{
    public static class Util
    {
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

        public static Vector3 ToVector3(this Color Color)
        {
            return new Vector3(Color.R / 255.0, Color.G / 255.0, Color.B / 255.0);
        }

        public static Color ToColor(this Vector3 Color)
        {
            return System.Drawing.Color.FromArgb((int)(Color.X * 255.0), (int)(Color.Y * 255.0), (int)(Color.Z * 255.0));
        }
    }
}
