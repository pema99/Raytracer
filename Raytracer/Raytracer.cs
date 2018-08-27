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
                new Sphere(new Material(Color.Red.ToVector3(), Vector3.Zero), new Vector3(-2.5, -1, 5), 1),
                new Sphere(new Material(Color.Green.ToVector3(), Vector3.Zero), new Vector3(0, -1, 6), 1),
                new Sphere(new Material(Color.Blue.ToVector3(), Vector3.Zero), new Vector3(2.5, -1, 5), 1),

                new Plane(new Material(Color.LightGray.ToVector3(), Vector3.Zero), new Vector3(0, -2, 5), new Vector3(0, 1, 0)),
                new Plane(new Material(Color.LightBlue.ToVector3(), Vector3.One), new Vector3(0, 5, 5), new Vector3(0, -1, 0)),
                new Plane(new Material(Color.Green.ToVector3(), Vector3.Zero), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new Material(Color.Green.ToVector3(), Vector3.Zero), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new Material(Color.Pink.ToVector3(),  Vector3.Zero), new Vector3(0, 0, 10), new Vector3(0, 0, -1)),
                new Plane(new Material(Color.LightSalmon.ToVector3(), Vector3.Zero), new Vector3(0, 0, -1), new Vector3(0, 0, 1))
            };
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
                    Framebuffer[x, y] = Trace(new Ray(Vector3.Zero, RayDir), 0);
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
                    Mapped = Framebuffer[x, y] / (Framebuffer[x, y] + Vector3.One);
                    Mapped = new Vector3(Math.Pow(Mapped.X, 1.0 / Gamma), Math.Pow(Mapped.Y, 1.0 / Gamma), Math.Pow(Mapped.Z, 1.0 / Gamma));
                    Render.SetPixel(x, y, Mapped.ToColor());
                }
            }
            Render.Save(Path);
        }

        private Vector3 Trace(Ray Ray, int Bounces)
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
                for (int i = 0; i < Samples; i++)
                {
                    Vector3 Sample = SampleHemisphere();
                    Vector3 SampleWorld = new Vector3(
                        Sample.X * NB.X + Sample.Y * FirstShapeNormal.X + Sample.Z * NT.X,
                        Sample.X * NB.Y + Sample.Y * FirstShapeNormal.Y + Sample.Z * NT.Y,
                        Sample.X * NB.Z + Sample.Y * FirstShapeNormal.Z + Sample.Z * NT.Z);
                    Indirect += Math.Max(Vector3.Dot(SampleWorld, FirstShapeNormal), 0) * Trace(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), Bounces + 1);
                }
                Indirect /= (float)Samples * (1.0 / (2.0 * Math.PI));

                Result = (Direct + Indirect) * FirstShape.Material.Color / Math.PI;
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

        private void CreateCartesian(Vector3 Normal, out Vector3 NT, out Vector3 NB)
        {
            if (Math.Abs(Normal.X) > Math.Abs(Normal.Y))
            {
                NT = new Vector3(Normal.Z, 0, -Normal.X) / Math.Sqrt(Normal.X * Normal.X + Normal.Z * Normal.Z);
            }
            else
            {
                NT = new Vector3(0, -Normal.Z, Normal.Y) / Math.Sqrt(Normal.Y * Normal.Y + Normal.Z * Normal.Z);
            }
            NB = Vector3.Cross(Normal, NT);
        }

        private Vector3 SampleHemisphere()
        {
            double R1 = Util.Random.NextDouble();
            double R2 = Util.Random.NextDouble();
            double SinTheta = Math.Sqrt(1 - R1 * R1);
            double Phi = 2 * Math.PI * R2;
            double X = SinTheta * Math.Cos(Phi);
            double Z = SinTheta * Math.Sin(Phi);
            return new Vector3(X, R1, Z);
        }
    }
}
