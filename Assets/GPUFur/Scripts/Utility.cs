using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshLabInternal
{

    public static class Utility
    {
        public static List<int> GetRangeList(int x, bool shuffle = false)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < x; i++)
            {
                list.Add(i);
            }

            if (shuffle)
            {
                Shuffle(list);
            }

            return list;
        }


        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            System.Random rnd = new System.Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int[] GetTrianglesArray(List<Triangle> tris)
        {
            int[] triangles = new int[tris.Count * 3];
            for (int i = 0; i < tris.Count; i++)
            {
                triangles[i * 3] = tris[i].a;
                triangles[i * 3 + 1] = tris[i].b;
                triangles[i * 3 + 2] = tris[i].c;
            }

            return triangles;
        }

        public static List<Triangle> GetTriangles(int[] tris, Vector3[] vertices)
        {
            List<Triangle> triangles = new List<Triangle>();
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < tris.Length / 3; i++)
            {
                Triangle t = new Triangle(tris[i * 3], tris[i * 3 + 1], tris[i * 3 + 2], i);
                t.GenerateEdges(edges);
                triangles.Add(t);
            }

            Triangle t1, t2;
            for (int i = 0; i < triangles.Count; i++)
            {
                t1 = triangles[i];

                for (int j = i + 1; j < triangles.Count; j++)
                {
                    t2 = triangles[j];
                    if (t1.IsNeighborOf(t2, vertices))
                    {
                        t1.AddNeighbor(t2);
                        t2.AddNeighbor(t1);
                    }
                }
            }


            return triangles;
        }


        public static void DecrementIndices(List<Triangle> tris, int after)
        {
            foreach (Triangle t in tris)
            {
                t.DecrementIndices(after);
            }
        }

    }


    public class Vertex
    {
        List<int> indices;

        public Vertex(params int[] indices)
        {
            this.indices = new List<int>(indices);
        }



    }

    public class Edge
    {
        public int i1, i2;

        public Edge(int i1, int i2)
        {
            this.i1 = i1;
            this.i2 = i2;
        }

        public bool IsSame(Edge other)
        {
            if ((i1 == other.i1 && i2 == other.i2) || (i1 == other.i2 && i2 == other.i1))
            {
                return true;
            }

            return false;
        }
    }


    
}
