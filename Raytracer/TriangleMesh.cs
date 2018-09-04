using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Vector3[] FaceNormals { get; set; }
        public Vector3[] VertexNormals { get; set; }
        public bool SmoothShading { get; set; }
        public int NumFaces { get; private set; }
        public SpatialGrid Grid { get; set; }

        public TriangleMesh(Material Material, Matrix TransformMatrix, string Path, double GridLambda = 3, bool SmoothShading = true)
        {
            this.Material = Material;
            this.SmoothShading = SmoothShading;

            StreamReader Reader = new StreamReader(Path);
            string Line;

            //Read header
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

            //Import vertex normals if they are specified
            this.VertexNormals = new Vector3[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                Line = Reader.ReadLine();
                string[] Tokens = Line.Split(' ');
                Vertices[i] = new Vector3(double.Parse(Tokens[0], CultureInfo.InvariantCulture), double.Parse(Tokens[1], CultureInfo.InvariantCulture), double.Parse(Tokens[2], CultureInfo.InvariantCulture));
                if (Tokens.Length >= 6)
                {
                    VertexNormals[i] = new Vector3(double.Parse(Tokens[3], CultureInfo.InvariantCulture), double.Parse(Tokens[4], CultureInfo.InvariantCulture), double.Parse(Tokens[5], CultureInfo.InvariantCulture));
                }
            }

            //Import faces
            for (int i = 0; i < NumFaces; i++)
            {
                Line = Reader.ReadLine();
                string[] Tokens = Line.Split(' ');
                VertexIndices[i * 3] = int.Parse(Tokens[1]);
                VertexIndices[i * 3 + 1] = int.Parse(Tokens[2]);
                VertexIndices[i * 3 + 2] = int.Parse(Tokens[3]);
            }

            //Move mesh to wanted location, calculated spatial grid
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], TransformMatrix);
            }
            Grid = new SpatialGrid(this, GridLambda);

            CalculateNormals();
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            return Grid.Intersect(Ray, out Hit, out Normal);
        }

        public bool IntersectTriangle(Ray Ray, int Index, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;

            Vector3 V0 = Vertices[VertexIndices[Index * 3]];
            Vector3 V1 = Vertices[VertexIndices[Index * 3 + 1]];
            Vector3 V2 = Vertices[VertexIndices[Index * 3 + 2]];

            Vector3 N = FaceNormals[Index];

            //Backface culling
            if (Vector3.Dot(N, Ray.Direction) > 0)
            {
                return false;
            }

            //Triangle's plane intersection
            double Denom = Vector3.Dot(-N, Ray.Direction);
            if (Denom <= 1e-6)
            {
                return false;
            }
            Vector3 RayToPlane = V0 - Ray.Origin;
            double T = Vector3.Dot(RayToPlane, -N) / Denom;

            //Triangle behind ray
            if (T < 0)
            {
                return false;
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
                return false;
            }

            //Edge B
            Vector3 EdgeB = V2 - V1;
            Vector3 VPB = CurrentHit - V1;
            C = Vector3.Cross(EdgeB, VPB);
            if ((CurrentU = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            //Edge C
            Vector3 EdgeC = V0 - V2;
            Vector3 VPC = CurrentHit - V2;
            C = Vector3.Cross(EdgeC, VPC);
            if ((CurrentV = Vector3.Dot(N, C)) < 0)
            {
                return false;
            }

            /*double U = 0, V = 0;
            Vector3 PVec = Vector3.Cross(Ray.Direction, B);
            double Det = Vector3.Dot(A, PVec);
            double InvDet = 1.0 / Det;
            Vector3 TVec = Ray.Origin - V0;
            U = Vector3.Dot(TVec, PVec) * InvDet;
            Vector3 QVec = Vector3.Cross(TVec, A);
            V = Vector3.Dot(Ray.Direction, QVec) * InvDet;*/

            Hit = CurrentHit;

            if (SmoothShading)
            {
                //Area of triangles
                double AreaABC = Vector3.Dot(N, Vector3.Cross((V1 - V0), (V2 - V0)));
                double AreaPBC = Vector3.Dot(N, Vector3.Cross((V1 - CurrentHit), (V2 - CurrentHit)));
                double AreaPCA = Vector3.Dot(N, Vector3.Cross((V2 - CurrentHit), (V0 - CurrentHit)));

                //Barycentric coords
                double BaryAlpha = AreaPBC / AreaABC;
                double BaryBeta = AreaPCA / AreaABC;
                double BaryGamma = 1.0 - BaryAlpha - BaryBeta;

                Normal = Vector3.Normalize(BaryAlpha * VertexNormals[VertexIndices[Index * 3]] + BaryBeta * VertexNormals[VertexIndices[Index * 3 + 1]] + BaryGamma * VertexNormals[VertexIndices[Index * 3 + 2]]);
            }
            else
            {
                Normal = N;
            }

            return true;
        }

        [Obsolete("This method requires recalculation of the grid, pass a matrix to the constructor instead.")]
        public void Transform(Matrix TransformMatrix, double GridLambda = 3)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], TransformMatrix);
            }

            Grid = new SpatialGrid(this, GridLambda);
            CalculateNormals();
        }
        
        private void CalculateNormals()
        {
            FaceNormals = new Vector3[NumFaces];
            for (int i = 0; i < FaceNormals.Length; i++)
            {
                Vector3 V0 = Vertices[VertexIndices[i * 3]];
                Vector3 V1 = Vertices[VertexIndices[i * 3 + 1]];
                Vector3 V2 = Vertices[VertexIndices[i * 3 + 2]];

                FaceNormals[i] = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));

                //TODO: Recalculate vertex normals
            }
        }
    }
}
