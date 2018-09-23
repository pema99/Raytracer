﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class Raytracer
    {
        private int _Width;
        public int Width
        {
            get { return _Width; }
            set
            {
                _Width = value;
                InvWidth = 1.0 / value;
                AspectRatio = (double)value / (double)Height;
            }
        }

        private int _Height;
        public int Height
        {
            get { return _Height; }
            set
            {
                _Height = value;
                InvHeight = 1.0 / value;
                AspectRatio = (double)Width / (double)value;
            }
        }

        private double _FOV;
        public double FOV
        {
            get { return _FOV; }
            set
            {
                _FOV = value;
                ViewAngle = Math.Tan(MathHelper.Pi * 0.5 * value / 180.0);
            }
        }

        private Vector3 _CameraRotation;
        public Vector3 CameraRotation
        {
            get { return _CameraRotation; }
            set
            {
                _CameraRotation = value;
                CameraRotationMatrix = Matrix.CreateRotationX(value.X) * Matrix.CreateRotationY(value.Y) * Matrix.CreateRotationZ(value.Z);
            }
        }

        public Vector3 CameraPosition { get; set; }
        public Texture SkyBox { get; set; }
        public int MinBounces { get; set; }
        public int MaxBounces { get; set; }
        public int Samples { get; set; }
        public int Threads { get; set; }

        public Vector3[,] Framebuffer { get; private set; }

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
            this.CameraPosition = CameraPosition;
            this.CameraRotation = CameraRotation;
            this.SkyBox = SkyBox;
            this.MinBounces = MinBounces;
            this.MaxBounces = MaxBounces;
            this.Samples = Samples;
            this.Threads = Threads;

            //Setup scene
            Shapes = new List<Shape>()
            {               
                //new TriangleMesh(new Material("Cerberus"), Matrix.CreateScale(5) * Matrix.CreateRotationY(Math.PI/2) * Matrix.CreateTranslation(-2, 0.5, 5), "Assets/Meshes/Gun.ply", 3, true, false),
                //new TriangleMesh(new Material(Color.White.ToVector3(), 0, 0, Vector3.Zero), Matrix.CreateScale(2) * Matrix.CreateTranslation(0, 0, 5), "Assets/meshes/monkeysmooth.ply", 3, true, false),

                //new TriangleMesh(new GlassMaterial(new Vector3(1, 1, 1), 1.35, new IsotropicMedium(new Vector3(0.9801986733, 0.4609674656, 0.4334596545), 0.15, 0.15)), Matrix.CreateScale(25) * Matrix.CreateRotationY(Math.PI) * Matrix.CreateTranslation(0, -2, 5), "Assets/meshes/dragon_vrip.ply", 3, false, false),
                new TriangleMesh(new GlassMaterial(Vector3.One, 1.1, 0.2), Matrix.CreateScale(25) * Matrix.CreateRotationY(Math.PI) * Matrix.CreateTranslation(0, -2.5, 5), "Assets/meshes/dragon_vrip.ply", 3, false, false),

                //new Sphere(new PBRMaterial("rustediron2"), new Vector3(0, 0, 4), 1),
                //new TriangleMesh(new VelvetMaterial(0.65, Color.Red.ToVector3()), Matrix.CreateRotationY(Math.PI-0.4) * Matrix.CreateTranslation(0, 0, 4), "Assets/meshes/ballcover.ply", 3, true, true),
                //new Plane(new PBRMaterial(Vector3.One, 1, 0.2), new Vector3(0, -0.98, 0), new Vector3(0, 1, 0)),

                //new TriangleMesh(new GlassMaterial(new Vector3(0.8, 1, 0.8), 1.1), Matrix.CreateScale(0.8) * Matrix.CreateTranslation(0, -2, 6), "Assets/Meshes/Coffee/Cup.ply", 3, true, false),
                //new TriangleMesh(new PBRMaterial(Vector3.One, 0, 0.1), Matrix.CreateScale(0.8) * Matrix.CreateTranslation(0, -2.01, 6), "Assets/Meshes/Coffee/Plate.ply", 3, true, true),
                //new Plane(new PBRMaterial(Color.Brown.ToVector3(), 0, 1), new Vector3(0, -2, 0), new Vector3(0, 1, 0)),

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
            Framebuffer = new Vector3[Width, Height];

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

        public void ExportToFile(string Path)
        {
            Bitmap Render = new Bitmap(Width, Height);
            Vector3 Mapped = Vector3.Zero;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //Tone mapping
                    Mapped = Framebuffer[x, y] / (Framebuffer[x, y] + Vector3.One);
                    Mapped = new Vector3(Math.Pow(Mapped.X, 1.0 / 2.2), Math.Pow(Mapped.Y, 1.0 / 2.2), Math.Pow(Mapped.Z, 1.0 / 2.2));

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
                        FinalColor += Throughput * Vector3.One;
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

                    if (SampledLobe == LobeType.SpecularTransmission || SampledLobe == LobeType.DiffuseTransmission)
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
