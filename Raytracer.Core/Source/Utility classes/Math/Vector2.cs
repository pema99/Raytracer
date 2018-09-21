using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public struct Vector2 : IEquatable<Vector2>
    {
        private static readonly Vector2 zeroVector = new Vector2(0f, 0f);
        private static readonly Vector2 unitVector = new Vector2(1f, 1f);
        private static readonly Vector2 unitXVector = new Vector2(1f, 0f);
        private static readonly Vector2 unitYVector = new Vector2(0f, 1f);

        public static Vector2 Zero { get { return zeroVector; } }
        public static Vector2 One { get { return unitVector; } }
        public static Vector2 UnitX { get { return unitXVector; } }
        public static Vector2 UnitY { get { return unitYVector; } }

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2(double value)
        {
            this.X = value;
            this.Y = value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector2) && Equals((Vector2)obj);
        }

        public bool Equals(Vector2 other)
        {
            return (X == other.X &&
                    Y == other.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public double Length()
        {
            return (double)Math.Sqrt((X * X) + (Y * Y));
        }

        public double LengthSquared()
        {
            return (X * X) + (Y * Y);
        }

        public void Normalize()
        {
            double val = 1.0f / (double)Math.Sqrt((X * X) + (Y * Y));
            X *= val;
            Y *= val;
        }

        public override string ToString()
        {
            return (
                "{X:" + X.ToString() +
                " Y:" + Y.ToString() +
                "}"
            );
        }

        public static Vector2 Add(Vector2 value1, Vector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static Vector2 Barycentric(
            Vector2 value1,
            Vector2 value2,
            Vector2 value3,
            double amount1,
            double amount2
        )
        {
            return new Vector2(
                MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
                MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2)
            );
        }

        public static Vector2 CatmullRom(
            Vector2 value1,
            Vector2 value2,
            Vector2 value3,
            Vector2 value4,
            double amount
        )
        {
            return new Vector2(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount)
            );
        }

        public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
        {
            return new Vector2(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y)
            );
        }

        public static double Distance(Vector2 value1, Vector2 value2)
        {
            double v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return (double)Math.Sqrt((v1 * v1) + (v2 * v2));
        }

        public static double DistanceSquared(Vector2 value1, Vector2 value2)
        {
            double v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return (v1 * v1) + (v2 * v2);
        }

        public static Vector2 Divide(Vector2 value1, Vector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static Vector2 Divide(Vector2 value1, double divider)
        {
            double factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }

        public static double Dot(Vector2 value1, Vector2 value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        public static Vector2 Lerp(Vector2 value1, Vector2 value2, double amount)
        {
            return new Vector2(
                MathHelper.Lerp(value1.X, value2.X, amount),
                MathHelper.Lerp(value1.Y, value2.Y, amount)
            );
        }

        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                value1.X > value2.X ? value1.X : value2.X,
                value1.Y > value2.Y ? value1.Y : value2.Y
            );
        }

        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(
                value1.X < value2.X ? value1.X : value2.X,
                value1.Y < value2.Y ? value1.Y : value2.Y
            );
        }

        public static Vector2 Multiply(Vector2 value1, Vector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static Vector2 Multiply(Vector2 value1, double scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        public static Vector2 Negate(Vector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static Vector2 Normalize(Vector2 value)
        {
            double val = 1.0f / (double)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
            value.X *= val;
            value.Y *= val;
            return value;
        }

        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            Vector2 result = Vector2.Zero;
            double val = 2.0f * ((vector.X * normal.X) + (vector.Y * normal.Y));
            result.X = vector.X - (normal.X * val);
            result.Y = vector.Y - (normal.Y * val);
            return result;
        }

        public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, double amount)
        {
            return new Vector2(
                MathHelper.SmoothStep(value1.X, value2.X, amount),
                MathHelper.SmoothStep(value1.Y, value2.Y, amount)
            );
        }

        public static Vector2 Subtract(Vector2 value1, Vector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static Vector2 Transform(Vector2 position, Matrix matrix)
        {
            return new Vector2(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42
            );
        }

        public static void Transform(
            Vector2[] sourceArray,
            ref Matrix matrix,
            Vector2[] destinationArray
        )
        {
            Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
        }

        public static void Transform(
            Vector2[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector2[] destinationArray,
            int destinationIndex,
            int length
        )
        {
            for (int x = 0; x < length; x += 1)
            {
                Vector2 position = sourceArray[sourceIndex + x];
                Vector2 destination = destinationArray[destinationIndex + x];
                destination.X = (position.X * matrix.M11) + (position.Y * matrix.M21)
                        + matrix.M41;
                destination.Y = (position.X * matrix.M12) + (position.Y * matrix.M22)
                        + matrix.M42;
                destinationArray[destinationIndex + x] = destination;
            }
        }

        public static Vector2 TransformNormal(Vector2 normal, Matrix matrix)
        {
            return new Vector2(
                (normal.X * matrix.M11) + (normal.Y * matrix.M21),
                (normal.X * matrix.M12) + (normal.Y * matrix.M22)
            );
        }

        public static void TransformNormal(
            Vector2[] sourceArray,
            ref Matrix matrix,
            Vector2[] destinationArray
        )
        {
            TransformNormal(
                sourceArray,
                0,
                ref matrix,
                destinationArray,
                0,
                sourceArray.Length
            );
        }

        public static void TransformNormal(
            Vector2[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector2[] destinationArray,
            int destinationIndex,
            int length
        )
        {
            for (int i = 0; i < length; i += 1)
            {
                Vector2 position = sourceArray[sourceIndex + i];
                Vector2 result = Vector2.Zero;
                result.X = (position.X * matrix.M11) + (position.Y * matrix.M21);
                result.Y = (position.X * matrix.M12) + (position.Y * matrix.M22);
                destinationArray[destinationIndex + i] = result;
            }
        }

        public static Vector2 operator -(Vector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static bool operator ==(Vector2 value1, Vector2 value2)
        {
            return (value1.X == value2.X &&
                    value1.Y == value2.Y);
        }

        public static bool operator !=(Vector2 value1, Vector2 value2)
        {
            return !(value1 == value2);
        }

        public static Vector2 operator +(Vector2 value1, Vector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static Vector2 operator -(Vector2 value1, Vector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static Vector2 operator *(Vector2 value1, Vector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static Vector2 operator *(Vector2 value, double scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2 operator *(double scaleFactor, Vector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2 operator /(Vector2 value1, Vector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static Vector2 operator /(Vector2 value1, double divider)
        {
            double factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }
    }
}
