using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
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
    }
}
