using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public abstract class MaterialNode
    {
        public abstract MaterialNode[] Inputs { get; protected set; }
        public abstract MaterialNodeValue Evaluate(Vector2 UV);

        public MaterialNode(int NumInputs)
        {
            this.Inputs = new MaterialNode[NumInputs];
        }
    }
}
