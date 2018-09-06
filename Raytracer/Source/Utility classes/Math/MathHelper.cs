using System;

namespace Raytracer
{
    public static class MathHelper
    {
        public const double E = Math.E;
        public const double Log10E = 0.4342945;
        public const double Log2E = 1.442695;
        public const double Pi = Math.PI;
        public const double PiOver2 = (Math.PI / 2.0);
        public const double PiOver4 = (Math.PI / 4.0);
        public const double TwoPi = (Math.PI * 2.0);

        public static double Barycentric(double value1, double value2, double value3, double amount1, double amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        public static double CatmullRom(double value1, double value2, double value3, double value4, double amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precission
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;
            return (0.5 * (2.0 * value2 +
                (value3 - value1) * amount +
                (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        public static double Clamp(double value, double min, double max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static double Distance(double value1, double value2)
        {
            return Math.Abs(value1 - value2);
        }

        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0.0)
                result = value1;
            else if (amount == 1.0)
                result = value2;
            else
                result = (2.0 * v1 - 2.0 * v2 + t2 + t1) * sCubed +
                    (3.0 * v2 - 3.0 * v1 - 2.0 * t1 - t2) * sSquared +
                    t1 * s +
                    v1;
            return result;
        }

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double LerpPrecise(double value1, double value2, double amount)
        {
            return ((1 - amount) * value1) + (value2 * amount);
        }

        public static double Max(double value1, double value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        public static int Max(int value1, int value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        public static double Min(double value1, double value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        public static int Min(int value1, int value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        public static double SmoothStep(double value1, double value2, double amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            double result = MathHelper.Clamp(amount, 0.0, 1.0);
            result = MathHelper.Hermite(value1, 0.0, value2, 0.0, result);

            return result;
        }

        public static double ToDegrees(double radians)
        {
            return (radians * 57.295779513082320876798154814105);
        }

        public static double ToRadians(double degrees)
        {
            return (degrees * 0.017453292519943295769236907684886);
        }

        public static double WrapAngle(double angle)
        {
            if ((angle > -Pi) && (angle <= Pi))
                return angle;
            angle %= TwoPi;
            if (angle <= -Pi)
                return angle + TwoPi;
            if (angle > Pi)
                return angle - TwoPi;
            return angle;
        }

        public static bool IsPowerOfTwo(int value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }
    }
}
