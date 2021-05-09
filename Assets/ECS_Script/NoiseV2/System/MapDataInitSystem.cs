using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Diagnostics;
using UnityEngine;
using Unity.Rendering;
using static Unity.Mathematics.noise;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class MapDataInitSystem : SystemBase
{
    EntityManager _em;
    MapSettingsTag settingData;
    Entity MapSettingsEntity;

    NativeArray<float> nMap;
    NativeArray<float2> octOffset;
    //Stopwatch sw;
    protected override void OnCreate()
    {
        //TO DO EVENT TerrainInitEvent holding all component and removing them one by one foreach system completed(use querydesc for sstem update)
        RequireForUpdate(GetEntityQuery(typeof(Event_MapGen_AddSetData)));
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnStartRunning()
    {
        //sw = Stopwatch.StartNew();
        //sw.Start();
        MapSettingsEntity = GetSingletonEntity<MapSettingsTag>();
        settingData = _em.GetComponentData<MapSettingsTag>(MapSettingsEntity);

        #region Check Values
        float mapScale = settingData.scale <= 0 ? 0.0001f : settingData.scale;
        //int mapWidth = settingData.width < 1 ? 1 : settingData.width;
        //int mapHeight = settingData.height < 1 ? 1 : settingData.height;
        float lacunarity = settingData.lacunarity < 1f ? 1f : settingData.lacunarity;
        int mapSeed = settingData.seed < 0 ? 1 : settingData.seed;
        int octaves = settingData.octaves < 0 ? 1 : settingData.octaves;
        #endregion Check Values

        //NEED TO CONVERT ALL THIS SHIT TO A BLOB ASSET!
        _em.AddComponent<NoiseMapData>(MapSettingsEntity);
        _em.AddComponentData(MapSettingsEntity, new mapChunkSizeData {value = 241});
        _em.AddComponentData(MapSettingsEntity, new mapHeightMultiplierData { value = settingData.heightMultiplier });
        _em.AddComponentData(MapSettingsEntity, new width {value = _em.GetComponentData<mapChunkSizeData>(MapSettingsEntity).value });
        _em.AddComponentData(MapSettingsEntity, new height { value = _em.GetComponentData<mapChunkSizeData>(MapSettingsEntity).value });
        _em.AddComponentData(MapSettingsEntity, new scale { value = mapScale}); //small = lot of small feature, big = biger features
        _em.AddComponentData(MapSettingsEntity, new seed { value = mapSeed });
        //_em.AddComponent<textureData>(MapSettingsEntity); // THE FUCK IS THIS?!
        _em.AddComponentData(MapSettingsEntity, new octavesData { value = octaves });
        _em.AddComponentData(MapSettingsEntity, new persistanceData { value = settingData.persistance });
        _em.AddComponentData(MapSettingsEntity, new lacunarityData { value = lacunarity });
        _em.AddComponentData(MapSettingsEntity, new offsetData { value = settingData.offset });
        _em.AddComponentData(MapSettingsEntity, new drawModeData { value = settingData.drawMode });
        _em.AddComponentData(MapSettingsEntity, new levelOfDetailData { value = settingData.levelOfDetail });
        _em.AddBuffer<noiseMapBuffer>(MapSettingsEntity);
        _em.AddBuffer<TerrainTypeBuffer>(MapSettingsEntity);
        /*
        _em.AddComponentData(MapSettingsEntity, new RendererData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).renderer });
        _em.AddComponentData(MapSettingsEntity, new MeshFilterData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).meshFilter });
        _em.AddComponentData(MapSettingsEntity, new MeshRendererData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).meshRenderer });
        */
        _em.RemoveComponent<DataRenderer>(MapSettingsEntity);
    }

    protected override void OnUpdate()
    {
        int mapWidth = GetComponent<width>(MapSettingsEntity).value;
        int mapHeight = GetComponent<height>(MapSettingsEntity).value;
        int octaves = GetComponent<octavesData>(MapSettingsEntity).value;
        int mapSeed = GetComponent<seed>(MapSettingsEntity).value;
        float mapScale = GetComponent<scale>(MapSettingsEntity).value;
        float lacunarity = GetComponent<lacunarityData>(MapSettingsEntity).value;
        
        if (settingData.drawMode == 0 || settingData.drawMode == 1)
        {
            _em.AddSharedComponentData(MapSettingsEntity, new RenderMesh { material = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).MatMap, mesh = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).mesh });
        }
        else if(settingData.drawMode == 2)
        {
            _em.AddSharedComponentData(MapSettingsEntity, new RenderMesh { material = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).MeshMat, mesh = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).mesh });
        }
        
        int noiseMapSurface = math.mul(mapWidth, mapHeight);
        nMap = new NativeArray<float>(noiseMapSurface , Allocator.TempJob);
        octOffset = new NativeArray<float2>(octaves, Allocator.TempJob);
        //Job For Perlin Noise
        PerlinNoiseJob perlinNoiseJob = new PerlinNoiseJob
        {
            widthJob = mapWidth,
            heightJob = mapHeight,
            seedJob = mapSeed,
            scaleJob = mapScale,
            octavesJob = octaves,
            persistanceJob = settingData.persistance,
            lacunarityJob = lacunarity,
            offsetJob = settingData.offset,
            noiseMap = nMap,
            octOffsetArray = octOffset,
        };
        JobHandle jobHandle = perlinNoiseJob.Schedule();
        jobHandle.Complete();
        //octOffset.Dispose();
        DynamicBuffer<noiseMapBuffer> nmBuffer = GetBuffer<noiseMapBuffer>(MapSettingsEntity);
        for (int i = 0; i < nMap.Length; i++)
        {
            noiseMapBuffer nMapElement = new noiseMapBuffer();
            nMapElement.value = nMap[i];
            nmBuffer.Add(nMapElement);
        }
        nMap.Dispose();
        octOffset.Dispose();
        #region Event Trigger End
        //_em.RemoveComponent<DataRenderer>(MapSettingsEntity);
        _em.RemoveComponent<MapSettingsTag>(MapSettingsEntity);
        _em.RemoveComponent<Event_MapGen_AddSetData>(GetSingletonEntity<Event_MapGenTag>());
        #endregion Event Trigger End
        //sw.Stop();
        //UnityEngine.Debug.Log($"Init Elapsed = {sw.Elapsed}");
    }

    [BurstCompile]
    public struct PerlinNoiseJob : IJob
    {
        [ReadOnly] public int widthJob;
        [ReadOnly] public int heightJob;
        [ReadOnly] public int seedJob;
        [ReadOnly] public float scaleJob;
        [ReadOnly] public int octavesJob;
        [ReadOnly] public float persistanceJob;
        [ReadOnly] public float lacunarityJob;
        [ReadOnly] public float2 offsetJob;

        //returned Value
        public NativeArray<float> noiseMap;
        public NativeArray<float2> octOffsetArray;
        public void Execute()
        {
            #region Random
            //(offset(x,y) per octaves changes)
            Unity.Mathematics.Random pRNG = new Unity.Mathematics.Random((uint)seedJob);
            
            for (int i = 0; i < octOffsetArray.Length;  i++)
            {
                float offsetX = pRNG.NextInt(-100000, 100000) + offsetJob.x;
                float offsetY = pRNG.NextInt(-100000, 100000) + offsetJob.y;
                octOffsetArray[i] = new float2(offsetX, offsetY);
            }
            #endregion Random
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = widthJob / 2f;
            float halfHeight = heightJob / 2f;

            for(int y = 0; y < heightJob; y++)
            {
                for (int x = 0; x < widthJob; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for(int i = 0; i< octavesJob; i++)
                    {
                        //float sampleX = ((x - halfWidth) / math.mul(scaleJob, frequency)) + octOffsetArray[i].x;
                        //float sampleY = ((y - halfHeight) / math.mul(scaleJob, frequency)) + octOffsetArray[i].y;
                        float sampleX = math.mul((x - halfWidth) / scaleJob, frequency) + octOffsetArray[i].x;
                        float sampleY = math.mul((y - halfHeight) / scaleJob, frequency) + octOffsetArray[i].y;
                        float2 sampleXY = new float2(sampleX, sampleY);

                        float pNoiseValue = snoise(sampleXY);
                        noiseHeight = math.mad(pNoiseValue, amplitude, noiseHeight);
                        //amplitude : decrease each octaves; frequency : increase each octaves
                        amplitude = math.mul(amplitude, persistanceJob);
                        frequency = math.mul(frequency, lacunarityJob);
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
                    noiseMap[math.mad(y, widthJob, x)] = noiseHeight; // to find index of a 2D array in a 1D array (y*width)+1
                } 
            }
            for (int y = 0; y < heightJob; y++)
            {
                for (int x = 0; x < widthJob; x++)
                {
                    noiseMap[math.mad(y, widthJob, x)] = math.unlerp(minNoiseHeight, maxNoiseHeight, noiseMap[math.mad(y, widthJob, x)]); //unlerp = InverseLerp so we want a %
                }
            }
        }
    }
    
    protected override void OnDestroy()
    {
        if (nMap.IsCreated)
            nMap.Dispose();
        if (octOffset.IsCreated)
            octOffset.Dispose();
    }

    protected override void OnStopRunning()
    {
        if (nMap.IsCreated)
            nMap.Dispose();
        if (octOffset.IsCreated)
            octOffset.Dispose();
    }
    
}

//TO DO : ADD PERLIN NOISE CALCUL job or job.witcode()
