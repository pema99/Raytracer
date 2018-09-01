using System;
using System.Collections.Generic;
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
        public int NumFaces { get; private set; }

        public TriangleMesh(Material Material, Vector3[] Vertices, int[] Indices)
        {
            this.Material = Material;
            this.Vertices = Vertices;
            this.VertexIndices = Indices;
            this.NumFaces = VertexIndices.Length / 3;
        }

        public override bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal)
        {
            double MinDistance = double.MaxValue;
            Hit = Vector3.Zero;
            Normal = Vector3.Zero;
            double MinU = 0, MinV = 0;

            Vector3 V0, V1, V2;
            Vector3 A, B;
            Vector3 N;
            double Denom;
            Vector3 RayToPlane;
            double T;
            Vector3 CurrentHit;
            Vector3 C;
            double CurrentU, CurrentV;
            Vector3 EdgeA, EdgeB, EdgeC;
            Vector3 VPA, VPB, VPC;
            double CurrentDistance;
            double UVScale;
            for (int i = 0; i < NumFaces; i++)
            {
                V0 = Vertices[VertexIndices[i * 3]];
                V1 = Vertices[VertexIndices[i * 3 + 1]];
                V2 = Vertices[VertexIndices[i * 3 + 2]];

                A = V1 - V0;
                B = V2 - V0;
                N = Vector3.Normalize(Vector3.Cross(A, B));

                //Backface culling
                if (Vector3.Dot(N, Ray.Direction) > 0)
                {
                    continue;
                }

                //Triangle's plane intersection
                Denom = Vector3.Dot(-N, Ray.Direction);
                if (Denom <= 1e-6)
                {
                    continue;
                }
                RayToPlane = V0 - Ray.Origin;
                T = Vector3.Dot(RayToPlane, -N) / Denom;

                //Triangle behind ray
                if (T < 0)
                {
                    continue;
                }

                //Find hit point
                CurrentHit = Ray.Origin + T * Ray.Direction;

                //Check if hit is in triangle
                //Edge A
                EdgeA = V1 - V0;
                VPA = CurrentHit - V0;
                C = Vector3.Cross(EdgeA, VPA);
                if (Vector3.Dot(N, C) < 0)
                {
                    continue;
                }

                //Edge B
                EdgeB = V2 - V1;
                VPB = CurrentHit - V1;
                C = Vector3.Cross(EdgeB, VPB);
                if ((CurrentU = Vector3.Dot(N, C)) < 0)
                {
                    continue;
                }

                //Edge C
                EdgeC = V0 - V2;
                VPC = CurrentHit - V2;
                C = Vector3.Cross(EdgeC, VPC);
                if ((CurrentV = Vector3.Dot(N, C)) < 0)
                {
                    continue;
                }

                //Scale UV's
                UVScale = Vector3.Dot(N, N);
                CurrentU /= UVScale;
                CurrentV /= UVScale;

                //Only want closest intersection
                CurrentDistance = (CurrentHit - Ray.Origin).Length();
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
    }
}
