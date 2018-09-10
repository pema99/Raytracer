using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Texture
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector3[,] Data { get; private set; }
        
        public Texture(Vector3[,] Data)
        {
            this.Data = Data;
            this.Width = Data.GetLength(0);
            this.Height = Data.GetLength(1);
        }

        public Texture(string Path, bool sRGB = false)
        {
            Bitmap BMP = new Bitmap(Path);
            BitmapData BMPData = BMP.LockBits(new Rectangle(0, 0, BMP.Width, BMP.Height), ImageLockMode.ReadOnly, BMP.PixelFormat);
            byte[] Pixels = new byte[BMPData.Stride * BMP.Height];
            Marshal.Copy(BMPData.Scan0, Pixels, 0, Pixels.Length);
            int BPP = Bitmap.GetPixelFormatSize(BMP.PixelFormat) / 8;

            this.Data = new Vector3[BMP.Width, BMP.Height];

            for (int y = 0; y < BMP.Height; y++)
            {
                for (int x = 0; x < BMP.Width; x++)
                {
                    Data[x, y] = new Vector3(Pixels[y * BMPData.Stride + x * BPP + 2] / 255.0, Pixels[y * BMPData.Stride + x * BPP + 1] / 255.0, Pixels[y * BMPData.Stride + x * BPP] / 255.0);
                    if (sRGB)
                    {
                        Data[x, y].X = Math.Pow(Data[x, y].X, 2.2);
                        Data[x, y].Y = Math.Pow(Data[x, y].Y, 2.2);
                        Data[x, y].Z = Math.Pow(Data[x, y].Z, 2.2);
                    }
                }
            }

            this.Width = BMP.Width;
            this.Height = BMP.Height;
        }

        public Vector3 GetColorAtUV(Vector2 UV)
        {
            return Data[(int)(UV.X * Width), (int)(Height - UV.Y * Height)];
        }
    }
}
