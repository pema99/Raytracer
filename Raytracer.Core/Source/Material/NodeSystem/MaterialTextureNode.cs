using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class MaterialTextureNode : MaterialNode
    {
        public override MaterialNode[] Inputs { get; protected set; }
        public Texture Texture { get; set; }

        public MaterialTextureNode(Texture Texture)
            : base(0)
        {
            this.Texture = Texture;
        }

        public override MaterialNodeValue Evaluate(Vector2 UV)
        {
            return new MaterialNodeValue(Texture.GetColorAtUV(UV));
        }
    }
}
