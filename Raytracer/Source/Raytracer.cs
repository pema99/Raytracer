using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Raytracer(int Width, int Height, double FOV, Vector3 CameraPosition, Vector3 CameraRotation, Texture EnvMap, int MinBounces, int MaxBounces, int Samples, int Threads)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.SkyBox = EnvMap;
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
                //new TriangleMesh(new Material(new Vector3(0.7, 1, 0.7), 1, 0.1, Vector3.Zero, 0, 1.3), Matrix.CreateScale(18) * Matrix.CreateTranslation(0.5, -3, 4), "Assets/meshes/dragon_vrip.ply", 3, false, false),

                new Sphere(new Material("wornpaintedcement"), new Vector3(-2.5, -0.5, 5), 1.5),
                new Sphere(new Material("rustediron2"), new Vector3(2.5, -0.5, 5), 1.5),

                //new Sphere(new Material(Color.Green.ToVector3(), 1, 0, Vector3.Zero), new Vector3(2.5, 0, 5), 1.5),
                //new Sphere(new Material(Color.Red.ToVector3(), 0, 0.1, Vector3.Zero), new Vector3(-0.5, -0.5, 6), 1.5),

                new Plane(new Material(Color.LightGray.ToVector3(), 0, 1, Vector3.Zero), new Vector3(0, -2, 5), new Vector3(0, 1, 0)),
                new Plane(new Material(Color.LightBlue.ToVector3(), 0, 1, Vector3.One), new Vector3(0, 5, 5), new Vector3(0, -1, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 0, 1, Vector3.Zero), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 0, 1, Vector3.Zero), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new Material(Color.Pink.ToVector3(),  0, 1, Vector3.Zero), new Vector3(0, 0, 10), new Vector3(0, 0, -1)),
                new Plane(new Material(Color.Black.ToVector3(), 0, 1, Vector3.Zero), new Vector3(0, 0, -1), new Vector3(0, 0, 1)),
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
                    Framebuffer[x, y] += Trace(new Ray(CameraPosition, RayDir), 0, Vector3.One);
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

        #region Raytracing
        private Vector3 Trace(Ray Ray, int Bounces, Vector3 Throughput)
        {
            //If we are about to hit max depth, no need to calculate lighting
            if (Bounces >= MaxBounces)
            {
                return Vector3.Zero;
            }

            //Raycast to nearest geometry, if any
            Raycast(Ray, out Shape Shape, out Vector3 Hit, out Vector3 Normal, out Vector2 UV);
            if (Shape != null)
            {
                //Area lights
                Vector3 Emission = Shape.Material.GetEmission(UV);
                if (Emission != Vector3.Zero)
                {
                    return Emission;
                }

                Vector3 FinalColor = Vector3.Zero;

                Vector3 Albedo = Shape.Material.GetAlbedo(UV);

                //Glass BXDF
                if (Util.Random.NextDouble() <= Shape.Material.GetTransparency(UV))
                {
                    double RefractiveIndex = Shape.Material.GetRefractiveIndex(UV);
                    if (Util.Random.NextDouble() <= FresnelReal(MathHelper.Clamp(Vector3.Dot(Ray.Direction, Normal), -1, 1), RefractiveIndex))
                    {
                        Vector3 ReflectionDirection = Vector3.Normalize(Vector3.Reflect(Ray.Direction, Normal));
                        Throughput *= Albedo;
                        FinalColor = Trace(new Ray(Hit + ReflectionDirection * 0.001, ReflectionDirection), Bounces + 1, Throughput) * Albedo;
                    }
                    else
                    {
                        double CosTheta = MathHelper.Clamp(Vector3.Dot(Ray.Direction, Normal), -1, 1);
                        double RefractiveIndexA = 1;
                        double RefractiveIndexB = RefractiveIndex;
                        if (CosTheta < 0)
                        {
                            CosTheta = -CosTheta;
                        }
                        else
                        {
                            var Temp = RefractiveIndexA;
                            RefractiveIndexA = RefractiveIndexB;
                            RefractiveIndexB = Temp;
                            Normal = -Normal;
                        }
                        double RefractiveRatio = RefractiveIndexA / RefractiveIndexB;
                        Vector3 RefractionDirection = RefractiveRatio * Ray.Direction + (RefractiveRatio * CosTheta - Math.Sqrt(1 - RefractiveRatio * RefractiveRatio * (1 - CosTheta * CosTheta))) * Normal;
                        Throughput *= Albedo;
                        FinalColor = Trace(new Ray(Hit + RefractionDirection * 0.001, RefractionDirection), Bounces + 1, Throughput) * Albedo;
                    }
                }

                //Cook-Torrance PBR BXDF
                else
                {
                    double Metalness = Shape.Material.GetMetalness(UV);

                    Vector3 ViewDirection = Vector3.Normalize(Ray.Origin - Hit);
                    Vector3 F0 = Vector3.Lerp(new Vector3(0.04), Albedo, Metalness);

                    double DiffuseSpecularRatio = 0.5 + (0.5 * Metalness);

                    //Diffuse
                    if (Util.Random.NextDouble() > DiffuseSpecularRatio)
                    {
                        Vector3 NT = Vector3.Zero;
                        Vector3 NB = Vector3.Zero;
                        CreateCartesian(Normal, out NT, out NB);

                        double R1 = Util.Random.NextDouble();
                        double R2 = Util.Random.NextDouble();
                        Vector3 Sample = CosineSampleHemisphere(R1, R2);
                        Vector3 SampleWorld = new Vector3(
                            Sample.X * NB.X + Sample.Y * Normal.X + Sample.Z * NT.X,
                            Sample.X * NB.Y + Sample.Y * Normal.Y + Sample.Z * NT.Y,
                            Sample.X * NB.Z + Sample.Y * Normal.Z + Sample.Z * NT.Z);
                        SampleWorld.Normalize();

                        Vector3 SampleRadiance = Trace(new Ray(Hit + SampleWorld * 0.001, SampleWorld), Bounces + 1, Throughput);
                        double CosTheta = Math.Max(Vector3.Dot(Normal, SampleWorld), 0);
                        Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                        Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);
                        Vector3 Kd = Vector3.One - Ks;

                        Kd *= 1.0 - Metalness;
                        Vector3 Diffuse = Kd * Albedo;

                        //for uniform: return SampleRadiance * (2 * Diffuse * CosTheta) / (1 - DiffuseSpecularRatio);
                        Vector3 BRDFAttenuation = (Diffuse * CosTheta) / Math.Sqrt(R1) / (1 - DiffuseSpecularRatio);
                        Throughput *= BRDFAttenuation;

                        FinalColor = SampleRadiance * BRDFAttenuation;
                    }

                    //Glossy
                    else
                    {
                        double Roughness = MathHelper.Clamp(Shape.Material.GetRoughness(UV), 0.001, 1);

                        Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, Normal);
                        double R1 = Util.Random.NextDouble();
                        double R2 = Util.Random.NextDouble();
                        Vector3 SampleWorld = SampleGGX(R1, R2, ReflectionDirection, Roughness);

                        Vector3 SampleRadiance = Trace(new Ray(Hit + SampleWorld * 0.001, SampleWorld), Bounces + 1, Throughput);
                        double CosTheta = Math.Max(Vector3.Dot(Normal, SampleWorld), 0);
                        Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                        Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

                        double D = GGXDistribution(Normal, Halfway, Roughness);
                        double G = GeometrySmith(Normal, ViewDirection, SampleWorld, Roughness);
                        Vector3 SpecularNumerator = D * G * Ks;
                        double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(Normal, ViewDirection), 0.0) * CosTheta + 0.001;
                        Vector3 Specular = SpecularNumerator / SpecularDenominator;

                        Vector3 BRDFAttenuation = Specular * CosTheta / (D * Vector3.Dot(Normal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001) / DiffuseSpecularRatio;
                        Throughput *= BRDFAttenuation;

                        FinalColor = SampleRadiance * BRDFAttenuation;
                    }
                }

                //Russian roulette
                if (Bounces >= MinBounces)
                {
                    if (Util.Random.NextDouble() > Math.Max(Throughput.X, Math.Max(Throughput.Y, Throughput.Z)))
                    {
                        return Vector3.Zero;
                    }
                }

                return FinalColor;
            }

            //Return skybox color if nothing hit
            if (SkyBox == null)
            {
                return Vector3.Zero;
            }
            else
            {
                return SkyBox.GetColorAtUV(new Vector2(1 - (1.0 + Math.Atan2(Ray.Direction.Z, Ray.Direction.X) / MathHelper.Pi) * 0.5, 1 - Math.Acos(Ray.Direction.Y) / MathHelper.Pi));
            }
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
        #endregion

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

        private Vector3 UniformSampleHemisphere(double R1, double R2)
        {
            double SinTheta = Math.Sqrt(1 - R1 * R1);
            double Phi = 2 * Math.PI * R2;
            double X = SinTheta * Math.Cos(Phi);
            double Z = SinTheta * Math.Sin(Phi);
            return new Vector3(X, R1, Z);
        }

        private Vector3 CosineSampleHemisphere(double R1, double R2)
        {
            double Theta = Math.Acos(Math.Sqrt(R1));
            double Phi = 2.0 * Math.PI * R2;

            return new Vector3(Math.Sin(Theta) * Math.Cos(Phi), Math.Cos(Theta), Math.Sin(Theta) * Math.Sin(Phi));
        }

        private Vector3 SampleGGX(double R1, double R2, Vector3 ReflectionDirection, double Roughness)
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

        public double FresnelReal(double CosTheta, double RefractiveIndex)
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
