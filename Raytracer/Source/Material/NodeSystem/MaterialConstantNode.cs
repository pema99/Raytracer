using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class MaterialConstantNode : MaterialNode
    {
        public override MaterialNode[] Inputs { get; protected set; }
        public MaterialNodeValue Constant { get; set; }

        public MaterialConstantNode(Vector3 Color)
            : base(0)
        {
            this.Constant = new MaterialNodeValue(Color);
        }

        public MaterialConstantNode(Texture Texture)
            : base(0)
        {
            this.Constant = new MaterialNodeValue(Texture);
        }

        public MaterialConstantNode(double Number)
            : base(0)
        {
            this.Constant = new MaterialNodeValue(Number);
        }

        public override MaterialNodeValue Evaluate(Vector2 UV)
        {
            return Constant;
        }
    }
}
