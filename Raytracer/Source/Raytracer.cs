using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Raytracer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double FOV { get; private set; }
        public Vector3 CameraPosition { get; private set; }
        public Vector3 CameraRotation { get; private set; }
        public Texture SkyBox { get; set; }
        public int MinBounces { get; private set; }
        public int MaxBounces { get; private set; }
        public int Samples { get; private set; }
        public int Threads { get; private set; }

        private Vector3[,] Framebuffer { get; set; }
        private List<Shape> Shapes { get; set; }

        private double InvWidth { get; set; }
        private double InvHeight { get; set; }
        private double AspectRatio { get; set; }
        private double ViewAngle { get; set; }
        private Matrix CameraRotationMatrix { get; set; }

        public Raytracer(int Width, int Height, double FOV, Vector3 CameraPosition, Vector3 CameraRotation, Texture SkyBox, int MinBounces, int MaxBounces, int Samples, int Threads)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.SkyBox = SkyBox;
            this.MinBounces = MinBounces;
            this.MaxBounces = MaxBounces;
            this.Samples = Samples;
            this.Threads = Threads;
            this.Framebuffer = new Vector3[Width, Height];

            this.InvWidth = 1.0 / Width;
            this.InvHeight = 1.0 / Height;
            this.AspectRatio = (double)Width / (double)Height;
            this.ViewAngle = Math.Tan(MathHelper.Pi * 0.5 * FOV / 180.0);
            this.CameraRotationMatrix = Matrix.CreateRotationX(CameraRotation.X) * Matrix.CreateRotationY(CameraRotation.Y) * Matrix.CreateRotationZ(CameraRotation.Z);

            //Setup scene
            Shapes = new List<Shape>()
            {               
                //new TriangleMesh(new Material("Cerberus"), Matrix.CreateScale(5) * Matrix.CreateRotationY(Math.PI/2) * Matrix.CreateTranslation(-2, 0.5, 5), "Assets/Meshes/Gun.ply", 3, true, false),
                //new TriangleMesh(new Material(Color.White.ToVector3(), 0, 0, Vector3.Zero), Matrix.CreateScale(2) * Matrix.CreateTranslation(0, 0, 5), "Assets/meshes/monkeysmooth.ply", 3, true, false),

                //new TriangleMesh(new GlassMaterial(new Vector3(1, 1, 1), 1.35, new IsotropicMedium(new Vector3(0.9801986733, 0.4609674656, 0.4334596545), 0.15, 0.15)), Matrix.CreateScale(2) * Matrix.CreateRotationY(Math.PI/2) * Matrix.CreateTranslation(0, 0, 5), "Assets/meshes/strange.ply", 3, false, false),

                new Sphere(new PBRMaterial("rustediron2"), new Vector3(0, 0, 4), 1),
                new TriangleMesh(new PBRMaterial(Color.Red.ToVector3(), 0, 1), Matrix.CreateRotationY(Math.PI-0.4) * Matrix.CreateTranslation(0, 0, 4), "Assets/meshes/ballcover.ply", 3, true, true),
                new Plane(new PBRMaterial(Vector3.One, 1, 0.2), new Vector3(0, -0.98, 0), new Vector3(0, 1, 0)),

                //new Sphere(new GlassMaterial(new Vector3(1, 1, 1), 1.3, new IsotropicMedium(new Vector3(0.5, 1, 0.5), 1, 0.4)), new Vector3(0, 0, 6), 2),
                //new Sphere(new EmissionMaterial(new Vector3(1, 0, 0)*2), new Vector3(-2, -2, 9), 2),
                //new TriangleMesh(new GlassMaterial(new Vector3(1, 1, 1), 1.35, new IsotropicMedium(new Vector3(0.9801986733, 0.4609674656, 0.4334596545), 0.15, 0.15)), Matrix.CreateRotationY(Math.PI) * Matrix.CreateScale(27) * Matrix.CreateTranslation(0, -3, 4), "Assets/meshes/dragon_vrip.ply", 3, false, false),

                //new Sphere(new PBRMaterial("wornpaintedcement"), new Vector3(-2.5, -0.5, 5), 1.5),
                //new Sphere(new PBRMaterial("rustediron2"), new Vector3(2.5, -0.5, 5), 1.5),

                //new Plane(new PBRMaterial(Color.LightGray.ToVector3(), 0, 1), new Vector3(0, -2, 5), new Vector3(0, 1, 0)),
                //new Plane(new EmissionMaterial(Vector3.One), new Vector3(0, 5, 5), new Vector3(0, -1, 0)),
                //new Plane(new PBRMaterial(Color.Green.ToVector3(), 0, 1), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                //new Plane(new PBRMaterial(Color.Green.ToVector3(), 0, 1), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                //new Plane(new PBRMaterial(Color.Pink.ToVector3(),  0, 1), new Vector3(0, 0, 10), new Vector3(0, 0, -1)),
                //new Plane(new PBRMaterial(Color.Black.ToVector3(), 0, 1), new Vector3(0, 0, -1), new Vector3(0, 0, 1)),
            };
        }

        public void Render()
        {
            int Progress = 0;
            using (new Timer(_ => Console.WriteLine("Rendered {0} of {1} scanlines", Progress, Width), null, 1000, 1000))
            {
                Parallel.For(0, Width, new ParallelOptions { MaxDegreeOfParallelism = Threads }, (i) => { RenderLine(i); Progress++; });
            }
        }

        private void RenderLine(int x)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int i = 0; i < Samples; i++)
                {
                    //Supersampling
                    Vector3 RayDir = new Vector3((2.0 * ((x + 0.5 + Util.Random.NextDouble()) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((y + 0.5 + Util.Random.NextDouble()) * InvHeight)) * ViewAngle, 1);
                    RayDir.Normalize();
                    RayDir = Vector3.Transform(RayDir, CameraRotationMatrix);

                    //Trace primary ray
                    Framebuffer[x, y] += Trace(new Ray(CameraPosition, RayDir));
                }
                Framebuffer[x, y] /= Samples;
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

        private Vector3 Trace(Ray Ray)
        {
            Medium CurrentMedium = null;

            Vector3 FinalColor = Vector3.Zero;
            Vector3 Throughput = Vector3.One;

            for (int Bounce = 0; Bounce < MaxBounces; Bounce++)
            {
                //Raycast to nearest geometry, if any
                Raycast(Ray, out Shape Shape, out Vector3 Hit, out Vector3 Normal, out Vector2 UV);

                //Return skybox color if nothing hit
                if (Shape == null)
                {
                    if (SkyBox == null)
                    {
                        FinalColor += Throughput * Vector3.Zero;
                    }
                    else
                    {
                        FinalColor += Throughput * SkyBox.GetColorAtUV(new Vector2(1 - (1.0 + Math.Atan2(Ray.Direction.Z, Ray.Direction.X) / MathHelper.Pi) * 0.5, 1 - Math.Acos(Ray.Direction.Y) / MathHelper.Pi));
                    }
                    break;
                }

                bool Scattered = false;
                if (CurrentMedium != null)
                {
                    double MaxDistance = (Hit - Ray.Origin).Length();

                    double Distance = CurrentMedium.SampleDistance(MaxDistance);
                    Vector3 Transmission = CurrentMedium.Transmission(Distance);
                    Throughput *= Transmission;

                    if (Distance < MaxDistance)
                    {
                        Scattered = true;

                        Ray.Direction = CurrentMedium.SampleDirection(Ray.Direction);
                        Ray.Origin = Hit + Ray.Direction * 0.001;
                    }
                }

                if (!Scattered)
                {
                    //Area lights
                    if (Shape.Material.HasProperty("emission"))
                    {
                        FinalColor += Throughput * Shape.Material.GetProperty("emission", UV).Color;
                        break;
                    }

                    Shape.Material.Evaluate(Vector3.Normalize(Ray.Origin - Hit), Normal, UV, out Vector3 SampleDirection, out LobeType SampledLobe, out Vector3 Attenuation);

                    //Accumulate BXDF attenuation
                    Throughput *= Attenuation;

                    if (SampledLobe == LobeType.SpecularTransmission)
                    {
                        CurrentMedium = Shape.Material.Medium;
                    }

                    //Set new ray direction to sampled ray
                    Ray.Origin = Hit + SampleDirection * 0.001;
                    Ray.Direction = SampleDirection;
                }

                //Russian roulette
                if (Bounce >= MinBounces)
                {
                    double Prob = Math.Max(Throughput.X, Math.Max(Throughput.Y, Throughput.Z));
                    if (Util.Random.NextDouble() > Prob)
                    {
                        break;
                    }
                    Throughput *= 1 / Prob;
                }
            }

            return FinalColor;
        }

        private bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal, out Vector2 FirstShapeUV)
        {
            double MinDistance = double.MaxValue;
            FirstShape = null;
            FirstShapeHit = Vector3.Zero;
            FirstShapeNormal = Vector3.Zero;
            FirstShapeUV = Vector2.Zero;
            foreach (Shape Shape in Shapes)
            {
                if (Shape.Intersect(Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV))
                {
                    double Distance = (Hit - Ray.Origin).Length();
                    if (Distance < MinDistance)
                    {
                        MinDistance = Distance;
                        FirstShape = Shape;
                        FirstShapeHit = Hit;
                        FirstShapeNormal = Normal;
                        FirstShapeUV = UV;
                    }
                }
            }
            return FirstShape != null;
        }
    }
}
