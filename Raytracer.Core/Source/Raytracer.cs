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
        public bool NEE { get; set; }

        public Vector3[,] Framebuffer { get; private set; }
        public Scene World { get; private set; }

        private double InvWidth { get; set; }
        private double InvHeight { get; set; }
        private double AspectRatio { get; set; }
        private double ViewAngle { get; set; }
        private Matrix CameraRotationMatrix { get; set; }

        public Raytracer(int Width, int Height, double FOV, Vector3 CameraPosition, Vector3 CameraRotation, Texture SkyBox, int MinBounces, int MaxBounces, int Samples, int Threads, bool NEE, bool LoadSampleScene = true)
        {
            //Load example scene
            if (LoadSampleScene)
            {
                Vector3 Pos = new Vector3(0.1, -2, 4);
                double Scale = 1.75;
                double Rot = Math.PI;
                LoadScene(new Scene(new List<Shape>()
                {
                    new Plane(new PBRMaterial(new Vector3(0.75), 0, 0.4), new Vector3(0, -1, 0), new Vector3(0, 1, 0)),
                    new Plane(new EmissionMaterial(Vector3.One), new Vector3(0, 2, 0), new Vector3(0, -1, 0)),
                    new Plane(new PBRMaterial(new Vector3(0), 0, 0.9), new Vector3(2, 0, 0), new Vector3(-1, 0, 0)),
                    new Plane(new PBRMaterial(new Vector3(0), 0, 0.9), new Vector3(-2, 0, 0), new Vector3(1, 0, 0)),
                    new Plane(new PBRMaterial(new Vector3(0), 0, 0.9), new Vector3(0, 0, 5), new Vector3(0, 0, -1)),
                    new Plane(new PBRMaterial(new Vector3(0), 0, 0.9), new Vector3(0, 0, -2), new Vector3(0, 0, 1)),

                    new TriangleMesh(new PBRMaterial(new Vector3(1, 1, 0.1), 1, 0.15), Matrix.CreateScale(1) * Matrix.CreateTranslation(0, -0.3, 2.9), "Assets/Meshes/dragon2.ply", 3, false, true),

                    //new Quad(new EmissionMaterial(Vector3.One), new Vector3(-1.3, 3.9999, 3.25), new Vector3(0, -1, 0), new Vector2(1, 4)),
                    //new Quad(new EmissionMaterial(Vector3.One), new Vector3(1.3, 3.9999, 3.25), new Vector3(0, -1, 0), new Vector2(1, 4)),
                    //new Quad(new EmissionMaterial(Vector3.One), new Vector3(0, 0, -0.9999), new Vector3(0, 0, 1), new Vector2(2, 2)),


                    //new TriangleMesh(new PBRMaterial("MeiGun"), Matrix.CreateScale(Scale) * Matrix.CreateRotationY(Rot) * Matrix.CreateTranslation(Pos), "Assets/Meshes/MeiGun.ply", 3, true, false),
                    //new Plane(new PBRMaterial(Color.CornflowerBlue.ToVector3(), 0, 1), new Vector3(0, -1.5, 0), new Vector3(0, 1, 0)),
                }), false);
            }

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
            this.NEE = NEE;
        }

        public void Render()
        {
            Vector3 RayDir = new Vector3((2.0 * ((149 + Util.Random.NextDouble()) * InvWidth) - 1.0) * ViewAngle * AspectRatio, (1.0 - 2.0 * ((355 + Util.Random.NextDouble()) * InvHeight)) * ViewAngle, 1);
            RayDir.Normalize();
            RayDir = Vector3.Transform(RayDir, CameraRotationMatrix);
            Trace(new Ray(CameraPosition, RayDir));

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
                    Vector3 Radiance;
                    do
                    {
                        Radiance = Trace(new Ray(CameraPosition, RayDir));
                    }
                    while (double.IsNaN(Radiance.X) || double.IsNaN(Radiance.Y) || double.IsNaN(Radiance.Z));
                    Framebuffer[x, y] += Radiance;
                }
                Framebuffer[x, y] /= Samples;
            }
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
                World.Raycast(Ray, out Shape Shape, out Vector3 Hit, out Vector3 Normal, out Vector2 UV);

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

                //Volumetrics, slightly borken
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

                //If no scattering event happened, do normal path tracing
                if (!Scattered)
                {
                    //Area lights
                    if (Shape.Material.HasProperty("emission"))
                    {
                        //Don't add emission to diffuse term if NEE is on, we already sample it directly
                        if (SampledLobe == LobeType.SpecularReflection || !NEE)
                        {
                            FinalColor += Throughput * Shape.Material.GetProperty("emission", UV).Color;
                        }
                        break;
                    }

                    Vector3 ViewDirection = Vector3.Normalize(Ray.Origin - Hit);

                    //Sample BSDF
                    Shape.Material.Sample(ViewDirection, Normal, UV, out Vector3 SampleDirection, out SampledLobe);
                    Shape.Material.PDF(ViewDirection, Normal, UV, SampleDirection, SampledLobe, out double PDF);
                    Shape.Material.Evaluate(ViewDirection, Normal, UV, SampleDirection, SampledLobe, out Vector3 Attenuation);

                    //Sample direct lighting
                    if (NEE)
                    {
                        FinalColor += Throughput * SampleLight(Shape, Hit, ViewDirection, Normal, UV, SampleDirection, SampledLobe, Attenuation);
                    }

                    //Accumulate BSDF attenuation
                    Throughput *= Attenuation / PDF;

                    //If we entered a medium, update current medium
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
                    Throughput *= 1.0 / Prob;
                }
            }

            return FinalColor;
        }

        private Vector3 SampleLight(Shape Shape, Vector3 Hit, Vector3 ViewDirection, Vector3 Normal, Vector2 UV, Vector3 SampleDirection, LobeType SampledLobe, Vector3 BSDFAttenuation)
        {
            //If this is not diffuse or there are no lights, no direct lighting calculation needs to be done
            if (SampledLobe != LobeType.DiffuseReflection || World.Lights.Count == 0)
            {
                return Vector3.Zero;
            }

            //Pick a light
            World.PickLight(out Shape Light, out double LightPickPDF);
            Vector3 Emission = Light.Material.GetProperty("emission", UV).Color;

            Vector3 TotalDirectLighting = Vector3.Zero;

            //Sample BSDF
            if (BSDFAttenuation != Vector3.Zero)
            {
                //Visibility check
                World.Raycast(new Ray(Hit + SampleDirection * 0.001, SampleDirection), out Shape BSDFShape, out Vector3 BSDFHit, out Vector3 BSDFNormal, out Vector2 BSDFUV);
                if (BSDFShape == Light)
                {
                    //Calculate light pdf for bsdf sample
                    double LightPDF = Math.Pow((Hit - BSDFHit).Length(), 2) / (Vector3.Dot(BSDFNormal, Vector3.Normalize(Hit - BSDFHit)) * Light.Area());

                    //Calculate bsdf pdf for bsdf sample
                    Shape.Material.PDF(ViewDirection, Normal, UV, SampleDirection, SampledLobe, out double ScatterPDF);

                    //Weighted sum of the 2 sampling strategies for this sample
                    TotalDirectLighting += BSDFAttenuation * Emission * Util.BalanceHeuristic(ScatterPDF, LightPDF) / ScatterPDF;
                }
            }

            //Sample light directly
            Vector3 LightSample = Light.Sample() - Hit;
            double Distance = LightSample.Length();
            LightSample.Normalize();

            //Visibility check
            World.Raycast(new Ray(Hit + LightSample * 0.001, LightSample), out Shape LightShape, out Vector3 LightHit, out Vector3 LightNormal, out Vector2 LightUV);
            if ((LightHit - Hit).Length() >= Distance - 0.001)
            {
                //Calculate light pdf for light sample, 
                double LightPDF = Math.Pow(Distance, 2) / (Vector3.Dot(LightNormal, -LightSample) * Light.Area());

                //Calculate bsdf pdf for light sample
                Shape.Material.Evaluate(ViewDirection, Normal, UV, LightSample, SampledLobe, out BSDFAttenuation);
                Shape.Material.PDF(ViewDirection, Normal, UV, LightSample, SampledLobe, out double ScatterPDF);

                //Weighted sum of the 2 sampling strategies for this sample
                if (BSDFAttenuation != Vector3.Zero)
                {
                    TotalDirectLighting += BSDFAttenuation * Emission * Util.BalanceHeuristic(LightPDF, ScatterPDF) / LightPDF;
                }
            }

            return TotalDirectLighting / LightPickPDF;
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

        public void LoadScene(string Path, bool LoadRenderSettings = true)
        {
            World = new Scene(Path);
            if (LoadRenderSettings)
            {
                this.Width = World.PreferredWidth;
                this.Height = World.PreferredHeight;
                this.FOV = World.PreferredFOV;
                this.CameraPosition = World.PreferredCameraPosition;
                this.CameraRotation = World.PreferredCameraRotation;
                this.SkyBox = World.PreferredSkyBox;
            }
        }

        public void LoadScene(Scene World, bool LoadRenderSettings = true)
        {
            this.World = World;
            if (LoadRenderSettings)
            {
                this.Width = World.PreferredWidth;
                this.Height = World.PreferredHeight;
                this.FOV = World.PreferredFOV;
                this.CameraPosition = World.PreferredCameraPosition;
                this.CameraRotation = World.PreferredCameraRotation;
                this.SkyBox = World.PreferredSkyBox;
            }
        }
    }
}
