using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class MaterialOutputNode : MaterialNode
    {
        public override MaterialNode[] Inputs { get; protected set; }

        public MaterialOutputNode(int NumInputs) 
            : base(NumInputs)
        {
        }

        public MaterialNodeValue GetFinalValue(int Index, Vector2 UV)
        {
            return Inputs[Index].Evaluate(UV);
        }

        public override MaterialNodeValue Evaluate(Vector2 UV)
        {
            throw new Exception("Material output nodes cannot be evaluated.");
        }
    }
}
