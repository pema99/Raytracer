using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public static class Util
    {
        public static bool SolveQuadratic(float A, float B, float C, out float T0, out float T1)
        {
            float D = B * B - 4 * A * C;
            if (D < 0)
            {
                T0 = T1 = 0;
                return false;
            }
            else if (D == 0)
            {
                T0 = T1 = -0.5f * B / A;
            }
            else
            {
                float Q = 0;
                if (B > 0)
                {
                    Q = -0.5f * (B + (float)Math.Sqrt(D));
                }
                else
                {
                    Q = -0.5f * (B - (float)Math.Sqrt(D));
                }
                T0 = Q / A;
                T1 = C / Q;
            }
            if (T0 > T1)
            {
                float Temp = T1;
                T1 = T0;
                T0 = Temp;
            }
            return true;
        }

        public static Vector3 ToVector3(this Color Color)
        {
            return new Vector3(Color.R/255f, Color.G/255f, Color.B/255f);
        }

        public static Vector4 ToVector4(this Color Color)
        {
            return new Vector4(Color.R/255f, Color.G/255f, Color.B/255f, Color.A/255f);
        }

        public static Color ToColor(this Vector3 Color)
        {
            return new Color(Color);
        }

        public static Color ToColor(this Vector4 Color)
        {
            return new Color(Color);
        }

        public static Color Multiply(this Color Color, Color Other)
        {
            return new Color(MathHelper.Clamp(Color.R * Other.R, 0, 255), MathHelper.Clamp(Color.G * Other.G, 0, 255), MathHelper.Clamp(Color.B * Color.B, 0, 255), MathHelper.Clamp(Color.A * Color.A, 0, 255));
        }
    }
}
