using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public struct MaterialNodeValue
    {
        public MaterialNodeValueType Type { get; set; }

        public Texture Texture { get; set; }
        public Vector3 Color { get; set; }
        public double Number { get; set; }

        public MaterialNodeValue(Texture Texture)
        {
            this.Type = MaterialNodeValueType.Texture;
            this.Texture = Texture;
            this.Color = Vector3.Zero;
            this.Number = 0;
        }

        public MaterialNodeValue(Vector3 Color)
        {
            this.Type = MaterialNodeValueType.Color;
            this.Texture = null;
            this.Color = Color;
            this.Number = 0;
        }

        public MaterialNodeValue(double Number)
        {
            this.Type = MaterialNodeValueType.Number;
            this.Texture = null;
            this.Color = Vector3.Zero;
            this.Number = Number;
        }

        public static implicit operator Texture(MaterialNodeValue M)
        {
            switch (M.Type)
            {
                case MaterialNodeValueType.Texture:
                    return M.Texture;

                case MaterialNodeValueType.Color:
                    return new Texture(new Vector3[,] { { M.Color } });

                case MaterialNodeValueType.Number:
                    return new Texture(new Vector3[,] { { new Vector3(M.Number) } });

                default:
                    return null;
            }
        }

        public static implicit operator Vector3(MaterialNodeValue M)
        {
            switch (M.Type)
            {
                case MaterialNodeValueType.Texture:
                    return M.Texture.Data[0, 0];

                case MaterialNodeValueType.Color:
                    return M.Color;

                case MaterialNodeValueType.Number:
                    return new Vector3(M.Number);

                default:
                    return Vector3.Zero;
            }
        }

        public static implicit operator double(MaterialNodeValue M)
        {
            switch (M.Type)
            {
                case MaterialNodeValueType.Texture:
                    return M.Texture.Data[0, 0].X;

                case MaterialNodeValueType.Color:
                    return M.Color.X;

                case MaterialNodeValueType.Number:
                    return M.Number;

                default:
                    return 0;
            }
        }
    }
}
