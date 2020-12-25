using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshLabInternal
{

    public class Triangle
    {
        public int a, b, c;
        public int index;

        public Edge e1, e2, e3;

        public List<Triangle> neighbors;

        public Triangle(int a, int b, int c, int index)
        {
            neighbors = new List<Triangle>();
            this.a = a;
            this.b = b;
            this.c = c;
            this.index = index;
        }

        public bool IsBackFaceOf(Triangle other)
        {
            return a == other.c && b == other.b && c == other.a;
        }

        public void GenerateEdges(List<Edge> edges)
        {
            Edge e1 = new Edge(a, b);
            Edge e2 = new Edge(b, c);
            Edge e3 = new Edge(c, a);
            bool b1 = false, b2 = false, b3 = false;
            for (int i = 0; i < edges.Count; i++)
            {
                Edge e = edges[i];
                if (e.IsSame(e1))
                {
                    e1 = e;
                    b1 = true;
                    continue;
                }
                if (e.IsSame(e2))
                {
                    e2 = e;
                    b2 = true;
                    continue;
                }
                if (e.IsSame(e3))
                {
                    e3 = e;
                    b3 = true;
                }
            }

            this.e1 = e1;
            this.e2 = e2;
            this.e3 = e3;

            if (!b1)
            {
                edges.Add(e1);
            }
            if (!b2)
            {
                edges.Add(e2);
            }
            if (!b3)
            {
                edges.Add(e3);
            }
        }

        public void AddNeighbor(Triangle other)
        {
            neighbors.Add(other);
        }

        public bool IsNeighborOf(Triangle other, Vector3[] vs)
        {
            bool sharesVertex = a == other.a || a == other.b || a == other.c || b == other.a || b == other.b || b == other.c || c == other.a || c == other.b || c == other.c;

            if (sharesVertex)
            {
                return true;
            }

            Vector3 v11 = vs[a], v12 = vs[b], v13 = vs[c], v21 = vs[other.a], v22 = vs[other.b], v23 = vs[other.c];

            float d11 = Vector3.SqrMagnitude(v11 - v21), d12 = Vector3.SqrMagnitude(v11 - v22), d13 = Vector3.SqrMagnitude(v11 - v23),
                d21 = Vector3.SqrMagnitude(v12 - v21), d22 = Vector3.SqrMagnitude(v12 - v22), d23 = Vector3.SqrMagnitude(v12 - v23),
                d31 = Vector3.SqrMagnitude(v13 - v21), d32 = Vector3.SqrMagnitude(v13 - v22), d33 = Vector3.SqrMagnitude(v13 - v23);

            float dMin = Mathf.Min(d11, d12, d13, d21, d22, d23, d31, d32, d33);

            if (Mathf.Approximately(dMin, 0.0f))
            {
                return true;
            }
            return false;

        }

        public bool SharesEdgeWith(Triangle other)
        {
            return (a == other.a && (b == other.c || c == other.b)) || (a == other.b && (b == other.a || c == other.c)) || (a == other.c && (b == other.b || c == other.a))
                || (b == other.a && (c == other.c || a == other.b)) || (b == other.b && (c == other.a || a == other.c)) || (b == other.c && (c == other.b || a == other.a))
                || (c == other.a && (a == other.c || b == other.b)) || (c == other.b && (a == other.a || b == other.c)) || (c == other.c && (a == other.b || b == other.a));

        }

        public void DecrementIndices(int after)
        {
            if (a > after)
            {
                a--;
            }
            if (b > after)
            {
                b--;
            }
            if (c > after)
            {
                c--;
            }
        }

    }

}