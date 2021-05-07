using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0)?1:math.mul(levelOfDetail,2);// 2
        int verticesPerLine = ( (width - 1) / meshSimplificationIncrement ) + 1; // 2
        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine); // 2
        int vertexIndex = 0;

        for(int y = 0; y < height; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {//1 (voir MeshGenerator)
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, math.mul(heightCurve.Evaluate(heightMap[x, y]), heightMultiplier), topLeftZ - y); //we search for the top left vertice of the square
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if(x < width-1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex ,vertexIndex+verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[math.mul(meshWidth, meshHeight)];
        uvs = new Vector2[math.mul(meshWidth, meshHeight)];
        triangles = new int[math.mul(math.mul((meshWidth-1), (meshHeight-1)), 6)];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
