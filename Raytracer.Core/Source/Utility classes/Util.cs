using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Raytracer.Core
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

        public static double SumComponents(this Vector3 Vec)
        {
            return Vec.X + Vec.Y + Vec.Z;
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

        public static double BalanceHeuristic(double PDF0, double PDF1)
        {
            return PDF0 / (PDF0 + PDF1);
        }

        public static double PowerHeuristic(double PDF0, double PDF1)
        {
            return PDF0 * PDF0 / (PDF0 * PDF0 + PDF1 * PDF1);
        }
        #endregion

        #region Parsing utility
        public static Vector3 ToVector3(this JToken T)
        {
            return new Vector3((double)T[0], (double)T[1], (double)T[2]);
        }

        public static Vector2 ToVector2(this JToken T)
        {
            return new Vector2((double)T[0], (double)T[1]);
        }

        public static MaterialNode ToMaterialNode(this JToken T)
        {
            if (T == null)
            {
                return null;
            }
            if (T is JArray)
            {
                return new MaterialConstantNode(T.ToVector3());
            }
            else
            {
                if (double.TryParse((string)T, NumberStyles.Any, CultureInfo.InvariantCulture, out double Result))
                {
                    return new MaterialConstantNode(Result);
                }
                return new MaterialTextureNode(new Texture((string)T));
            }
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

        public static Vector3 UniformSampleSphere(double R1, double R2)
        {
            double CosPhi = 2.0f * Util.Random.NextDouble() - 1.0f;
            double SinPhi = Math.Sqrt(1.0f - CosPhi * CosPhi);
            double Theta = 2 * Math.PI * Util.Random.NextDouble();

            return new Vector3(SinPhi * Math.Sin(Theta), CosPhi, SinPhi * Math.Cos(Theta));
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

        public static Vector2 UniformSampleDisc(double R1, double R2)
        {
            double Theta = R2 * Math.PI * 2;
            return new Vector2(Math.Sqrt(R1) * Math.Cos(Theta), Math.Sqrt(R1) * Math.Sin(Theta));
        }

        public static double HeronsFormula(double SideA, double SideB, double SideC)
        {
            double S = (SideA + SideB + SideC) * 0.5;
            return Math.Sqrt(S * (S - SideA) * (S - SideB) * (S - SideC));
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
            AABBMax = new Vector3(MaxX, MaxY, MaxZ);
        }
        #endregion

        #region Shading
        public static Vector3 SampleGGX(double R1, double R2, Vector3 ReflectionDirection, double Roughness)
        {
            double A = Math.Pow(Roughness, 2.0);

            //Generate spherical
            double Phi = 2.0 * Math.PI * R1;
            double CosTheta = Math.Sqrt((1.0 - R2) / (1.0 + (A * A - 1.0) * R2));
            double SinTheta = Math.Sqrt(1.0 - CosTheta * CosTheta);

            //Spherical to cartesian
            Vector3 H = new Vector3(Math.Cos(Phi) * SinTheta, Math.Sin(Phi) * SinTheta, CosTheta);

            //Tangent-space to world-space
            Vector3 Up = Math.Abs(ReflectionDirection.Z) < 0.999 ? new Vector3(0.0, 0.0, 1.0) : new Vector3(1.0, 0.0, 0.0);
            Vector3 Tangent = Vector3.Normalize(Vector3.Cross(Up, ReflectionDirection));
            Vector3 BiTangent = Vector3.Cross(ReflectionDirection, Tangent);

            return Vector3.Normalize(Tangent * H.X + BiTangent * H.Y + ReflectionDirection * H.Z);
        }

        public static double GGXDistribution(Vector3 Normal, Vector3 Halfway, double Roughness)
        {
            double Numerator = Math.Pow(Roughness, 2.0);
            double Denominator = Math.Pow(Math.Max(Vector3.Dot(Normal, Halfway), 0), 2) * (Numerator - 1.0) + 1.0;
            Denominator = Math.Max(Math.PI * Math.Pow(Denominator, 2.0), 1e-7);
            return Numerator / Denominator;
        }

        public static double GeometrySchlickGGX(Vector3 Normal, Vector3 View, double Roughness)
        {
            double Numerator = Math.Max(Vector3.Dot(Normal, View), 0);
            double R = (Roughness * Roughness) / 8.0;
            double Denominator = Numerator * (1.0 - R) + R;
            return Numerator / Denominator;
        }

        public static double GeometrySmith(Vector3 Normal, Vector3 View, Vector3 Light, double Roughness)
        {
            return GeometrySchlickGGX(Normal, View, Roughness) * GeometrySchlickGGX(Normal, Light, Roughness);
        }

        public static Vector3 FresnelSchlick(double CosTheta, Vector3 F0)
        {
            return F0 + (Vector3.One - F0) * Math.Pow((1.0 - CosTheta), 5.0);
        }

        public static double FresnelReal(double CosTheta, double RefractiveIndex)
        {
            double RefractiveIndexA = 1;
            double RefractiveIndexB = RefractiveIndex;

            if (CosTheta > 0)
            {
                var Temp = RefractiveIndexA;
                RefractiveIndexA = RefractiveIndexB;
                RefractiveIndexB = Temp;
            }

            double SinOut = RefractiveIndexA / RefractiveIndexB * Math.Sqrt(Math.Max(0, 1 - Math.Pow(CosTheta, 2)));

            if (SinOut >= 1)
            {
                return 1;
            }
            else
            {
                double CosOut = Math.Sqrt(Math.Max(0, 1 - SinOut * SinOut));
                CosTheta = Math.Abs(CosTheta);
                double Rs = ((RefractiveIndexB * CosTheta) - (RefractiveIndexA * CosOut)) / ((RefractiveIndexB * CosTheta) + (RefractiveIndexA * CosOut));
                double Rp = ((RefractiveIndexA * CosTheta) - (RefractiveIndexB * CosOut)) / ((RefractiveIndexA * CosTheta) + (RefractiveIndexB * CosOut));
                return (Rs * Rs + Rp * Rp) / 2;
            }
        }
        #endregion
    }
}

