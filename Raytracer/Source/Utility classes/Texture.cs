using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public Texture(string Path)
        {
            Bitmap File = new Bitmap(Path);
            this.Data = new Vector3[File.Width, File.Height];
            for (int x = 0; x < File.Width; x++)
            {
                for (int y = 0; y < File.Height; y++)
                {
                    Data[x, y] = File.GetPixel(x, y).ToVector3();
                }
            }
            this.Width = File.Width;
            this.Height = File.Height;
        }

        public Vector3 GetColorAtUV(Vector2 UV)
        {
            return Data[(int)(UV.X * Width), (int)(UV.Y * Height)];
        }
    }
}
