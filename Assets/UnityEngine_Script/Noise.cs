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
    /// <param name="mapWidth">surface</param>
    /// <param name="mapHeight"></param>
    /// <param name="seed">random generation (see valheim)</param>
    /// <param name="scale"></param>
    /// <param name="octaves">number of layers of noise map</param>
    /// <param name="persistance">control amplitude decrease(0f -> 1f): 0.35 will make "moutains" smaller; a high value increase the influence of small features</param>
    /// <param name="lacunarity">frequency: lacunarity calcul is lacunarity^octaves[]; a high value increases the amount if small features</param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, float2 offset)
    {
        //Stopwatch stopWatch = new Stopwatch(); //Test Performance
        //stopWatch.Start();

        float[,] noiseMap = new float[mapWidth, mapHeight];
        #region Random Seed Generation
        if (seed == 0) { seed = 1; }
        //Careful if cNoise (function perlinNoise) parameter are too high, it may return the same number every time
        // max monobehavior(-100000, 100000) : (test higher with ecs)
        //System.Random prng = new System.Random(seed);

        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)seed);
        float2[] octaveOffsets = new float2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            /*
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            */
            //Careful the bigger the number in Next(parameter) the uglier the generation is
            //Since those value
            float offsetX = prng.NextInt(-100000, 100000) + offset.x;
            float offsetY = prng.NextInt(-100000, 100000) + offset.y;
            octaveOffsets[i] = new float2(offsetX, offsetY);
        }
        #endregion Random Seed Generation

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        //default value for maxNoiseHeight
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //Make the noise scale changes happen to the center of the plane rather than the top left corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // h3 w1 w2 w3 w4 w5
        // h2 w1 w2 w3 w4 w5
        // h1 w1 w2 w3 w4 w5
        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    //float sampleX = ( (x-halfWidth) / math.mul(scale, frequency) ) + octaveOffsets[i].x;
                    //float sampleY = ( (y-halfHeight) / math.mul(scale, frequency) ) + octaveOffsets[i].y;

                    float sampleX = math.mul((x - halfWidth) / scale, frequency) + octaveOffsets[i].x;
                    float sampleY = math.mul((y - halfHeight) / scale, frequency) + octaveOffsets[i].y;

                    float2 sampleXY = new float2(sampleX, sampleY);

                    float perlinValue = math.mul(cnoise(sampleXY), 2) - 1; //float2 ok here since we calculate a value and do not store them;
                    //float perlinValue = math.mul(snoise(sampleXY), 2) - 1; //float2 ok here since we calculate a value and do not store them;
                    noiseHeight += math.mul(perlinValue, amplitude);

                    amplitude = math.mul(amplitude, persistance); //persistance range (0 - > 1) and decrease each octave so the amplitude
                    frequency = math.mul(frequency, lacunarity); // frequence increase each octave
                }

                //Define the min and max value for the noise after calculation
                //then make a check before return value (See Below: iterations before return)
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }
        //Normalize noiseMap[x,y] between minNoiseMap and maxNoiseMap
        //InverseLerp returns a percent(%) between two samples given a point(t) (param x,y,t : from x -> y how much t represente as percent(p))
        //Lerp returns a point(t) given two samples and a percent(%) (param x,y,p : given % p gives value(t) between x - > y).
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = math.unlerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]); //unlerp = InverseLerp so we want a %
            }
        }

        //stopWatch.Stop();
        //UnityEngine.Debug.Log($"Noise Calculation : {stopWatch.ElapsedMilliseconds} ms");
        return noiseMap;
    }
}
