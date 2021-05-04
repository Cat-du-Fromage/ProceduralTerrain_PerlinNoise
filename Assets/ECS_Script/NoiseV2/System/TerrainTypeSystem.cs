using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(MapDataInitSystem))]
public class TerrainTypeSystem : SystemBase
{
    NativeArray<float> regionsHeight;
    NativeArray<Color> regionsColor;
    Entity terrain;
    EntityManager _em;

    //Stopwatch sw;
    protected override void OnCreate()
    {
        var queryDescription = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Event_MapGen_RegionsData>() },
            None = new ComponentType[] {ComponentType.ReadOnly<Event_MapGen_AddSetData>() },
        };
        RequireForUpdate(GetEntityQuery(queryDescription));
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnStartRunning()
    {
        //sw = Stopwatch.StartNew();
        //sw.Start();
        terrain = GetSingletonEntity<NoiseMapData>();
    }

    protected override void OnUpdate()
    {
        RegionsData regionHeight = GetComponent<RegionsData>(terrain);
        RegionsColor regionColor = GetComponent<RegionsColor>(terrain);
        float[] regionHArray = { regionHeight.Ocean, regionHeight.Coast, regionHeight.Sand, regionHeight.Plain, regionHeight.Forest, regionHeight.Tundra, regionHeight.Mountain, regionHeight.Snow };
        Color[] regionCArray = { regionColor.OceanColor, regionColor.CoastColor, regionColor.SandColor, regionColor.PlainColor, regionColor.ForestColor, regionColor.TundraColor, regionColor.MountainColor, regionColor.SnowColor};

        regionsHeight = new NativeArray<float>(regionHArray.Length, Allocator.Persistent);
        regionsColor = new NativeArray<Color>(regionCArray.Length, Allocator.Persistent);

        regionsHeight.CopyFrom(regionHArray);
        regionsColor.CopyFrom(regionCArray);
        DynamicBuffer<TerrainTypeBuffer> terrainBuffer = GetBuffer<TerrainTypeBuffer>(terrain);
        TerrainTypeJob terrainTypeJob = new TerrainTypeJob
        {
            regionHeightJob = regionsHeight,
            regionColorJob = regionsColor,
            RegionsDataBuffer = terrainBuffer,
        };
        JobHandle jobHandle = terrainTypeJob.Schedule();
        jobHandle.Complete();

        regionsHeight.Dispose();
        regionsColor.Dispose();

        #region Event Trigger End
        _em.RemoveComponent<RegionsData>(terrain);
        _em.RemoveComponent<RegionsColor>(terrain);
        _em.RemoveComponent<Event_MapGen_RegionsData>(GetSingletonEntity<Event_MapGenTag>());
        #endregion Event Trigger End
        //sw.Stop();
        //Debug.Log($"Elapsed TerrainTypeSystem = {sw.Elapsed}");
    }

    [BurstCompile]
    public struct TerrainTypeJob : IJob
    {
        public NativeArray<float> regionHeightJob;
        public NativeArray<Color> regionColorJob;
        public DynamicBuffer<TerrainTypeBuffer> RegionsDataBuffer;
        public void Execute()
        {
            for (int i = 0; i < regionHeightJob.Length; i++)
            {
                TerrainTypeBuffer terrainData = new TerrainTypeBuffer();
                terrainData.height = regionHeightJob[i];
                terrainData.colour = regionColorJob[i];
                RegionsDataBuffer.Add(terrainData);
            }
        }
    }

    protected override void OnStopRunning()
    {
        if (regionsHeight.IsCreated)
        {
            regionsHeight.Dispose();
        }
        if (regionsColor.IsCreated)
        {
            regionsColor.Dispose();
        }
    }

    protected override void OnDestroy()
    {
        if (regionsHeight.IsCreated)
        {
            regionsHeight.Dispose();
        }
        if (regionsColor.IsCreated)
        {
            regionsColor.Dispose();
        }
    }
}
