using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.noise;



public static class Noise
{
    /// <summary>
    /// CAREFUL float2(x,y) is not equal to float[,]
    /// float[x,y] is a 2d array than can store multiple data at a given index(x,y) where float2(x,y) is store ONLY 2 data(oone for x, one for y!
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        Stopwatch stopWatch = new Stopwatch(); //Test Performance
        stopWatch.Start();

        float[,] noiseMap = new float[mapWidth, mapHeight];

        if(scale <= 0)
        {
            scale = 0.0001f;
        }
        // h3 w1 w2 w3 w4 w5
        // h2 w1 w2 w3 w4 w5
        // h1 w1 w2 w3 w4 w5
        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                //float sampleX = x / scale;
                //float sampleY = y / scale;

                float2 sampleXY = new float2(x / scale, y / scale);

                float perlinValue = cnoise(sampleXY); //float2 ok here since we calculate a value and do not store them;
                noiseMap[x, y] = perlinValue;
            }
        }
        stopWatch.Stop();
        UnityEngine.Debug.Log($"Noise Calculation : {stopWatch.ElapsedMilliseconds} ms");
        return noiseMap;
    }
}
