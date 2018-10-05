using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Core
{
    public class SpatialGrid
    {
        public Vector3 AABBMin { get; set; }
        public Vector3 AABBMax { get; set; }
        public List<int>[,,] Grid { get; set; }
        public Vector3 GridResolution { get; set; }
        public Vector3 CellSize { get; set; }
        public TriangleMesh Owner { get; set; }

        private readonly int[] AxisMap = { 2, 1, 2, 1, 2, 2, 0, 0 };

        public SpatialGrid(TriangleMesh Owner, double GridLambda = 3)
        {
            this.Owner = Owner;
            this.CalculateAABB();

            //Calculate grid resolution using cleary's approximation
            double GridResolutionTerm = Math.Pow((GridLambda * Owner.NumFaces) / ((AABBMax.X - AABBMin.X) * (AABBMax.Y - AABBMin.Y) * (AABBMax.Z - AABBMin.Z)), (1.0 / 3.0));
            this.GridResolution = new Vector3(Math.Floor((AABBMax.X - AABBMin.X) * GridResolutionTerm), Math.Floor((AABBMax.Y - AABBMin.Y) * GridResolutionTerm), Math.Floor((AABBMax.Z - AABBMin.Z) * GridResolutionTerm));

            this.Grid = new List<int>[(int)GridResolution.X, (int)GridResolution.Y, (int)GridResolution.Z];
            this.CellSize = new Vector3((AABBMax.X - AABBMin.X) / GridResolution.X, (AABBMax.Y - AABBMin.Y) / GridResolution.Y, (AABBMax.Z - AABBMin.Z) / GridResolution.Z);

            //Initialize grid
            for (int x = 0; x < GridResolution.X; x++)
            {
                for (int y = 0; y < GridResolution.Y; y++)
                {
                    for (int z = 0; z < GridResolution.Z; z++)
                    {
                        Grid[x, y, z] = new List<int>();
                    }
                }
            }

            //Insert triangles
            for (int i = 0; i < Owner.NumFaces; i++)
            {
                //Get verts
                Vector3[] Verts = new Vector3[3];
                Verts[0] = Owner.Vertices[Owner.VertexIndices[i * 3]];
                Verts[1] = Owner.Vertices[Owner.VertexIndices[i * 3 + 1]];
                Verts[2] = Owner.Vertices[Owner.VertexIndices[i * 3 + 2]];

                //Calculating bounding box coords in cell space
                Util.CalculateTriangleAABB(Verts, out Vector3 TriAABBMin, out Vector3 TriAABBMax);
                Vector3 CellMin = TriAABBMin - AABBMin;
                Vector3 CellMax = TriAABBMax - AABBMin;
                CellMin = new Vector3(
                    MathHelper.Clamp(Math.Floor(CellMin.X / CellSize.X), 0, Grid.GetLength(0) - 1),
                    MathHelper.Clamp(Math.Floor(CellMin.Y / CellSize.Y), 0, Grid.GetLength(1) - 1),
                    MathHelper.Clamp(Math.Floor(CellMin.Z / CellSize.Z), 0, Grid.GetLength(2) - 1));
                CellMax = new Vector3(
                    MathHelper.Clamp(Math.Floor(CellMax.X / CellSize.X), 0, Grid.GetLength(0) - 1),
                    MathHelper.Clamp(Math.Floor(CellMax.Y / CellSize.Y), 0, Grid.GetLength(1) - 1),
                    MathHelper.Clamp(Math.Floor(CellMax.Z / CellSize.Z), 0, Grid.GetLength(2) - 1));

                for (int x = (int)CellMin.X; x <= CellMax.X; x++)
                {
                    for (int y = (int)CellMin.Y; y <= CellMax.Y; y++)
                    {
                        for (int z = (int)CellMin.Z; z <= CellMax.Z; z++)
                        {
                            Grid[x, y, z].Add(i);
                        }
                    }
                }
            }
        }

        public bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            UV = Vector2.Zero;

            //AABB check
            if (!Util.IntersectAABB(Ray, AABBMin, AABBMax, out Vector3 AABBHit, out double TMin, out double TMax))
            {
                return false;
            }

            //Setup starting cell, either inside or on AABB
            Vector3 Start = Ray.Origin - AABBMin;
            Vector3 Cell = new Vector3((int)(Start.X / CellSize.X - 0.000000001), (int)(Start.Y / CellSize.Y - 0.000000001), (int)(Start.Z / CellSize.Z - 0.000000001));
            if (Cell.X < 0 || Cell.Y < 0 || Cell.Z < 0 ||
                Cell.X >= GridResolution.X || Cell.Y >= GridResolution.Y || Cell.Z >= GridResolution.Z) {
                Start = AABBHit - AABBMin;
                Cell = new Vector3((int)(Start.X / CellSize.X - 0.000000001), (int)(Start.Y / CellSize.Y - 0.000000001), (int)(Start.Z / CellSize.Z - 0.000000001));
            }

            //Setup values for DDA
            Vector3 Step = new Vector3(Math.Sign(Ray.Direction.X), Math.Sign(Ray.Direction.Y), Math.Sign(Ray.Direction.Z));
            Vector3 StepDelta = Vector3.Zero;
            Vector3 NextIntersection = Vector3.Zero;
            Vector3 Exit = Vector3.Zero;
            for (int i = 0; i < 3; i++)
            {
                if (Ray.Direction[i] < 0)
                {
                    StepDelta[i] = -CellSize[i] / Ray.Direction[i];
                    NextIntersection[i] = (Cell[i] * CellSize[i] - Start[i]) / Ray.Direction[i];
                    Exit[i] = -1;
                }
                else
                {
                    StepDelta[i] = CellSize[i] / Ray.Direction[i];
                    NextIntersection[i] = ((Cell[i] + 1) * CellSize[i] - Start[i]) / Ray.Direction[i];
                    Exit[i] = GridResolution[i];
                }
            }

            //DDA algorithm
            while (true)
            {
                //Ray triangle checks
                double MinDistance = double.MaxValue;
                foreach (int Tri in Grid[(int)Cell.X, (int)Cell.Y, (int)Cell.Z])
                {
                    if (Owner.IntersectTriangle(Ray, Tri, out Vector3 TriHit, out Vector3 TriNormal, out Vector2 TriUV))
                    {
                        double CurrentDistance = (TriHit - Ray.Origin).Length();
                        if (CurrentDistance < MinDistance)
                        {
                            MinDistance = CurrentDistance;
                            Hit = TriHit;
                            Normal = TriNormal;
                            UV = TriUV;
                        }
                    }
                }
                if (MinDistance < double.MaxValue)
                {
                    return true;
                }

                //Check current axis
                int Axis = AxisMap[(Util.BoolToInt(NextIntersection.X < NextIntersection.Y) << 2) +
                                   (Util.BoolToInt(NextIntersection.X < NextIntersection.Z) << 1) +
                                   (Util.BoolToInt(NextIntersection.Y < NextIntersection.Z))];

                //Do step
                Cell[Axis] += Step[Axis];

                //Check out of bounds
                if (Cell[Axis] == Exit[Axis])
                {
                    return false;
                }

                //Set next intersection
                NextIntersection[Axis] += StepDelta[Axis];
            }
        }

        private void CalculateAABB()
        {
            double MinX = double.MaxValue;
            double MinY = double.MaxValue;
            double MinZ = double.MaxValue;
            double MaxX = double.MinValue;
            double MaxY = double.MinValue;
            double MaxZ = double.MinValue;
            foreach (Vector3 Vertex in Owner.Vertices)
            {
                if (Vertex.X < MinX)
                {
                    MinX = Vertex.X;
                }
                if (Vertex.Y < MinY)
                {
                    MinY = Vertex.Y;
                }
                if (Vertex.Z < MinZ)
                {
                    MinZ = Vertex.Z;
                }
                if (Vertex.X > MaxX)
                {
                    MaxX = Vertex.X;
                }
                if (Vertex.Y > MaxY)
                {
                    MaxY = Vertex.Y;
                }
                if (Vertex.Z > MaxZ)
                {
                    MaxZ = Vertex.Z;
                }
            }
            AABBMin = new Vector3(MinX, MinY, MinZ);
            AABBMax = new Vector3(MaxX, MaxY, MaxZ);
        }
    }
}
