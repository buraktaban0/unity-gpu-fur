using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using MeshLabInternal;
using UnityEditor;

namespace MeshLabCore
{

    public static class MeshLab
    {

        private static List<Triangle> triangles;
        public static List<int> availableTriangles;
        public static int pieces;
        public static List<Vector3> vertices;
        public static List<Vector3> normals;
        public static List<Vector4> tangents;
        public static List<Vector2> uvs;


        /*public static Mesh Simplify(Mesh meshToSimplify, float reduceToPercent, bool isTwoSided)
        {
            reduceToPercent = Mathf.Clamp(reduceToPercent, 1f, 100f);

            if (reduceToPercent > 99f)
            {
                Debug.LogError("MeshLab - Called Simplify with no significant reduction percent!");
                return DuplicateMesh(meshToSimplify);
            }

            MeshData data = new MeshData(meshToSimplify);
            MeshData dataFinal = new MeshData(meshToSimplify);

            if (isTwoSided)
            {
                data.ConvertToOneSided();
            }


            int triCount = data.tri.Count;
            int targetTriCount = (int)(triCount * reduceToPercent * 0.01f);
            int trisToRemove = triCount - targetTriCount;

            if (trisToRemove <= 0)
            {
                return DuplicateMesh(meshToSimplify);
            }

            while (trisToRemove > 0)
            {
                List<int> availableIndices = new List<int>();
                for (int i = 0; i < dataFinal.tri.Count; i++)
                {
                    availableIndices.Add(i);
                }
                Utility.Shuffle(availableIndices);

                while (availableIndices.Count > 0 && trisToRemove > 0)
                {
                    int index1 = availableIndices[0];
                    availableIndices.RemoveAt(0);
                    trisToRemove--;

                }

            }
        }*/
        
        public static Mesh DuplicateMesh(Mesh mesh)
        {
            Mesh m = new Mesh();
            m.vertices = mesh.vertices;
            m.normals = mesh.normals;
            m.tangents = mesh.tangents;
            m.uv = mesh.uv;
            m.triangles = mesh.triangles;
            return m;
        }
        public static Mesh SubdivideMesh(Mesh original, int subdivisions)
        {
            Mesh mesh = DuplicateMesh(original);
            Debug.Log(mesh.triangles.Length / 3);
            for (int s = 0; s < subdivisions; s++)
            {
                List<Vector3> vertices = new List<Vector3>(mesh.vertices);
                List<Vector3> normals = new List<Vector3>(mesh.normals);
                List<Vector4> tangents = new List<Vector4>(mesh.tangents);
                List<Vector2> uvs = new List<Vector2>(mesh.uv);
                List<Triangle> triangles = Utility.GetTriangles(mesh.triangles, mesh.vertices);
                List<Triangle> finalTris = new List<Triangle>(triangles);
                int triangleCount = triangles.Count;

                Triangle t1;
                int a, b, c, d, e, f, index;
                for (int i = 0; i < triangleCount; i++)
                {
                    finalTris.RemoveAt(i);
                    t1 = triangles[i];
                    a = t1.a;
                    b = t1.b;
                    c = t1.c;

                    d = vertices.Count;
                    vertices.Add((vertices[a] + vertices[b]) / 2f);
                    normals.Add((normals[a] + normals[b]) / 2f);
                    tangents.Add((tangents[a] + tangents[b]) / 2f);
                    uvs.Add((uvs[a] + uvs[b]) / 2f);
                    e = vertices.Count;
                    vertices.Add((vertices[b] + vertices[c]) / 2f);
                    normals.Add((normals[b] + normals[c]) / 2f);
                    tangents.Add((tangents[b] + tangents[c]) / 2f);
                    uvs.Add((uvs[b] + uvs[c]) / 2f);
                    f = vertices.Count;
                    vertices.Add((vertices[a] + vertices[c]) / 2f);
                    normals.Add((normals[a] + normals[c]) / 2f);
                    tangents.Add((tangents[a] + tangents[c]) / 2f);
                    uvs.Add((uvs[a] + uvs[c]) / 2f);

                    finalTris.Add(new Triangle(a, d, f, finalTris.Count));
                    finalTris.Add(new Triangle(b, e, d, finalTris.Count));
                    finalTris.Add(new Triangle(c, f, e, finalTris.Count));
                    finalTris.Add(new Triangle(d, e, f, finalTris.Count));
                }

                mesh.Clear();
                mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.tangents = tangents.ToArray();
                mesh.uv = uvs.ToArray();
                mesh.triangles = Utility.GetTrianglesArray(finalTris);
            }
            return mesh;
        }







    }

}