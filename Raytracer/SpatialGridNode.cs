using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class SpatialGridNode
    {
        public List<int> TriangleIndices { get; set; }

        public SpatialGridNode()
        {
            this.TriangleIndices = new List<int>();
        }
    }
}
