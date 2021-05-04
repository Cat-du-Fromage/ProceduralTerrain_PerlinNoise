using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Diagnostics;
/*
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class MapSettingSystem : SystemBase
{
    EntityManager _em;
    MapSettingsTag settingData;

    NativeArray<float> nMapNativeArray;
    NativeArray<Color> colourMapNativeArray;
    NativeArray<TerrainTypeData> terrains;
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<MapSettingsTag>();
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        Entity MapSettingsEntity = GetSingletonEntity<MapSettingsTag>();
        settingData = _em.GetComponentData<MapSettingsTag>(MapSettingsEntity);

        _em.AddComponent<NoiseMapData>(MapSettingsEntity);

        _em.AddComponent<width>(MapSettingsEntity);
        _em.AddComponent<height>(MapSettingsEntity);
        _em.AddComponent<scale>(MapSettingsEntity); //small = lot of small feature, big = biger features
        _em.AddComponent<seed>(MapSettingsEntity);
        _em.AddBuffer<noiseMapBuffer>(MapSettingsEntity);
        _em.AddComponent<textureData>(MapSettingsEntity);
        _em.AddComponent<RendererData>(MapSettingsEntity);
        _em.AddComponent<octavesData>(MapSettingsEntity);
        _em.AddComponent<persistanceData>(MapSettingsEntity);
        _em.AddComponent<lacunarityData>(MapSettingsEntity);
        _em.AddComponent<offsetData>(MapSettingsEntity);

        _em.AddBuffer<TerrainTypeBuffer>(MapSettingsEntity);
        //_em.AddComponent<colourArrayData>(MapSettingsEntity);

        #region Check Values
        float scale = settingData.scale <= 0 ? 0.0001f : settingData.scale;
        int mapWidth = settingData.width < 1 ? 1 : settingData.width;
        int mapHeight = settingData.height < 1 ? 1 : settingData.height;
        float lacunarity = settingData.lacunarity < 1f ? 1f : settingData.lacunarity;
        int seed = settingData.seed < 0 ? 1 : settingData.seed;
        int octaves = settingData.octaves < 0 ? 1 : settingData.octaves;
        #endregion Check Values

        _em.SetComponentData(MapSettingsEntity, new width { value = mapWidth });
        _em.SetComponentData(MapSettingsEntity, new height { value = mapHeight });
        _em.SetComponentData(MapSettingsEntity, new seed { value = seed });
        _em.SetComponentData(MapSettingsEntity, new scale { value = scale });
        _em.SetComponentData(MapSettingsEntity, new octavesData { value = octaves });
        _em.SetComponentData(MapSettingsEntity, new persistanceData { value = settingData.persistance });
        _em.SetComponentData(MapSettingsEntity, new lacunarityData { value = lacunarity });
        _em.SetComponentData(MapSettingsEntity, new offsetData { value = settingData.offset });
        sw.Stop();
        UnityEngine.Debug.Log($"Elapsed = {sw.Elapsed}");// Elapsed = 00:00:00.0083416
        var widthData = GetComponent<width>(MapSettingsEntity).value;
        var heightData = GetComponent<height>(MapSettingsEntity).value;
        var seedData = GetComponent<seed>(MapSettingsEntity).value;
        var scaleData = GetComponent<scale>(MapSettingsEntity).value;
        var octavesData = GetComponent<octavesData>(MapSettingsEntity).value;
        var persistanceData = GetComponent<persistanceData>(MapSettingsEntity).value;
        var lacunarityData = GetComponent<lacunarityData>(MapSettingsEntity).value;
        var offsetData = GetComponent<offsetData>(MapSettingsEntity).value;

        //=====================================================================================================
        //TerrainType DynamicBuffer
        
        //DynamicBuffer<TerrainTypeBuffer> terrainData = GetBuffer<TerrainTypeBuffer>(MapSettingsEntity);
        //nMapNativeArray = new NativeArray<float>(8, Allocator.TempJob); //8 for now
        //colourMapNativeArray = new NativeArray<Color>(8, Allocator.TempJob); //8 for now
        //terrains = new NativeArray<TerrainTypeData>(8, Allocator.TempJob);

        //nMapNativeArray.Dispose();
        //colourMapNativeArray.Dispose();

        //RegionsData terrainHeight = GetComponent<RegionsData>(MapSettingsEntity);
        //RegionsColor terrainColor = GetComponent<RegionsColor>(MapSettingsEntity);
        
        //=====================================================================================================
        //NATIVE ARRAY constructor (Noise Map, Color Map)
        //=====================================================================================================
        int mapSurface = math.mul(widthData, heightData);
        nMapNativeArray = new NativeArray<float>(mapSurface, Allocator.TempJob);
        colourMapNativeArray = new NativeArray<Color>(mapSurface, Allocator.TempJob);
        //=====================================================================================================
        // NOISE MAP VALUE
        //=====================================================================================================
        float[] nMap = NoiseMono.NoiseMapGen(widthData, heightData, seedData, scaleData, octavesData, persistanceData, lacunarityData, offsetData);

        DynamicBuffer<noiseMapBuffer> nmBuffer = GetBuffer<noiseMapBuffer>(MapSettingsEntity);
        for (int i = 0; i < nMap.Length; i++)
        {
            noiseMapBuffer nMapElement = new noiseMapBuffer();
            nMapElement.value = nMap[i];
            nmBuffer.Add(nMapElement);
            nMapNativeArray[i] = nMap[i];
        }
        //=====================================================================================================

        //=====================================================================================================
        // COLOR MAP VALUE
        //Storing the Data causes massiv lag, maybe better with dynamicBuffer
        //=====================================================================================================
        Color[] colorsMapArray = new Color[mapSurface];
        ColourMapJob colourMapJob = new ColourMapJob
        {
            mWidth = widthData,
            mHeight = heightData,
            noiseMapJob = nMapNativeArray,
            colorsJob = colourMapNativeArray,
        };
        JobHandle jobHandle = colourMapJob.Schedule();
        jobHandle.Complete();
        colourMapNativeArray.CopyTo(colorsMapArray); //copy the value from NativeArray<T> to a simple Array[]
        //_em.SetComponentData(MapSettingsEntity, new colourArrayData { value = colorsMapArray });
        //=====================================================================================================

        //=====================================================================================================
        // TEXTURE2D
        //=====================================================================================================
        Texture2D texture2D = new Texture2D(widthData, heightData);
        texture2D.SetPixels(colorsMapArray);
        texture2D.Apply();
        _em.SetComponentData(MapSettingsEntity, new textureData { value = texture2D });
        //=====================================================================================================

        //=====================================================================================================
        //DRAWMODE
        //=====================================================================================================
        Renderer textureRender = _em.GetComponentData<DataRenderer>(MapSettingsEntity).renderer;
        switch (settingData.drawMode)
        {
            case 0://RENDERER (Black and white texture)
                textureRender.sharedMaterial.mainTexture = texture2D;
                textureRender.transform.localScale = new float3(widthData, 1, heightData);
                _em.SetComponentData(MapSettingsEntity, new RendererData { value = textureRender });
                break;

            default://Default is renderer(case 0:)
                textureRender.sharedMaterial.mainTexture = texture2D;
                textureRender.transform.localScale = new float3(widthData, 1, heightData);
                _em.SetComponentData(MapSettingsEntity, new RendererData { value = textureRender });
                break;
        }
        //=====================================================================================================
        //still needed in case the program continue to run for some reasons
        nMapNativeArray.Dispose();
        colourMapNativeArray.Dispose();

        _em.RemoveComponent<DataRenderer>(MapSettingsEntity);
        _em.RemoveComponent<MapSettingsTag>(MapSettingsEntity);
    }

    protected override void OnDestroy()
    {
        if(nMapNativeArray.IsCreated)
        {
            nMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }

    protected override void OnStopRunning()
    {
        if (nMapNativeArray.IsCreated)
        {
            nMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }
}



/// <summary>
/// Map Colour calculation
/// </summary>
[BurstCompile]
public struct ColourMapJob : IJob
{
    public NativeArray<Color> colorsJob;
    public NativeArray<float> noiseMapJob;
    public int mWidth;
    public int mHeight;
    public void Execute()
    {
        //colors. = new Color[math.mul(mWidth, mHeight)];
        for (int y = 0; y < mWidth; y++)
        {
            for (int x = 0; x < mHeight; x++)
            {
                colorsJob[math.mad(y, mWidth, x)] = Color.Lerp(Color.black, Color.white, noiseMapJob[math.mad(y, mWidth, x)]);
            }
        }
    }
}

public struct TerrainsData : IJob
{
    public NativeArray<float> terrainHeightData;
    public NativeArray<Color> terrainColorData;
    public NativeArray<TerrainTypeData> terrainTypeArray;
    public RegionsData regHeight;
    public RegionsColor regColor;

    public void Execute()
    {
        for(int i = 0; i < terrainHeightData.Length; i++)
        {
            TerrainTypeData terrain = new TerrainTypeData();
            switch (i)
            {
                case 0:
                    terrain.name = "Ocean";
                    terrain.height = regHeight.Ocean;
                    terrain.colour = regColor.OceanColor;
                    terrainTypeArray[i] = terrain;
                    break;
                case 1:
                    terrain.name = "Coast";
                    terrain.height = regHeight.Coast;
                    terrain.colour = regColor.CoastColor;
                    terrainTypeArray[i] = terrain;
                    break;
                
            }
        }
    }
}

*/