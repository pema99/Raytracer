using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Raytracer
{
    public class Raytracer
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float FOV { get; set; }
        public Color[] Framebuffer { get; set; }
        public Texture2D Rendertarget { get; set; }
        public List<Light> Lights { get; set; }
        public List<Shape> Shapes { get; set; }
        public float InvWidth { get; set; }
        public float InvHeight { get; set; }
        public float AspectRatio { get; set; }
        public float ViewAngle { get; set; }
        public int MaxDepth = 5;

        System.Drawing.Bitmap Texture = new System.Drawing.Bitmap("Content/wood.jpg");

        public Raytracer(int Width, int Height, float FOV, Texture2D Rendertarget)
        {
            this.Width = Width;
            this.Height = Height;
            this.FOV = FOV;
            this.Rendertarget = Rendertarget;
            this.Framebuffer = new Color[Width * Height];

            this.InvWidth = 1f / (float)Width;
            this.InvHeight = 1f / (float)Height;
            this.AspectRatio = (float)Width / (float)Height;
            this.ViewAngle = (float)Math.Tan(MathHelper.Pi * 0.5f * FOV / 180f);

            Lights = new List<Light>()
            {
                new Light(new Vector3(0, 4, 2), 15f, Color.White),
                //new Light(new Vector3(0, 0, 0), 20f, Color.White),
                //new Light(new Vector3(520, -150, 100)),
            };
            Shapes = new List<Shape>()
            {
                new Sphere(new Material(Color.Red.ToVector3(), 128, 0, 1, 1.1f), new Vector3(-2.5f, 5, -1), 1),
                new Sphere(new Material(Color.Green.ToVector3(), 128, 0, 0, 0), new Vector3(0, 6, -1), 1),
                new Sphere(new Material(Color.Blue.ToVector3(), 128, 1, 0, 0), new Vector3(2.5f, 5, -1), 1),

                new Plane(new Material(Color.LightGray.ToVector3(), 128, 0, 0, 0), new Vector3(0, 5, -2), new Vector3(0, 0, 1)),
                new Plane(new Material(Color.LightBlue.ToVector3(), 128, 0, 0, 0), new Vector3(0, 5, 5), new Vector3(0, 0, -1)),
                new Plane(new Material(Color.Pink.ToVector3(), 128, 0, 0, 0), new Vector3(0, 10, 0), new Vector3(0, -1, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 128, 0, 0, 0), new Vector3(7, 0, 0), new Vector3(-1, 0, 0)),
                new Plane(new Material(Color.Green.ToVector3(), 128, 0, 0, 0), new Vector3(-7, 0, 0), new Vector3(1, 0, 0)),
                new Plane(new Material(Color.LightSalmon.ToVector3(), 128, 0, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 0))
            };
        }

        public void Update(GameTime gameTime)
        {
            #region Input
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Lights[0].Origin += new Vector3(0, 0.1f, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Lights[0].Origin -= new Vector3(0, 0.1f, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Lights[0].Origin -= new Vector3(0.1f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                Lights[0].Origin += new Vector3(0.1f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Lights[0].Origin -= new Vector3(0, 0, 0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Lights[0].Origin += new Vector3(0, 0, 0.1f);
            }
            #endregion
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //For each pixel
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    //Set backbuffer
                    SetPixel(x, y, Color.Black);

                    //Raycast to nearest shape
                    Vector3 RayDir = new Vector3((2f * ((x + 0.5f) * InvWidth) - 1) * ViewAngle * AspectRatio, 1, (1f - 2f * ((y + 0.5f) * InvHeight)) * ViewAngle);
                    RayDir.Normalize();

                    SetPixel(x, y, new Color(Trace(new Ray(Vector3.Zero, RayDir), 0)));
                }
            }
            Rendertarget.SetData<Color>(Framebuffer);
            spriteBatch.Draw(Rendertarget, new Rectangle(0, 0, Width, Height), Color.White);
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
                        Vector3 ReflectionDirection = Ray.Direction - 2f * (Vector3.Dot(Ray.Direction, FirstShapeNormal)) * FirstShapeNormal;
                        ReflectionDirection.Normalize();
                        Ray ReflectionRay = new Ray(FirstShapeHit + FirstShapeNormal * 0.01f, ReflectionDirection);
                        ReflectionColor = Trace(ReflectionRay, Depth + 1);
                    }

                    Vector3 RefractionColor = Vector3.Zero;
                    if (FirstShape.Material.Transparency > 0)
                    {
                        float IndexOfRefraction = (Inside) ? FirstShape.Material.RefractiveIndex : 1 / FirstShape.Material.RefractiveIndex; // are we inside or outside the surface? 
                        float CosI = -Vector3.Dot(FirstShapeNormal, Ray.Direction);
                        float K = 1 - IndexOfRefraction * IndexOfRefraction * (1 - CosI * CosI);
                        Vector3 refrdir = Ray.Direction * IndexOfRefraction + FirstShapeNormal * (IndexOfRefraction * CosI - (float)Math.Sqrt(K));
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
                        Raycast(ShadowRay, out Shape FirstShadow, out Vector3 FirstShadowHit, out Vector3 FirstShadowNormal);

                        if (FirstShadow == null || FirstShadow == FirstShape)
                        {
                            //Do phong shading, additive
                            Result += Phong(FirstShape, FirstShapeHit, FirstShapeNormal, Light);
                            Result = new Vector3(MathHelper.Clamp(Result.X, 0, 1), MathHelper.Clamp(Result.Y, 0, 1), MathHelper.Clamp(Result.Z, 0, 1));
                            //Result += Phong(FirstShape, FirstShapeHit, FirstShapeNormal, Light);
                        }
                    }
                }
            }

            return Result;
        }

        public bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal)
        {
            float MinDistance = float.MaxValue;
            FirstShape = null;
            FirstShapeHit = Vector3.Zero;
            FirstShapeNormal = Vector3.Zero;
            foreach (Shape Shape in Shapes)
            {
                if (Shape.Intersect(Ray, out Vector3 Hit, out Vector3 Normal))
                {
                    float Distance = (Hit - Ray.Origin).Length();
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
            //float TexX = (1 + (float)Math.Atan2(Normal.Y, Normal.X) / MathHelper.Pi) * 0.5f;
            //float TexY = (float)Math.Acos(Normal.Z) / MathHelper.Pi;
            //var TexData = Texture.GetPixel((int)(TexX * Texture.Width), (int)(TexY * Texture.Height));
            //var TexColor = new Vector3(TexData.R / 255f, TexData.G / 255f, TexData.B / 255f);
            var TexColor = Shape.Material.Color;

            //Gamma correction
            //TexColor = new Vector3((float)Math.Pow(TexColor.X, 2.2f), (float)Math.Pow(TexColor.Y, 2.2f), (float)Math.Pow(TexColor.Z, 2.2f));

            //Diffuse
            Vector3 LightVector = Light.Origin - Hit;
            Vector3 Temp = Light.Origin - Hit;
            LightVector.Normalize();
            float CosTheta = Vector3.Dot(Normal, LightVector);
            if (CosTheta < 0.0f)
            {
                CosTheta = 0.0f;
            }
            Vector3 Diffuse = TexColor * CosTheta * Light.Intensity * Light.Color;

            //Specular
            Vector3 Halfway = (LightVector + new Vector3(0, -1f, 0));
            Halfway.Normalize();
            float NdotH = Vector3.Dot(Normal, Halfway);
            float Intensity = (float)Math.Pow(MathHelper.Clamp(NdotH, 0, 1), Shape.Material.Shininess);
            Vector3 Specular = Light.Color * Intensity * Light.Intensity;

            //Final luminance
            Vector3 Final = (Diffuse + Specular) * (1 / Temp.LengthSquared());
            Final = new Vector3(MathHelper.Clamp(Final.X, 0, 1), MathHelper.Clamp(Final.Y, 0, 1), MathHelper.Clamp(Final.Z, 0, 1));

            //Gamma correction
            //Final = new Vector3((float)Math.Pow(Final.X, 1.0 / 2.2f), (float)Math.Pow(Final.Y, 1.0 / 2.2f), (float)Math.Pow(Final.Z, 1.0 / 2.2f));

            return Final;
        }

        public void AddPixel(int X, int Y, Color Color)
        {
            Color Pixel = GetPixel(X, Y);
            if (Pixel == null)
            {
                SetPixel(X, Y, Color);
            }
            else
            {
                Framebuffer[X + Y * Width] = new Color(
                    (byte)MathHelper.Clamp(Pixel.R + Color.R, 0, 255),
                    (byte)MathHelper.Clamp(Pixel.G + Color.G, 0, 255),
                    (byte)MathHelper.Clamp(Pixel.B + Color.B, 0, 255),
                    (byte)MathHelper.Clamp(Pixel.A + Color.A, 0, 255)
                );
            }
        }

        public void SetPixel(int X, int Y, Color Data)
        {
            Framebuffer[X + Y * Width] = Data;
        }

        public Color GetPixel(int X, int Y)
        {
            return Framebuffer[X + Y * Width];
        }
    }
}
