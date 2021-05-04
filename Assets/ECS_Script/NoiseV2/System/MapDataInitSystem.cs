using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Diagnostics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class MapDataInitSystem : SystemBase
{
    EntityManager _em;
    MapSettingsTag settingData;
    Entity MapSettingsEntity;
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

        _em.SetComponentData(MapSettingsEntity, new width { value = mapWidth });
        _em.SetComponentData(MapSettingsEntity, new height { value = mapHeight });
        _em.SetComponentData(MapSettingsEntity, new seed { value = seed });
        _em.SetComponentData(MapSettingsEntity, new scale { value = scale });
        _em.SetComponentData(MapSettingsEntity, new octavesData { value = octaves });
        _em.SetComponentData(MapSettingsEntity, new persistanceData { value = settingData.persistance });
        _em.SetComponentData(MapSettingsEntity, new lacunarityData { value = lacunarity });
        _em.SetComponentData(MapSettingsEntity, new offsetData { value = settingData.offset });
        _em.SetComponentData(MapSettingsEntity, new drawModeData { value = settingData.drawMode });
        _em.SetComponentData(MapSettingsEntity, new RendererData { value = _em.GetComponentData<DataRenderer>(MapSettingsEntity).renderer });

        float[] nMap = NoiseMono.NoiseMapGen(settingData.width, settingData.height, settingData.seed, settingData.scale, settingData.octaves, settingData.persistance, settingData.lacunarity, settingData.offset);
        int noiseMapSurface = math.mul(settingData.width, settingData.height);
        DynamicBuffer<noiseMapBuffer> nmBuffer = GetBuffer<noiseMapBuffer>(MapSettingsEntity);
        nmBuffer.Capacity = noiseMapSurface;
        for (int i = 0; i < nMap.Length; i++)
        {
            noiseMapBuffer nMapElement = new noiseMapBuffer();
            nMapElement.value = nMap[i];
            nmBuffer.Add(nMapElement);
        }
        //UnityEngine.Debug.Log($"REMOVE");
        _em.RemoveComponent<MapSettingsTag>(MapSettingsEntity);
        _em.RemoveComponent<Event_MapGen_AddSetData>(GetSingletonEntity<Event_MapGenTag>());
        //sw.Stop();
        //UnityEngine.Debug.Log($"Elapsed = {sw.Elapsed}");
    }
}

//TO DO : ADD PERLIN NOISE CALCUL job or job.witcode()
