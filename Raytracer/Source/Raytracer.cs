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
        public int MaxBounces { get; private set; }
        public int Samples { get; private set; }
        public bool Branched { get; private set; }
        public int Threads { get; private set; }

        private Vector3[,] Framebuffer { get; set; }
        private List<Shape> Shapes { get; set; }

        private double InvWidth { get; set; }
        private double InvHeight { get; set; }
        private double AspectRatio { get; set; }
        private double ViewAngle { get; set; }
        
        private int[] SamplesPerBounce { get; set; }

        public Raytracer(int Width, int Height, double FOV, int MaxBounces, int Samples, bool Branched, int Threads)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.MaxBounces = MaxBounces;
            this.Samples = Samples;
            this.Branched = Branched;
            this.Threads = Threads;
            this.Framebuffer = new Vector3[Width, Height];

            this.InvWidth = 1.0 / Width;
            this.InvHeight = 1.0 / Height;
            this.AspectRatio = (double)Width / (double)Height;
            this.ViewAngle = Math.Tan(MathHelper.Pi * 0.5 * FOV / 180.0);

            //Calculate samples per bounce, exponential falloff
            if (Branched)
            {
                this.SamplesPerBounce = new int[MaxBounces];
                double DecimalMargin = 0;
                for (int i = 0; i < MaxBounces; i++)
                {
                    double CurrentSamples = Samples * Math.Pow(0.5, (i + 1 - (i == MaxBounces - 1 ? 1 : 0)));
                    DecimalMargin += CurrentSamples - (int)CurrentSamples;
                    SamplesPerBounce[i] = (int)CurrentSamples;
                }
                for (int i = 0; i < DecimalMargin; i++)
                {
                    SamplesPerBounce[i]++;
                }
            }

            //Setup scene
            Shapes = new List<Shape>()
            {
                //new TriangleMesh(new Material(Color.DarkGreen.ToVector3(), 1, 0.02, Vector3.Zero), Matrix.CreateScale(25) * Matrix.CreateTranslation(0, -1, 6), "Assets/Meshes/dragon_vrip.ply", 3, false, false),

                new Sphere(new Material("wornpaintedcement"), new Vector3(-2.5, -0.5, 5), 1.5),
                //new Sphere(new Material(Color.Green.ToVector3(), 1, 0.3, Vector3.Zero), new Vector3(0, -1, 6), 1),
                new Sphere(new Material("rustediron2"), new Vector3(2.5, -0.5, 5), 1.5),

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
                //Raycast to nearest shape
                Vector3 RayDir = new Vector3((2.0 * ((x + 0.5) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((y + 0.5) * InvHeight)) * ViewAngle, 1);
                RayDir.Normalize();

                if (Branched)
                {
                    Framebuffer[x, y] = TraceBranched(new Ray(Vector3.Zero, RayDir), Vector3.Zero, 0);
                }
                else
                {
                    for (int i = 0; i < Samples; i++)
                    {
                        //Trace primary ray
                        Framebuffer[x, y] += TraceUnbranched(new Ray(Vector3.Zero, RayDir), Vector3.Zero, 0);

                        //Supersampling
                        RayDir = new Vector3((2.0 * ((x + 0.5 + Util.Random.NextDouble()) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((y + 0.5 + Util.Random.NextDouble()) * InvHeight)) * ViewAngle, 1);
                        RayDir.Normalize();
                    }
                    //Think I'm supposed to divide by maxbounces TODO: Figure out
                    Framebuffer[x, y] /= Samples;// / MaxBounces;
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

        #region Raytracing
        private Vector3 TraceUnbranched(Ray Ray, Vector3 ViewPosition, int Bounces)
        {
            //Raycast to nearest geometry, if any
            Raycast(Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal, out Vector2 UV);
            if (FirstShape != null)
            {
                //Area lights
                Vector3 Emission = FirstShape.Material.GetEmission(UV);
                if (Emission != Vector3.Zero)
                {
                    return Emission;
                }

                //If we are about to hit max depth, no need to calculate indirect lighting
                if (Bounces >= MaxBounces)
                {
                    return Vector3.Zero;
                }

                //Indirect lighting using monte carlo path tracing
                Vector3 Albedo = FirstShape.Material.GetAlbedo(UV);
                double Metalness = FirstShape.Material.GetMetalness(UV);

                Vector3 ViewDirection = Vector3.Normalize(ViewPosition - FirstShapeHit);
                Vector3 F0 = Vector3.Lerp(new Vector3(0.04), Albedo, Metalness);

                double DiffuseSpecularRatio = 0.5 + (0.5 * Metalness);

                if (Util.Random.NextDouble() > DiffuseSpecularRatio)
                {
                    Vector3 NT = Vector3.Zero;
                    Vector3 NB = Vector3.Zero;
                    CreateCartesian(FirstShapeNormal, out NT, out NB);

                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 Sample = SampleHemisphere(R1, R2);
                    Vector3 SampleWorld = new Vector3(
                        Sample.X * NB.X + Sample.Y * FirstShapeNormal.X + Sample.Z * NT.X,
                        Sample.X * NB.Y + Sample.Y * FirstShapeNormal.Y + Sample.Z * NT.Y,
                        Sample.X * NB.Z + Sample.Y * FirstShapeNormal.Z + Sample.Z * NT.Z);
                    SampleWorld.Normalize();

                    Vector3 SampleRadiance = TraceUnbranched(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);
                    Vector3 Kd = Vector3.One - Ks;

                    Kd *= 1.0 - Metalness;
                    Vector3 Diffuse = Kd * Albedo;

                    return (2 * Diffuse * SampleRadiance * CosTheta) / (1 - DiffuseSpecularRatio);
                }
                else 
                {
                    double Roughness = MathHelper.Clamp(FirstShape.Material.GetRoughness(UV), 0.0001, 1);

                    Vector3 ReflectionDirection = Vector3.Reflect(-ViewDirection, FirstShapeNormal);
                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 SampleWorld = ImportanceSampleGGX(R1, R2, ReflectionDirection, Roughness);

                    Vector3 SampleRadiance = TraceUnbranched(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

                    double D = GGXDistribution(FirstShapeNormal, Halfway, Roughness);
                    double G = GeometrySmith(FirstShapeNormal, ViewDirection, SampleWorld, Roughness);
                    Vector3 SpecularNumerator = D * G * Ks;
                    double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(FirstShapeNormal, ViewDirection), 0.0) * CosTheta + 0.001;
                    Vector3 Specular = SpecularNumerator / SpecularDenominator;

                    return Specular * SampleRadiance * CosTheta / (D * Vector3.Dot(FirstShapeNormal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001) / DiffuseSpecularRatio;
                }
            }
            return Vector3.Zero;
        }

        private Vector3 TraceBranched(Ray Ray, Vector3 ViewPosition, int Bounces)
        {
            //Raycast to nearest geometry, if any
            Raycast(Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal, out Vector2 UV);
            if (FirstShape != null)
            {
                //Area lights
                Vector3 Emission = FirstShape.Material.GetEmission(UV);
                if (Emission != Vector3.Zero)
                {
                    return Emission;
                }

                //If we are about to hit max depth, no need to calculate indirect lighting
                if (Bounces >= MaxBounces)
                {
                    return Vector3.Zero;
                }

                //Indirect lighting using monte carlo path tracing
                Vector3 Indirect = Vector3.Zero;

                Vector3 Albedo = FirstShape.Material.GetAlbedo(UV);
                double Roughness = MathHelper.Clamp(FirstShape.Material.GetRoughness(UV), 0.001, 1);
                double Metalness = FirstShape.Material.GetMetalness(UV);

                Vector3 TotalDiffuse = Vector3.Zero;
                Vector3 TotalSpecular = Vector3.Zero;

                Vector3 ViewDirection = Vector3.Normalize(ViewPosition - FirstShapeHit);
                Vector3 F0 = Vector3.Lerp(new Vector3(0.04), Albedo, Metalness);

                double TotalSamples = SamplesPerBounce[Bounces];
                double HalfSamples = TotalSamples * 0.5;
                double MetalSamplesOffset = (TotalSamples * 0.5 * Metalness);
                int DiffuseSamples = (int)(HalfSamples - MetalSamplesOffset);
                int SpecularSamples = (int)(HalfSamples + MetalSamplesOffset);

                Vector3 NT = Vector3.Zero;
                Vector3 NB = Vector3.Zero;
                if(DiffuseSamples > 0)
                {
                    CreateCartesian(FirstShapeNormal, out NT, out NB);
                }

                Vector3 ReflectionDirection = Vector3.Zero;
                if (SpecularSamples > 0)
                {
                    ReflectionDirection = Vector3.Reflect(-ViewDirection, FirstShapeNormal);
                }

                //Diffuse
                for (int i = 0; i < DiffuseSamples; i++)
                {
                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 Sample = SampleHemisphere(R1, R2);
                    Vector3 SampleWorld = new Vector3(
                        Sample.X * NB.X + Sample.Y * FirstShapeNormal.X + Sample.Z * NT.X,
                        Sample.X * NB.Y + Sample.Y * FirstShapeNormal.Y + Sample.Z * NT.Y,
                        Sample.X * NB.Z + Sample.Y * FirstShapeNormal.Z + Sample.Z * NT.Z);
                    SampleWorld.Normalize();

                    Vector3 SampleRadiance = TraceBranched(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);
                    Vector3 Kd = Vector3.One - Ks;

                    Kd *= 1.0 - Metalness;
                    Vector3 Diffuse = Kd * Albedo;
                    
                    TotalDiffuse += Diffuse * SampleRadiance * CosTheta;
                }

                //Specular
                for (int i = 0; i < SpecularSamples; i++)
                {
                    double R1 = Util.Random.NextDouble();
                    double R2 = Util.Random.NextDouble();
                    Vector3 SampleWorld = ImportanceSampleGGX(R1, R2, ReflectionDirection, Roughness);

                    Vector3 SampleRadiance = TraceBranched(new Ray(FirstShapeHit + SampleWorld * 0.001, SampleWorld), FirstShapeHit, Bounces + 1);
                    double CosTheta = Math.Max(Vector3.Dot(FirstShapeNormal, SampleWorld), 0);
                    Vector3 Halfway = Vector3.Normalize(SampleWorld + ViewDirection);

                    Vector3 Ks = FresnelSchlick(Math.Max(Vector3.Dot(Halfway, ViewDirection), 0.0), F0);

                    double D = GGXDistribution(FirstShapeNormal, Halfway, Roughness);
                    double G = GeometrySmith(FirstShapeNormal, ViewDirection, SampleWorld, Roughness);
                    Vector3 SpecularNumerator = D * G * Ks;
                    double SpecularDenominator = 4.0 * Math.Max(Vector3.Dot(FirstShapeNormal, ViewDirection), 0.0) * CosTheta + 0.001;
                    Vector3 Specular = SpecularNumerator / SpecularDenominator;

                    TotalSpecular += Specular * SampleRadiance * CosTheta / (D * Vector3.Dot(FirstShapeNormal, Halfway) / (4 * Vector3.Dot(Halfway, ViewDirection)) + 0.0001);
                }

                //Divide results
                if (DiffuseSamples > 0)
                {
                    //PDF division and color division by pi is implicit here
                    //We move division with PDF out because it is constant
                    //And we move MaterialColor / Pi division out because pi is constant also
                    //DiffuseSamples * (1.0 / (2.0 * Pi)) * Pi    is equivalent to    DiffuseSamples / 2
                    TotalDiffuse /= (double)DiffuseSamples * 0.5;
                }
                if (SpecularSamples > 0)
                {
                    TotalSpecular /= (double)SpecularSamples;
                }
                return TotalDiffuse + TotalSpecular;
            }
            return Vector3.Zero;
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
