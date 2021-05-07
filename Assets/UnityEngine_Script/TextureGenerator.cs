using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        //first take length of the grid (width and height)
        int width = heightMap.GetLength(0); // 0 = array[0]
        int height = heightMap.GetLength(1); // 1 = array[1]
        //====================================
        //COLOR IS DEFINED HERE
        //====================================
        Color[] colourMap = new Color[math.mul(width,height)];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[math.mad(y, width, x)] = Color.Lerp(Color.black, Color.white, heightMap[x, y]); // y * width gives row we currently in (in Color[]) and + x gives the column
            }
        }
        //====================================
        return TextureFromColourMap(colourMap, width, height);
    }
}
