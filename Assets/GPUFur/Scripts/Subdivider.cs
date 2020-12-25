using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Subdivider
{

    public static Mesh Subdivide(Mesh mesh, int subdivisions)
    {
        Mesh finalMesh = new Mesh();
        while (subdivisions > 0)
        {
            List<Triangle> tris = new List<Triangle>();
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                tris.Add(new Triangle(triangles[i * 3], triangles[i * 3 + 1], triangles[i * 3 + 2]));
            }

        }

        return finalMesh;
    }




    private class Triangle
    {
        public int a, b, c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

}
