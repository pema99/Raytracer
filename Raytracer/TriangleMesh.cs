using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class TriangleMesh : Shape
    {
        public override Material Material { get; set; }
        public Vector3[] Vertices { get; set; }
        public int[] VertexIndices { get; set; }
        public Vector3[] Normals { get; set; }
        public int NumFaces { get; private set; }
        public Vector3 AABBMin { get; private set; }
        public Vector3 AABBMax { get; private set; }

        public TriangleMesh(Material Material, Vector3[] Vertices, int[] VertexIndices, Vector3[] Normals)
        {
            this.Material = Material;
            this.Vertices = Vertices;
            this.VertexIndices = VertexIndices;
            this.Normals = Normals;
            this.NumFaces = VertexIndices.Length / 3;

            CalculateNormals();
            CalculateAABB();
        }

        public TriangleMesh(Material Material, Vector3[] Vertices, int[] VertexIndices)
        {
            this.Material = Material;
            this.Vertices = Vertices;
            this.VertexIndices = VertexIndices;
            this.NumFaces = VertexIndices.Length / 3;

            CalculateNormals();
            CalculateAABB();
        }

        public TriangleMesh(Material Material, string Path)
        {
            this.Material = Material;

            StreamReader Reader = new StreamReader(Path);
            string Line;
            while ((Line = Reader.ReadLine()) != null)
            {
                string[] Tokens = Line.Split(' ');
                if (Tokens[0] == "element")
                {
                    if (Tokens[1] == "vertex")
                    {
                        Vertices = new Vector3[int.Parse(Tokens[2])];
                    }
                    if (Tokens[1] == "face")
                    {
                        NumFaces = int.Parse(Tokens[2]);
                        VertexIndices = new int[NumFaces * 3];
                    }
                }
                if (Tokens[0] == "end_header")
                {
                    break;
                }
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Line = Reader.ReadLine();
                string[] Tokens = Line.Split(' ');
                Vertices[i] = new Vector3(double.Parse(Tokens[0], CultureInfo.InvariantCulture), double.Parse(Tokens[1], CultureInfo.InvariantCulture), double.Parse(Tokens[2], CultureInfo.InvariantCulture));
            }
            for (int i = 0; i < NumFaces; i++)
            {
                Line = Reader.ReadLine();
                string[] Tokens = Line.Split(' ');
                VertexIndices[i * 3] = int.Parse(Tokens[1]);
                VertexIndices[i * 3 + 1] = int.Parse(Tokens[2]);
                VertexIndices[i * 3 + 2] = int.Parse(Tokens[3]);
            }

            CalculateNormals();
            CalculateAABB();
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            double MinDistance = double.MaxValue;
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            double MinU = 0, MinV = 0;

            if (!IntersectAABB(Ray))
            {
                return false;
            }

            for (int i = 0; i < NumFaces; i++)
            {
                Vector3 V0 = Vertices[VertexIndices[i * 3]];
                Vector3 V1 = Vertices[VertexIndices[i * 3 + 1]];
                Vector3 V2 = Vertices[VertexIndices[i * 3 + 2]];

                Vector3 A = V1 - V0;
                Vector3 B = V2 - V0;
                Vector3 N = Normals[i];

                //Backface culling
                if (Vector3.Dot(N, Ray.Direction) > 0)
                {
                    continue;
                }

                //Triangle's plane intersection
                double Denom = Vector3.Dot(-N, Ray.Direction);
                if (Denom <= 1e-6)
                {
                    continue;
                }
                Vector3 RayToPlane = V0 - Ray.Origin;
                double T = Vector3.Dot(RayToPlane, -N) / Denom;

                //Triangle behind ray
                if (T < 0)
                {
                    continue;
                }

                //Find hit point
                Vector3 CurrentHit = Ray.Origin + T * Ray.Direction;

                //Check if hit is in triangle
                double CurrentU, CurrentV;

                //Edge A
                Vector3 EdgeA = V1 - V0;
                Vector3 VPA = CurrentHit - V0;
                Vector3 C = Vector3.Cross(EdgeA, VPA);
                if (Vector3.Dot(N, C) < 0)
                {
                    continue;
                }

                //Edge B
                Vector3 EdgeB = V2 - V1;
                Vector3 VPB = CurrentHit - V1;
                C = Vector3.Cross(EdgeB, VPB);
                if ((CurrentU = Vector3.Dot(N, C)) < 0)
                {
                    continue;
                }

                //Edge C
                Vector3 EdgeC = V0 - V2;
                Vector3 VPC = CurrentHit - V2;
                C = Vector3.Cross(EdgeC, VPC);
                if ((CurrentV = Vector3.Dot(N, C)) < 0)
                {
                    continue;
                }

                //Scale UV's
                double UVScale = Vector3.Dot(N, N);
                CurrentU /= UVScale;
                CurrentV /= UVScale;

                //Only want closest intersection
                double CurrentDistance = (CurrentHit - Ray.Origin).Length();
                if (CurrentDistance < MinDistance)
                {
                    MinDistance = CurrentDistance;
                    Hit = CurrentHit;
                    Normal = N;
                    MinU = CurrentU;
                    MinV = CurrentV;
                }
            }

            if (MinDistance < double.MaxValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Transform(Matrix TransformMatrix)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], TransformMatrix);
            }
            CalculateNormals();
            CalculateAABB();
        }
        
        public void CalculateNormals()
        {
            Normals = new Vector3[NumFaces];
            for (int i = 0; i < Normals.Length; i++)
            {
                Vector3 V0 = Vertices[VertexIndices[i * 3]];
                Vector3 V1 = Vertices[VertexIndices[i * 3 + 1]];
                Vector3 V2 = Vertices[VertexIndices[i * 3 + 2]];

                Normals[i] = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
            }
        }

        public void CalculateAABB()
        {
            double MinX = double.MaxValue;
            double MinY = double.MaxValue;
            double MinZ = double.MaxValue;
            double MaxX = double.MinValue;
            double MaxY = double.MinValue;
            double MaxZ = double.MinValue;
            foreach (Vector3 Vertex in Vertices)
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

        public bool IntersectAABB(Ray r)
        {
            double TMin = (AABBMin.X - r.Origin.X) / r.Direction.X;
            double TMax = (AABBMax.X - r.Origin.X) / r.Direction.X;

            if (TMin > TMax)
            {
                double Temp = TMin;
                TMin = TMax;
                TMax = Temp;
            }

            double TYMax = (AABBMax.Y - r.Origin.Y) / r.Direction.Y;
            double TYMin = (AABBMin.Y - r.Origin.Y) / r.Direction.Y;

            if (TYMin > TYMax)
            {
                double Temp = TYMin;
                TYMin = TYMax;
                TYMax = Temp;
            }

            if ((TMin > TYMax) || (TYMin > TMax))
                return false;

            if (TYMin > TMin)
                TMin = TYMin;

            if (TYMax < TMax)
                TMax = TYMax;

            double TZMin = (AABBMin.Z - r.Origin.Z) / r.Direction.Z;
            double TZMax = (AABBMax.Z - r.Origin.Z) / r.Direction.Z;

            if (TZMin > TZMax)
            {
                double Temp = TZMin;
                TZMin = TZMax;
                TZMax = Temp;
            }

            if ((TMin > TZMax) || (TZMin > TMax))
                return false;

            if (TZMin > TMin)
                TMin = TZMin;

            if (TZMax < TMax)
                TMax = TZMax;

            return true;
        }
    }
}
