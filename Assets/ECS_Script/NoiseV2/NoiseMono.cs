using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using static Unity.Mathematics.noise;

public static class NoiseMono
{
    public static float[] NoiseMapGen(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float2 offset)
    {
        float[] noiseMap = new float[mapWidth * mapHeight];
        #region Check Values
        scale = scale <= 0 ? 0.0001f : scale;
        /*
        mapWidth = mapWidth < 1 ? 1 : mapWidth;
        mapHeight = mapHeight < 1 ? 1 : mapHeight;
        lacunarity = lacunarity < 1f ? 1f : mapHeight;
        seed = seed < 0 ? 1 : seed;
        */
        #endregion Check Values
        #region Random Seed Generation
        if (seed == 0) seed = 1;
        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)seed);
        float2[] octaveOffsets = new float2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            //Careful the bigger the number in Next(parameter) the uglier the generation is
            //Since those value
            float offsetX = prng.NextUInt(0, 100000) + offset.x;
            float offsetY = prng.NextUInt(0, 100000) + offset.y;
            octaveOffsets[i] = new float2(offsetX, offsetY);
        }
        //UnityEngine.Debug.Log($"seed generation: {seed}");
        #endregion Random Seed Generation

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //Make the noise scale changes happen to the center of the plane rather than the top left corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                //Octaves application
                for(int i = 0; i<octaves;i++)
                {
                    //Perlin noise calculation (maybe good to check how it works one day..)
                    float sampleX = ((x - halfWidth) / math.mul(scale, frequency)) + octaveOffsets[i].x;
                    float sampleY = ((y - halfHeight) / math.mul(scale, frequency)) + octaveOffsets[i].y;
                    float2 sampleXY = new float2(sampleX, sampleY);

                    float pNoiseValue = cnoise(sampleXY);
                    noiseHeight = math.mad(pNoiseValue, amplitude, noiseHeight);

                    //amplitude : decrease each octaves; frequency : increase each octaves
                    amplitude = math.mul(amplitude, persistance);
                    frequency = math.mul(frequency, lacunarity);
                }
                //First we check max and min Height for the terrain
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                //then we apply thoses value to the terrain
                noiseMap[math.mad(y, mapWidth, x)] = noiseHeight; // to find index of a 2D array in a 1D array (y*width)+1
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[math.mad(y, mapWidth, x)] = math.unlerp(minNoiseHeight, maxNoiseHeight, noiseMap[math.mad(y, mapWidth, x)]); //unlerp = InverseLerp so we want a %
            }
        }
        return noiseMap;
    }
}
