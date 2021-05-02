using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using static Unity.Mathematics.noise;

public static class NoiseMono
{
    public static float[] NoiseMapGen(int mapWidth, int mapHeight, float scale)
    {
        float[] noiseMap = new float[mapWidth * mapHeight];

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;
                float2 sampleXY = new float2(sampleX, sampleY);

                float pNoiseValue = cnoise(sampleXY);
                noiseMap[math.mad(y, mapWidth, x)] = pNoiseValue; // to find index of a 2D array in a 1D array (y*width)+1
            }
        }
        return noiseMap;
    }
}
