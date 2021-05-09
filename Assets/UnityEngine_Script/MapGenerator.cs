using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };
    public DrawMode drawMode;

    public const int mapChunkSize = 241; // must be a factor of width -1 => in our case width - 1 = 240

    [Range(0, 6)]
    public int levelOfDetail;
    //public int mapWidth;
    //public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public float2 offset;

    public float meshHeightMultiplier; //1 (voir MeshGenerator)
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        { //1 (voir MeshGenerator)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }
    #region REQUEST THREAD
    public void RequestMapData(Action<MapData> callBack)
    {
        //Entities.Foreach equivalent? delegate is a lambda
        ThreadStart threadStart = delegate
        {
            MapDataThread(callBack);//start "event"
        };

        new Thread(threadStart).Start();
    }

    /// <summary>
    /// NoiseHeight AND ColorMAP are needed
    /// </summary>
    /// <param name="callBack"></param>
    void MapDataThread(Action<MapData> callBack)
    {
        MapData mapData = GenerateMapData(); // get noiseMap and ColorMap
        lock (mapDataThreadInfoQueue) // equivalent of dependency?
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate 
        {
            MeshDataThread(mapData, callBack);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callBack)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }
    }
    #endregion REQUEST THREAD
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        //Apply color depending of the map's height value and value height's value assign to each regions
        Color[] colourMap = new Color[math.mul(mapChunkSize, mapChunkSize)];
        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colourMap[math.mad(y, mapChunkSize, x)] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1) { lacunarity = 1;}
        if (octaves < 0) { octaves = 0;}
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
