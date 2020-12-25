using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDNoise
{

    public static Texture3D Get3DNoise(int width, int height, int depth, Vector3 scale)
    {
        Vector3 offset = new Vector3(Random.Range(0f, 99999f), Random.Range(0f, 99999f), Random.Range(0f, 99999f));
        Texture3D tex = new Texture3D(width, height, depth, TextureFormat.ARGB32, false);
        Color c = new Color(1, 1, 1, 1);
        Color[] colors = new Color[width * height * depth];
        float xyR, xyG, xyB, yzR, yzG, yzB, xzR, xzG, xzB;
        float _x, _y, _z;
        //float aR = 0, aG = 0, aB = 0;
        for (int x = 0; x < width; x++)
        {
            _x = (float)x / width;
            for (int y = 0; y < height; y++)
            {
                _y = (float)y / height;
                xyR = Mathf.PerlinNoise(_x * scale.x + offset.x, _y * scale.y + offset.y);
                xyG = Mathf.PerlinNoise(_x * scale.x + offset.x + 0.33f, _y * scale.y + offset.y + 0.66f);
                xyB = Mathf.PerlinNoise(_x * scale.x + offset.x + 0.66f, _y * scale.y + offset.y + 0.33f);
                for (int z = 0; z < depth; z++)
                {
                    _z = (float)z / depth;
                    yzR = Mathf.PerlinNoise(_y * scale.y + offset.y, _z * scale.z + offset.z);
                    yzG = Mathf.PerlinNoise(_y * scale.y + offset.y + 0.33f, _z * scale.z + offset.z + 0.66f);
                    yzB = Mathf.PerlinNoise(_y * scale.y + offset.y + 0.66f, _z * scale.z + offset.z + 0.33f);

                    xzR = Mathf.PerlinNoise(_x * scale.x + offset.x, _z * scale.z + offset.z);
                    xzG = Mathf.PerlinNoise(_x * scale.x + offset.x + 0.33f, _z * scale.z + offset.z + 0.66f);
                    xzB = Mathf.PerlinNoise(_x * scale.x + offset.x + 0.66f, _z * scale.z + offset.z + 0.33f);


                    c.r = (xyR + yzR + xzR) * 0.33f;
                    c.g = (xyG + yzG + xzG) * 0.33f;
                    c.b = (xyB + yzB + xzB) * 0.33f;

                    /* c.r = Mathf.Pow(xyR * yzR * xzR, 0.33f);
                     c.g = Mathf.Pow(xyG * yzG * xzG, 0.33f);
                     c.b = Mathf.Pow(xyB * yzB * xzB, 0.33f);*/
                    /*c.r = (xyR + yzR + xzR) * 0.33f - 0.5f;
                    c.g = (xyG + yzG + xzG) * 0.33f - 0.5f;
                    c.b = (xyB + yzB + xzB) * 0.33f - 0.5f;*/

                    /*aR += c.r;
                    aG += c.g;
                    aB += c.b;*/

                    colors[width * height * z + width * y + x] = c;
                }
            }
        }
        tex.SetPixels(colors);
        //Debug.Log(aR / width / height / depth + "  " + aG / width / height / depth + "  " + aB / width / height / depth);
        /*Debug.Log(colors[width * height * 16 + width * 16 + 16]);
        Debug.Log(colors[width * height * 17 + width * 17 + 17]);
        Debug.Log(colors[width * height * 18 + width * 18 + 18]);*/
        tex.wrapMode = TextureWrapMode.Mirror;
        tex.Apply();
        return tex;
    }

}
