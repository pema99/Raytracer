using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Raytracer
{
    public class Raytracer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double FOV { get; private set; }
        public int MaxBounces { get; private set; }
        public int Samples { get; private set; }
        public int Threads { get; private set; }

        private Vector3[,] Framebuffer { get; set; }
        private List<Light> Lights { get; set; }
        private List<Shape> Shapes { get; set; }

        private double InvWidth { get; set; }
        private double InvHeight { get; set; }
        private double AspectRatio { get; set; }
        private double ViewAngle { get; set; }

        public Raytracer(int Width, int Height, double FOV, int MaxBounces, int Samples, int Threads)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.MaxBounces = MaxBounces;
            this.Samples = Samples;
            this.Threads = Threads;
            this.Framebuffer = new Vector3[Width, Height];

            this.InvWidth = 1.0 / Width;
            this.InvHeight = 1.0 / Height;
            this.AspectRatio = (double)Width / (double)Height;
            this.ViewAngle = Math.Tan(MathHelper.Pi * 0.5 * FOV / 180.0);

            Lights = new List<Light>()
            {
                //new Light(new Vector3(0, 2, 4), 10, Color.White.ToVector3()),
            };
            Shapes = new List<Shape>()
            {
                new TriangleMesh(new Material(Color.Red.ToVector3(), 0, 1, Vector3.Zero),
                    new Vector3[] {
                        new Vector3(-1.000000, -1.000000, 1.000000),
                        new Vector3(-1.000000, 1.000000, 1.000000),
                        new Vector3(-1.000000, 1.000000, -1.000000),
                        new Vector3(1.000000, 1.000000, 1.000000),
                        new Vector3(1.000000, 1.000000, -1.000000),
                        new Vector3(1.000000, -1.000000, 1.000000),
                        new Vector3(1.000000, -1.000000, -1.000000),
                        new Vector3(-1.000000, -1.000000, -1.000000)
                    },
                    new int[] {
                        0, 1, 2,
                        1, 3, 4,
                        3, 5, 6,
                        0, 7, 5,
                        7, 2, 4,
                        5, 3, 1,
                        7, 0, 2,
                        2, 1, 4,
                        4, 3, 6,
                        5, 7, 6,
                        6, 7, 4,
                        0, 5, 1
                    }),

                //new Sphere(new Material(Color.White.ToVector3(), 1, 0.01, Vector3.Zero), new Vector3(-2.5, -1, 5), 1),
                //new Sphere(new Material(Color.Green.ToVector3(), 1, 0.3, Vector3.Zero), new Vector3(0, -1, 6), 1),
                //new Sphere(new Material(Color.Blue.ToVector3(), 0, 1, Vector3.Zero), new Vector3(2.5, -1, 5), 1),

                new Plane(new Material(Color.LightGray.ToVector3(), 1, 0.02, Vector3.Zero), new Vector3(0, -2, 5), new Vector3(0, 1, 0)),
                new Plane(new Material(Color.LightBlue.ToVector3(), 0, 1, Vector3.One), new Vector3(0, 5, 5), new Vector3(0, -1, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 0, 1, Vector3.Zero), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 0, 1, Vector3.Zero), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new Material(Color.Pink.ToVector3(),  0, 1, Vector3.Zero), new Vector3(0, 0, 10), new Vector3(0, 0, -1)),
                new Plane(new Material(Color.LightSalmon.ToVector3(), 0, 1, Vector3.Zero), new Vector3(0, 0, -1), new Vector3(0, 0, 1)),
            };

            Matrix M = Matrix.CreateTranslation(2, -1, 6);
            for (int i = 0; i < (Shapes[0] as TriangleMesh).Vertices.Length; i++)
            {
                (Shapes[0] as TriangleMesh).Vertices[i] = Vector3.Transform((Shapes[0] as TriangleMesh).Vertices[i], M);
            }
        }

        public void Render()
        {
            //TODO: Allow thread numbers that the width isn't divisble with
            Thread[] ThreadPool = new Thread[Threads];
            for (int i = 0; i < Threads; i++)
            {
                int j = i;
                ThreadPool[i] = new Thread(() => {
                    RenderLines(j, Width / Threads);
                });
                ThreadPool[i].Start();
            }
            foreach (Thread T in ThreadPool)
            {
                T.Join();
            }
        }

        private void RenderLines(int Start, int Amount)
        {
            for (int x = Start * Amount; x < Start * Amount + Amount; x++)
            {
                Console.WriteLine("Processed line " + x);
                for (int y = 0; y < Height; y++)
                {
                    //Raycast to nearest shape
                    Vector3 RayDir = new Vector3((2.0 * ((x + 0.5) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((y + 0.5) * InvHeight)) * ViewAngle, 1);
                    RayDir.Normalize();

                    //Trace primary ray
                    Framebuffer[x, y] = Trace(new Ray(Vector3.Zero, RayDir), Vector3.Zero, 0);
                }
            }
        }

        public void ExportToFile(string Path, double Gamma = 2.2)
        {
            Bitmap Render = new Bitmap(Width, Height);
            Vector3 Mapped = Vector3.Zero;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //Tone mapping
                    Mapped = Framebuffer[x, y] / (Framebuffer[x, y] + Vector3.One);
                    Mapped = new Vector3(Math.Pow(Mapped.X, 1.0 / Gamma), Math.Pow(Mapped.Y, 1.0 / Gamma), Math.Pow(Mapped.Z, 1.0 / Gamma));

                    //Draw
                    Render.SetPixel(x, y, Mapped.ToColor());
                }
            }
            Render.Save(Path);
        }

        private Vector3 Trace(Ray Ray, Vector3 ViewPosition, int Bounces)
        {
            Vector3 Result = Vector3.Zero;

            //Raycast to nearest geometry, if any
            Raycast(Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal);
            if (FirstShape != null)
            {
                //Area lights
                if (FirstShape.Material.Emission != Vector3.Zero)
                {
                    return FirstShape.Material.Emission;
                }

                //Direct lighting, phong
                Vector3 Direct = Vector3.Zero;
                foreach (Light Light in Lights)
                {
                    Vector3 ShadowRayDirection = Light.Origin - FirstShapeHit;
                    ShadowRayDirection.Normalize();
                    Ray ShadowRay = new Ray(FirstShapeHit + FirstShapeNormal * 0.001, ShadowRayDirection);
                    Raycast(ShadowRay, out Shape FirstShadow, out Vector3 FirstShadowHit, out Vector3 FirstShadowNormal);

                    if (FirstShadow == null || FirstShadow == FirstShape || (FirstShadowHit - FirstShapeHit).Length() > (Light.Origin - FirstShapeHit).Length())
                    {
                        double Distance = (Light.Origin - FirstShapeHit).Length();
                        double Attenuation = 1.0 / (Distance * Distance);
                        double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, ShadowRayDirection), 0);
                        Direct = Light.Intensity * Light.Color * CosTheta * Attenuation;
                    }
                }

                //If we are about to hit max depth, no need to calculate indirect lighting
                if (Bounces >= MaxBounces)
                {
                    return (Direct * FirstShape.Material.Color) / Math.PI;
                }

                //Indirect lighting using monte carlo path tracing
                Vector3 Indirect = Vector3.Zero;
                CreateCartesian(FirstShapeNormal, out Vector3 NT, out Vector3 NB);

                Vector3 TotalDiffuse = Vector3.Zero;
                Vector3 TotalSpecular = Vector3.Zero;

                Vector3 ViewDirection = Vector3.Normalize(ViewPosition - FirstShapeHit);
                Vector3 F0 = Vector3.Lerp(new Vector3(0.04), FirstShape.Material.Color, FirstShape.Material.Metalness);
                Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, FirstShapeNormal);

                for (int i = 0; i < Samples; i++)
                {
                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 Sample = SampleHemisphere(R1, R2);
                    Vector3 SampleWorld = new Vector3(
                        Sample.X * NB.X + Sample.Y * FirstShapeNormal.X + Sample.Z * NT.X,
                        Sample.X * NB.Y + Sample.Y * FirstShapeNormal.Y + Sample.Z * NT.Y,
                        Sample.X * NB.Z + Sample.Y * FirstShapeNormal.Z + Sample.Z * NT.Z);
                    SampleWorld.Normalize();

                    Vector3 SampleRadiance = Trace(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);
                    Vector3 Kd = Vector3.One - Ks;

                    Kd *= 1.0 - FirstShape.Material.Metalness;
                    Vector3 Diffuse = Kd * FirstShape.Material.Color / Math.PI;
                    
                    TotalDiffuse += Diffuse * SampleRadiance * CosTheta / (1.0 / (2.0 * Math.PI));
                }
                for (int i = 0; i < Samples; i++)
                {
                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 SampleWorld = ImportanceSampleGGX(R1, R2, ReflectionDirection, FirstShape.Material.Roughness);

                    Vector3 SampleRadiance = Trace(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

                    double D = GGXDistribution(FirstShapeNormal, Halfway, FirstShape.Material.Roughness);
                    double G = GeometrySmith(FirstShapeNormal, ViewDirection, SampleWorld, FirstShape.Material.Roughness);
                    Vector3 SpecularNumerator = D * G * Ks;
                    double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(FirstShapeNormal, ViewDirection), 0.0) * CosTheta + 0.001;
                    Vector3 Specular = SpecularNumerator / SpecularDenominator;

                    TotalSpecular += Specular * SampleRadiance * CosTheta / (D * Vector3.Dot(FirstShapeNormal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection))+0.0001);
                }

                TotalDiffuse /= (double)Samples;
                TotalSpecular /= (double)Samples;

                Indirect = TotalDiffuse + TotalSpecular;

                Result = (Direct + Indirect);
            }
            return Result;
        }

        private bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal)
        {
            double MinDistance = double.MaxValue;
            FirstShape = null;
            FirstShapeHit = Vector3.Zero;
            FirstShapeNormal = Vector3.Zero;
            foreach (Shape Shape in Shapes)
            {
                if (Shape.Intersect(Ray, out Vector3 Hit, out Vector3 Normal))
                {
                    double Distance = (Hit - Ray.Origin).Length();
                    if (Distance < MinDistance)
                    {
                        MinDistance = Distance;
                        FirstShape = Shape;
                        FirstShapeHit = Hit;
                        FirstShapeNormal = Normal;
                    }
                }
            }
            return FirstShape != null;
        }

        #region MonteCarlo
        private void CreateCartesian(Vector3 Normal, out Vector3 NT, out Vector3 NB)
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

        private Vector3 SampleHemisphere(double R1, double R2)
        {
            double SinTheta = Math.Sqrt(1 - R1 * R1);
            double Phi = 2 * Math.PI * R2;
            double X = SinTheta * Math.Cos(Phi);
            double Z = SinTheta * Math.Sin(Phi);
            return new Vector3(X, R1, Z);
        }

        private Vector3 ImportanceSampleGGX(double R1, double R2, Vector3 ReflectionDirection, double Roughness)
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
        #endregion

        #region PBR
        public double GGXDistribution(Vector3 Normal, Vector3 Halfway, double Roughness)
        {
            double Numerator = Math.Pow(Roughness, 2.0);
            double Denominator = Math.Pow(Math.Max(Vector3.Dot(Normal, Halfway), 0), 2) * (Numerator - 1.0) + 1.0;
            Denominator = Math.Max(Math.PI * Math.Pow(Denominator, 2.0), 1e-7);
            return Numerator / Denominator;
        }

        public double GeometrySchlickGGX(Vector3 Normal, Vector3 View, double Roughness)
        {
            double Numerator = Math.Max(Vector3.Dot(Normal, View), 0);
            double R = (Roughness * Roughness) / 8.0;
            double Denominator = Numerator * (1.0 - R) + R;
            return Numerator / Denominator;
        }

        public double GeometrySmith(Vector3 Normal, Vector3 View, Vector3 Light, double Roughness)
        {
            return GeometrySchlickGGX(Normal, View, Roughness) * GeometrySchlickGGX(Normal, Light, Roughness);
        }

        public Vector3 FresnelSchlick(double CosTheta, Vector3 F0)
        {
            return F0 + (Vector3.One - F0) * Math.Pow((1.0 - CosTheta), 5.0);
        }
        #endregion
    }
}
