using System;
using System.Collections.Generic;
using System.Drawing;

namespace Raytracer
{
    public class Raytracer
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double FOV { get; set; }
        public Vector3[,] Framebuffer { get; set; }
        public List<Light> Lights { get; set; }
        public List<Shape> Shapes { get; set; }
        public double InvWidth { get; set; }
        public double InvHeight { get; set; }
        public double AspectRatio { get; set; }
        public double ViewAngle { get; set; }
        public int MaxDepth = 5;

        public Raytracer(int Width, int Height, double FOV)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.Framebuffer = new Vector3[Width, Height];

            this.InvWidth = 1.0 / Width;
            this.InvHeight = 1.0 / Height;
            this.AspectRatio = (double)Width / (double)Height;
            this.ViewAngle = Math.Tan(MathHelper.Pi * 0.5 * FOV / 180.0);

            Lights = new List<Light>()
            {
                new Light(new Vector3(0, 4, 2), 20, Color.White.ToVector3()),
            };
            Shapes = new List<Shape>()
            {
                new Sphere(new Material(Color.LightCyan.ToVector3(), 128, 0, 1, 1.1), new Vector3(-2.5, 5, -1), 1),
                new Sphere(new Material(Color.Green.ToVector3(), 128, 0, 0, 0), new Vector3(0, 6, -1), 1),
                new Sphere(new Material(Color.LightCyan.ToVector3(), 128, 1, 0, 0), new Vector3(2.5, 5, -1), 1),

                new Plane(new Material(Color.LightGray.ToVector3(), 4, 0, 0, 0), new Vector3(0, 5, -2), new Vector3(0, 0, 1)),
                new Plane(new Material(Color.LightBlue.ToVector3(), 4, 0, 0, 0), new Vector3(0, 5, 5), new Vector3(0, 0, -1)),
                new Plane(new Material(Color.Green.ToVector3(), 4, 0, 0, 0), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 4, 0, 0, 0), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new Material(Color.Pink.ToVector3(), 4, 0, 0, 0), new Vector3(0, 10, 0), new Vector3(0, -1, 0)),
                new Plane(new Material(Color.LightSalmon.ToVector3(), 4, 0, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 0))
            };
        }

        public void RenderToFile(string Path)
        {
            Bitmap Render = new Bitmap(Width, Height);

            //For each pixel
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //Set backbuffer
                    Render.SetPixel(x, y, Color.Black);

                    //Raycast to nearest shape
                    Vector3 RayDir = new Vector3((2.0 * ((x + 0.5) * InvWidth) - 1.0) * ViewAngle * AspectRatio, 1, (1.0 - 2.0 * ((y + 0.5) * InvHeight)) * ViewAngle);
                    RayDir.Normalize();

                    Render.SetPixel(x, y, Trace(new Ray(Vector3.Zero, RayDir), 0).ToColor());
                }
            }

            Render.Save(Path);
        }

        public Vector3 Trace(Ray Ray, int Depth)
        {
            Vector3 Result = Vector3.Zero;

            Raycast(Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal);

            if (FirstShape != null)
            {
                if (FirstShape.Material.Reflection > 0 || FirstShape.Material.Transparency > 0 && Depth < MaxDepth)
                {
                    bool Inside = false;
                    if (Vector3.Dot(Ray.Direction, FirstShapeNormal) > 0)
                    {
                        FirstShapeNormal = -FirstShapeNormal;
                        Inside = true;
                    }

                    Vector3 ReflectionColor = Vector3.Zero;
                    if (FirstShape.Material.Reflection > 0 && !Inside)
                    {
                        Vector3 ReflectionDirection = Ray.Direction - 2 * (Vector3.Dot(Ray.Direction, FirstShapeNormal)) * FirstShapeNormal;
                        ReflectionDirection.Normalize();
                        Ray ReflectionRay = new Ray(FirstShapeHit + FirstShapeNormal * 0.01f, ReflectionDirection);
                        ReflectionColor = Trace(ReflectionRay, Depth + 1);
                    }

                    Vector3 RefractionColor = Vector3.Zero;
                    if (FirstShape.Material.Transparency > 0)
                    {
                        double IndexOfRefraction = (Inside) ? FirstShape.Material.RefractiveIndex : 1 / FirstShape.Material.RefractiveIndex; // are we inside or outside the surface? 
                        double CosI = -Vector3.Dot(FirstShapeNormal, Ray.Direction);
                        double K = 1 - IndexOfRefraction * IndexOfRefraction * (1 - CosI * CosI);
                        Vector3 refrdir = Ray.Direction * IndexOfRefraction + FirstShapeNormal * (IndexOfRefraction * CosI - Math.Sqrt(K));
                        refrdir.Normalize();
                        RefractionColor = Trace(new Ray(FirstShapeHit - FirstShapeNormal * 0.01f, refrdir), Depth + 1);
                    }

                    Result =  (ReflectionColor * FirstShape.Material.Reflection + RefractionColor * FirstShape.Material.Transparency) * FirstShape.Material.Color;
                }
                else
                {
                    foreach (Light Light in Lights)
                    {
                        //Raycast from light to nearest shape, shadow check
                        Vector3 ShadowRayDirection = FirstShapeHit - Light.Origin;
                        ShadowRayDirection.Normalize();
                        Ray ShadowRay = new Ray(Light.Origin, ShadowRayDirection);
                        Raycast(ShadowRay, out Shape FirstShadow, out Vector3 FirstShadowHit, out Vector3 FirstShadowNormal, out Vector3 FirstShadowColor);

                        if (FirstShadow == null || FirstShadow == FirstShape)
                        {
                            //Do phong shading, additive
                            Result += Phong(FirstShape, FirstShapeHit, FirstShapeNormal, Light) * FirstShadowColor;
                            Result = new Vector3(MathHelper.Clamp(Result.X, 0, 1), MathHelper.Clamp(Result.Y, 0, 1), MathHelper.Clamp(Result.Z, 0, 1));
                            //Result += Phong(FirstShape, FirstShapeHit, FirstShapeNormal, Light);
                        }
                    }
                }
            }

            return Result;
        }

        //For everything
        public bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal)
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

        //For shadows
        public bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal, out Vector3 LightColor)
        {
            double MinDistance = double.MaxValue;
            FirstShape = null;
            FirstShapeHit = Vector3.Zero;
            FirstShapeNormal = Vector3.Zero;
            LightColor = Vector3.One;
            foreach (Shape Shape in Shapes)
            {
                if (Shape.Intersect(Ray, out Vector3 Hit, out Vector3 Normal))
                {
                    if (Shape.Material.Transparency > 0)
                    {
                        LightColor *= Shape.Material.Color * Shape.Material.Transparency;
                        continue;
                    }

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

        public Vector3 Phong(Shape Shape, Vector3 Hit, Vector3 Normal, Light Light)
        {
            //TODO: No hardcoded texture
            //Texture mapping
            //double TexX = (1 + (double)Math.Atan2(Normal.Y, Normal.X) / MathHelper.Pi) * 0.5;
            //double TexY = (double)Math.Acos(Normal.Z) / MathHelper.Pi;
            //var TexData = Texture.GetPixel((int)(TexX * Texture.Width), (int)(TexY * Texture.Height));
            //var TexColor = new Vector3(TexData.R / 255f, TexData.G / 255f, TexData.B / 255f);
            var TexColor = Shape.Material.Color;

            Vector3 LightDirection = Light.Origin - Hit;
            double Distance = LightDirection.LengthSquared();
            LightDirection.Normalize();

            //Diffuse
            double Lambertian = Math.Max(Vector3.Dot(Normal, LightDirection), 0);

            //Specular
            double Specular = 0.0;
            if (Lambertian > 0.0)
            {
                Vector3 ViewDirection = Vector3.Zero - Hit;
                ViewDirection.Normalize();
                Vector3 Halfway = (LightDirection + ViewDirection);
                Halfway.Normalize();

                double SpecularAngle = Math.Max(Vector3.Dot(Normal, Halfway), 0.0);
                Specular = Math.Pow(SpecularAngle, Shape.Material.Shininess);
            }

            //Final luminance
            Vector3 Final = TexColor * Lambertian * Light.Color * Light.Intensity / Distance +
                            Specular * Light.Color * Light.Intensity / Distance;

            //Gamma correction
            //Final = new Vector3((double)Math.Pow(Final.X, 1.0 / 2.2), (double)Math.Pow(Final.Y, 1.0 / 2.2), (double)Math.Pow(Final.Z, 1.0 / 2.2));

            return Final;
        }
    }
}
