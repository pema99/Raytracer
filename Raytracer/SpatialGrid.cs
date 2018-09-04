using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class SpatialGrid
    {
        public Vector3 AABBMin { get; set; }
        public Vector3 AABBMax { get; set; }
        public SpatialGridNode[,,] Grid { get; set; }
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

            this.Grid = new SpatialGridNode[(int)GridResolution.X, (int)GridResolution.Y, (int)GridResolution.Z];
            this.CellSize = new Vector3((AABBMax.X - AABBMin.X) / GridResolution.X, (AABBMax.Y - AABBMin.Y) / GridResolution.Y, (AABBMax.Z - AABBMin.Z) / GridResolution.Z);

            //Initialize grid
            for (int x = 0; x < GridResolution.X; x++)
            {
                for (int y = 0; y < GridResolution.Y; y++)
                {
                    for (int z = 0; z < GridResolution.Z; z++)
                    {
                        Grid[x, y, z] = new SpatialGridNode();
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
                            Grid[x, y, z].TriangleIndices.Add(i);
                        }
                    }
                }
            }
        }

        public bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;

            if (!Util.IntersectAABB(Ray, AABBMin, AABBMax, out Vector3 AABBHit, out double TMin, out double TMax))
            {
                return false;
            }
                                                                                                                                                                           
            Vector3 Exit = Vector3.Zero;
            Vector3 Step = Vector3.Zero;
            Vector3 CurrentCell = Vector3.Zero;
            Vector3 StepDelta = Vector3.Zero;
            Vector3 NextIntersection = Vector3.Zero;

            //Ray start to cell coords
            for (int i = 0; i < 3; i++)
            {                                                                                                                                              
                double RayOriginCell = (AABBHit[i] - AABBMin[i]);
                CurrentCell[i] = MathHelper.Clamp(Math.Floor(RayOriginCell / CellSize[i]), 0, GridResolution[i] - 1);
                if (Ray.Direction[i] < 0)
                {
                    StepDelta[i] = -CellSize[i] * (1 / Ray.Direction[i]);
                    NextIntersection[i] = TMin + (CurrentCell[i] * CellSize[i] - RayOriginCell) * (1 / Ray.Direction[i]);
                    Exit[i] = -1;
                    Step[i] = -1;
                }
                else
                {
                    StepDelta[i] = CellSize[i] * (1 / Ray.Direction[i]);
                    NextIntersection[i] = TMin + ((CurrentCell[i] + 1) * CellSize[i] - RayOriginCell) * (1 / Ray.Direction[i]);
                    Exit[i] = GridResolution[i];
                    Step[i] = 1;
                }
            }

            //Keep going until we hit something or are out of bounds, DDA Algorithm                                                                                                                                                            
            while (true)
            {
                double MinDistance = double.MaxValue;
                foreach (int Tri in Grid[(int)CurrentCell.X, (int)CurrentCell.Y, (int)CurrentCell.Z].TriangleIndices)
                {
                    if (Owner.IntersectTriangle(Ray, Tri, out Vector3 TriHit, out Vector3 TriNormal))
                    {
                        double CurrentDistance = (TriHit - Ray.Origin).Length();
                        if (CurrentDistance < MinDistance)
                        {
                            MinDistance = CurrentDistance;
                            Hit = TriHit;
                            Normal = TriNormal;
                        }
                    }
                }
                if (MinDistance < double.MaxValue)
                {
                    return true;
                }

                //Bit shifting technique from scratchapixel, optimization. B0 = Y < Z, B1 = X < Z, B2 = X < Y
                int AxisIndex = (Util.BoolToInt(NextIntersection[0] < NextIntersection[1]) << 2) + (Util.BoolToInt(NextIntersection[0] < NextIntersection[2]) << 1) + (Util.BoolToInt(NextIntersection[1] < NextIntersection[2]));
                int Axis = AxisMap[AxisIndex];

                if (TMax < NextIntersection[Axis])
                {
                    return false;
                }

                //Step in on current axis
                CurrentCell[Axis] += Step[Axis];

                //If we are out bounds return
                if (CurrentCell[Axis] == Exit[Axis])
                {
                    return false;
                }

                //Update next intersection axis
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
