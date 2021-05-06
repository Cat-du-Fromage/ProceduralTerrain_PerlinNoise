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

        _em.AddComponent<NoiseMapData>(MapSettingsEntity);
        _em.AddComponent<width>(MapSettingsEntity);
        _em.AddComponent<height>(MapSettingsEntity);
        _em.AddComponent<scale>(MapSettingsEntity); //small = lot of small feature, big = biger features
        _em.AddComponent<seed>(MapSettingsEntity);
        _em.AddBuffer<noiseMapBuffer>(MapSettingsEntity);
        _em.AddComponent<textureData>(MapSettingsEntity);
        _em.AddComponent<RendererData>(MapSettingsEntity);
        _em.AddComponent<MeshFilterData>(MapSettingsEntity);
        _em.AddComponent<MeshRendererData>(MapSettingsEntity);
        _em.AddComponent<octavesData>(MapSettingsEntity);
        _em.AddComponent<persistanceData>(MapSettingsEntity);
        _em.AddComponent<lacunarityData>(MapSettingsEntity);
        _em.AddComponent<offsetData>(MapSettingsEntity);
        _em.AddComponent<drawModeData>(MapSettingsEntity);
        _em.AddBuffer<TerrainTypeBuffer>(MapSettingsEntity);

    }

    protected override void OnUpdate()
    {
        #region Check Values
        float scale = settingData.scale <= 0 ? 0.0001f : settingData.scale;
        int mapWidth = settingData.width < 1 ? 1 : settingData.width;
        int mapHeight = settingData.height < 1 ? 1 : settingData.height;
        float lacunarity = settingData.lacunarity < 1f ? 1f : settingData.lacunarity;
        int seed = settingData.seed < 0 ? 1 : settingData.seed;
        int octaves = settingData.octaves < 0 ? 1 : settingData.octaves;
        #endregion Check Values

        _em.SetComponentData(MapSettingsEntity, new RendererData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).renderer });
        _em.SetComponentData(MapSettingsEntity, new MeshFilterData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).meshFilter });
        _em.SetComponentData(MapSettingsEntity, new MeshRendererData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).meshRenderer });
        _em.RemoveComponent<DataRenderer>(MapSettingsEntity);

        _em.SetComponentData(MapSettingsEntity, new width { value = mapWidth });
        _em.SetComponentData(MapSettingsEntity, new height { value = mapHeight });
        _em.SetComponentData(MapSettingsEntity, new seed { value = seed });
        _em.SetComponentData(MapSettingsEntity, new scale { value = scale });
        _em.SetComponentData(MapSettingsEntity, new octavesData { value = octaves });
        _em.SetComponentData(MapSettingsEntity, new persistanceData { value = settingData.persistance });
        _em.SetComponentData(MapSettingsEntity, new lacunarityData { value = lacunarity });
        _em.SetComponentData(MapSettingsEntity, new offsetData { value = settingData.offset });
        _em.SetComponentData(MapSettingsEntity, new drawModeData { value = settingData.drawMode });

        //_em.SetSharedComponentData(MapSettingsEntity, new RenderMesh { mesh = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).mesh });
        if (settingData.drawMode == 0 || settingData.drawMode == 1)
        {
            _em.SetSharedComponentData(MapSettingsEntity, new RenderMesh { material = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).MatMap, mesh = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).mesh });
        }
        else if(settingData.drawMode == 2)
        {
            _em.SetSharedComponentData(MapSettingsEntity, new RenderMesh { material = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).MeshMat, mesh = _em.GetComponentData<MapMaterialData>(MapSettingsEntity).mesh });
        }

        int noiseMapSurface = math.mul(mapWidth, mapHeight);
        nMap = new NativeArray<float>(noiseMapSurface , Allocator.TempJob);
        octOffset = new NativeArray<float2>(octaves, Allocator.TempJob);
        //Job For Perlin Noise
        PerlinNoiseJob perlinNoiseJob = new PerlinNoiseJob
        {
            widthJob = mapWidth,
            heightJob = mapHeight,
            seedJob = seed,
            scaleJob = scale,
            octavesJob = octaves,
            persistanceJob = settingData.persistance,
            lacunarityJob = lacunarity,
            offsetJob = settingData.offset,
            noiseMap = nMap,
            octOffsetArray = octOffset,
        };
        JobHandle jobHandle = perlinNoiseJob.Schedule();
        jobHandle.Complete();
        octOffset.Dispose();
        DynamicBuffer<noiseMapBuffer> nmBuffer = GetBuffer<noiseMapBuffer>(MapSettingsEntity);
        for (int i = 0; i < nMap.Length; i++)
        {
            noiseMapBuffer nMapElement = new noiseMapBuffer();
            nMapElement.value = nMap[i];
            nmBuffer.Add(nMapElement);
        }
        nMap.Dispose();
        #region Event Trigger End
        _em.RemoveComponent<DataRenderer>(MapSettingsEntity);
        _em.RemoveComponent<MapSettingsTag>(MapSettingsEntity);
        _em.RemoveComponent<Event_MapGen_AddSetData>(GetSingletonEntity<Event_MapGenTag>());
        #endregion Event Trigger End
        //sw.Stop();
        //UnityEngine.Debug.Log($"Init Elapsed = {sw.Elapsed}");
    }

    [BurstCompile]
    public struct PerlinNoiseJob : IJob
    {
        public int widthJob;
        public int heightJob;
        public int seedJob;
        public float scaleJob;
        public int octavesJob;
        public float persistanceJob;
        public float lacunarityJob;
        public float2 offsetJob;

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
                float offsetX = pRNG.NextUInt(0, 100000) + offsetJob.x;
                float offsetY = pRNG.NextUInt(0, 100000) + offsetJob.y;
                octOffsetArray[i] = new float2(offsetX, offsetY);
            }
            #endregion Random
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = math.half(widthJob);
            float halfHeight = math.half(heightJob);

            for(int y = 0; y < heightJob; y++)
            {
                for (int x = 0; x < widthJob; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for(int i = 0; i< octavesJob; i++)
                    {
                        float sampleX = ((x - halfWidth) / math.mul(scaleJob, frequency)) + octOffsetArray[i].x;
                        float sampleY = ((y - halfHeight) / math.mul(scaleJob, frequency)) + octOffsetArray[i].y;
                        float2 sampleXY = new float2(sampleX, sampleY);

                        float pNoiseValue = cnoise(sampleXY);
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
