using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class MaterialNormalNode : MaterialNode
    {
        public override MaterialNode[] Inputs { get; protected set; }
        public Texture Texture { get; set; }

        public MaterialNormalNode(Texture Texture)
            : base(0)
        {
            this.Texture = Texture;
        }

        public override MaterialNodeValue Evaluate(Vector2 UV)
        {
            Vector3 Normal = 2 * Texture.GetColorAtUV(UV);
            //Z is up in tangent space of normalmaps
            return new MaterialNodeValue(new Vector3(Normal.X - 1, Normal.Z - 1, Normal.Y - 1));
        }
    }
}
