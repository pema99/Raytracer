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
        public int NumFaces { get; private set; }
        public SpatialGrid Grid { get; set; }

        public bool SmoothShading { get; set; }
        public bool BackFaceCulling { get; set; }

        public TriangleMesh(Material Material, Matrix TransformMatrix, string Path, double GridLambda = 3, bool SmoothShading = true, bool BackFaceCulling = true)
        {
            this.Material = Material;
            this.SmoothShading = SmoothShading;
            this.BackFaceCulling = BackFaceCulling;

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

        //Möller-Trumbore algorithm
        public bool IntersectTriangle(Ray Ray, int Index, out Vector3 Hit, out Vector3 Normal)
        {
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;

            double Epsilon = 0.000001;

            Vector3 V0 = Vertices[VertexIndices[Index * 3]];
            Vector3 V1 = Vertices[VertexIndices[Index * 3 + 1]];
            Vector3 V2 = Vertices[VertexIndices[Index * 3 + 2]];

            Vector3 EdgeA = V1 - V0;
            Vector3 EdgeB = V2 - V0;

            Vector3 P = Vector3.Cross(Ray.Direction, EdgeB);
            double Determinant = Vector3.Dot(EdgeA, P);

            if (BackFaceCulling)
            {
                if (Determinant < Epsilon)
                {
                    return false;
                }
            }
            else
            {
                if (Math.Abs(Determinant) < Epsilon)
                {
                    return false;
                }
            }

            double InvDeterminant = 1.0 / Determinant;
            Vector3 S = Ray.Origin - V0;
            double U = InvDeterminant * Vector3.Dot(S, P);
            if (U < 0.0 || U > 1.0)
            {
                return false;
            }

            Vector3 Q = Vector3.Cross(S, EdgeA);
            double V = InvDeterminant * Vector3.Dot(Ray.Direction, Q);
            if (V < 0.0 || U + V > 1.0)
            {
                return false;
            }

            double T = InvDeterminant * Vector3.Dot(EdgeB, Q);
            if (T > Epsilon)
            {
                Hit = Ray.Origin + Ray.Direction * T;

                Vector3 N = FaceNormals[Index];
                if (SmoothShading)
                {
                    //Area of triangles
                    double AreaABC = Vector3.Dot(N, Vector3.Cross((V1 - V0), (V2 - V0)));
                    double AreaPBC = Vector3.Dot(N, Vector3.Cross((V1 - Hit), (V2 - Hit)));
                    double AreaPCA = Vector3.Dot(N, Vector3.Cross((V2 - Hit), (V0 - Hit)));

                    //Barycentric coords
                    double BaryAlpha = AreaPBC / AreaABC;
                    double BaryBeta = AreaPCA / AreaABC;
                    double BaryGamma = 1.0 - BaryAlpha - BaryBeta;

                    //Interpolated vertex normal
                    Normal = Vector3.Normalize(BaryAlpha * VertexNormals[VertexIndices[Index * 3]] + BaryBeta * VertexNormals[VertexIndices[Index * 3 + 1]] + BaryGamma * VertexNormals[VertexIndices[Index * 3 + 2]]);
                }
                else
                {
                    Normal = N;
                }

                return true;
            }

            return false;
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
