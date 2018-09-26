using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private List<Shape> Lights { get; set; }

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
                //Cerberus gun scene
                //new TriangleMesh(new Material("Cerberus"), Matrix.CreateScale(5) * Matrix.CreateRotationY(Math.PI/2) * Matrix.CreateTranslation(-2, 0.5, 5), "Assets/Meshes/Gun.ply", 3, true, false),

                //Velvet Cloth scene
                //new Sphere(new PBRMaterial("rustediron2"), new Vector3(0, 0, 4), 1),
                //new TriangleMesh(new VelvetMaterial(0.65, Color.Red.ToVector3()), Matrix.CreateRotationY(Math.PI-0.4) * Matrix.CreateTranslation(0, 0, 4), "Assets/meshes/ballcover.ply", 3, true, true),
                //new Plane(new PBRMaterial(Vector3.One, 1, 0.2), new Vector3(0, -0.98, 0), new Vector3(0, 1, 0)),

                //Coffee Scene
                //new TriangleMesh(new GlassMaterial(new Vector3(0.8, 1, 0.8), 1.1, 0), Matrix.CreateScale(0.8) * Matrix.CreateTranslation(0, -2, 6), "Assets/Meshes/Coffee/Cup.ply", 3, true, false),
                //new TriangleMesh(new PBRMaterial(Vector3.One, 0, 0.1), Matrix.CreateScale(0.8) * Matrix.CreateTranslation(0, -2.01, 6), "Assets/Meshes/Coffee/Plate.ply", 3, true, true),
                //new Plane(new PBRMaterial(Color.Brown.ToVector3(), 0, 1), new Vector3(0, -2, 0), new Vector3(0, 1, 0)),

                //BlenderBall Scene
                //new TriangleMesh(new VelvetMaterial(Color.Gold.ToVector3(), 0.8), Matrix.CreateScale(0.25) * Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderBall/BlenderBallShell.ply"),
                //new TriangleMesh(new PBRMaterial(Color.Black.ToVector3(), 0, 0.8), Matrix.CreateScale(0.25) * Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderBall/BlenderBallCore.ply"),
                //new TriangleMesh(new PBRMaterial(Color.Black.ToVector3(), 0, 0.8), Matrix.CreateScale(0.25) * Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderBall/BlenderBallBase.ply"),
                //new Plane(new LambertianMaterial(Color.Gray.ToVector3()), new Vector3(0, -1.5, 0), new Vector3(0, 1, 0))

                //BlenderCup scene
                //new TriangleMesh(new PBRMaterial(Color.White.ToVector3(), 0, 0.1), Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderCup/Plate.ply"),
                //new TriangleMesh(new GlassMaterial(new Vector3(0.8, 0.8, 1), 1.3, 0.3), Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderCup/CupInner.ply"),
                //new TriangleMesh(new PBRMaterial(Color.DarkGoldenrod.ToVector3(), 1, 0.3), Matrix.CreateRotationY(-Math.PI/4) * Matrix.CreateTranslation(0, -1.5, 5), "Assets/Meshes/BlenderCup/CupOuter.ply"),
                //new Plane(new LambertianMaterial(Color.Brown.ToVector3()), new Vector3(0, -1.5, 0), new Vector3(0, 1, 0))

                //GlassTall scene
                //new TriangleMesh(new GlassMaterial(Color.White.ToVector3()*0.8, 1.45, 0), Matrix.CreateScale(1.5) * Matrix.CreateTranslation(0, -2, 5), "Assets/Meshes/GlassTall/GlassTall.ply"),
                //new TriangleMesh(new GlassMaterial(Color.White.ToVector3(), 1.33, 0, new IsotropicMedium(Color.CornflowerBlue.ToVector3(), 2, 2)), Matrix.CreateScale(1.5) * Matrix.CreateTranslation(0, -2, 5), "Assets/Meshes/GlassTall/GlassTallLiquid.ply"),

                //2 ball material scene
                new Sphere(new LambertianMaterial(Color.Blue.ToVector3()), new Vector3(-2.5, -0.5, 5), 1.5),
                //new Sphere(new PBRMaterial("rustediron2"), new Vector3(2.5, -0.5, 5), 1.5),

                //Box Scene
                new Plane(new LambertianMaterial(Color.LightGray.ToVector3()), new Vector3(0, -2, 5), new Vector3(0, 1, 0)),
                new Plane(new LambertianMaterial(Color.Pink.ToVector3()), new Vector3(0, 5, 5), new Vector3(0, -1, 0)),
                new Plane(new LambertianMaterial(Color.Green.ToVector3()), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new LambertianMaterial(Color.Green.ToVector3()), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new LambertianMaterial(Color.Pink.ToVector3()), new Vector3(0, 0, 10), new Vector3(0, 0, -1)),
                new Plane(new LambertianMaterial(Color.Black.ToVector3()), new Vector3(0, 0, -1), new Vector3(0, 0, 1)),

                new Sphere(new EmissionMaterial(Vector3.One), new Vector3(2, 4, 7), 0.5),
                new Sphere(new EmissionMaterial(Vector3.One), new Vector3(-2, 4, 7), 0.5)
            };
            Lights = new List<Shape>();
            foreach (Shape S in Shapes)
            {
                if (S.Material.HasProperty("emission"))
                {
                    Lights.Add(S);
                }
            }
        }

        public void Render()
        {
            Framebuffer = new Vector3[Width, Height];

            int Progress = 0;
            using (new Timer(_ => Console.WriteLine("Rendered {0} of {1} scanlines", Progress, Height), null, 1000, 1000))
            {
                Parallel.For(0, Height, new ParallelOptions { MaxDegreeOfParallelism = Threads }, (i) => { RenderLine(i); Progress++; });
            }
        }

        private void RenderLine(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int i = 0; i < Samples; i++)
                {
                    //Supersampling
                    Vector3 RayDir = new Vector3((2.0 * ((x + Util.Random.NextDouble()) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((y + Util.Random.NextDouble()) * InvHeight)) * ViewAngle, 1);
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

            LobeType SampledLobe = LobeType.SpecularReflection;

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
                        //Don't add emission to diffuse term, we already sample it directly
                        if (SampledLobe == LobeType.SpecularReflection)
                        {
                            FinalColor += Throughput * Shape.Material.GetProperty("emission", UV).Color;
                        }
                        break;
                    }

                    Vector3 ViewDirection = Vector3.Normalize(Ray.Origin - Hit);

                    Shape.Material.Sample(ViewDirection, Normal, UV, out Vector3 SampleDirection, out SampledLobe);
                    Shape.Material.PDF(ViewDirection, Normal, UV, SampleDirection, SampledLobe, out double PDF);
                    Shape.Material.Evaluate(ViewDirection, Normal, UV, SampleDirection, SampledLobe, out Vector3 Attenuation);

                    if (SampledLobe == LobeType.DiffuseReflection)
                    {
                        Sphere Light = Lights[Util.Random.Next(Lights.Count)] as Sphere;
                        Vector3 Emission = Light.Material.GetProperty("emission", UV).Color;

                        Vector3 TotalDirectLighting = Vector3.Zero;
                        Vector3 BSDFAttenuation = Vector3.Zero;
                        double LightPDF, ScatterPDF;

                        //Sample light directly
                        Vector3 LightSample = Light.Sample() - Hit;
                        double Distance = LightSample.Length();
                        LightSample.Normalize();

                        //Visibility check
                        Raycast(new Ray(Hit + LightSample * 0.001, LightSample), out Shape LightShape, out Vector3 LightHit, out Vector3 LightNormal, out Vector2 LightUV);
                        if (LightShape == Light)
                        {
                            //Calculate light pdf for light sample
                            LightPDF = Math.Pow(Distance, 2) / (Vector3.Dot(LightNormal, -LightSample) * Light.Area());

                            //Calculate bsdf pdf for light sample
                            Shape.Material.Evaluate(ViewDirection, Normal, UV, LightSample, SampledLobe, out BSDFAttenuation);
                            Shape.Material.PDF(ViewDirection, Normal, UV, LightSample, SampledLobe, out ScatterPDF);

                            //Weighted sum of the 2 sampling strategies
                            if (BSDFAttenuation != Vector3.Zero)
                            {
                                TotalDirectLighting += BSDFAttenuation * Emission * Util.BalanceHeuristic(LightPDF, ScatterPDF) / LightPDF;
                            }
                        }

                        //Sample light using BSDF  -  TODO: Sample only diffuse
                        Shape.Material.Sample(ViewDirection, Normal, UV, out Vector3 BSDFSample, out LobeType BSDFLobe);
                        Shape.Material.Evaluate(ViewDirection, Normal, UV, BSDFSample, BSDFLobe, out BSDFAttenuation);
                        if (BSDFAttenuation != Vector3.Zero)
                        {
                            //Visibility check
                            Raycast(new Ray(Hit + BSDFSample * 0.001, BSDFSample), out Shape BSDFShape, out Vector3 BSDFHit, out Vector3 BSDFNormal, out Vector2 BSDFUV);
                            if (BSDFShape == Light)
                            {
                                //Calculate light pdf for bsdf sample
                                LightPDF = Math.Pow((BSDFHit - Hit).Length(), 2) / (Vector3.Dot(BSDFNormal, Vector3.Normalize(Hit - BSDFHit)) * Light.Area());

                                //Calculate bsdf pdf for bsdf sample
                                Shape.Material.PDF(ViewDirection, Normal, UV, BSDFSample, BSDFLobe, out ScatterPDF);

                                //Weighted sum of the 2 sampling strategies
                                TotalDirectLighting += BSDFAttenuation * Emission * Util.BalanceHeuristic(ScatterPDF, LightPDF) / ScatterPDF;
                            }
                        }

                        FinalColor += Throughput * (TotalDirectLighting / (1.0 / Lights.Count));
                    }

                    //Accumulate BXDF attenuation
                    Throughput *= Attenuation / PDF;

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
