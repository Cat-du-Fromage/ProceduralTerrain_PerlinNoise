using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

public class MapEndlessTerrain : SystemBase
{

    EntityManager _em;
    NativeHashMap<float2, Entity> Terrainchunks;
    NativeArray<float2> chunksPosition;

    Entity cameraEntity;
    float2 viewerPosition;

    int chunksVisibleInViewDst;
    public const float maxViewDistance = 450;
    int chunkSize; // Retrive from System Init Map

    //QUERY
    EntityQuery chunksDisplayedQuery;


    //Native ARRAY
    NativeArray<Entity> chunksDisplayed;
    NativeArray<Entity> chunksInView;

    //Terrain archetype
    EntityArchetype chunkArchetype;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<ChunksHolder_Tag>();
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnStartRunning()
    {
        chunksVisibleInViewDst = (int)math.round(maxViewDistance / chunkSize);
        chunkArchetype = _em.CreateArchetype
            (
            ComponentType.ReadOnly<Tag_Chunks>(),
            typeof(GridPosition_Data),
            typeof(RenderBounds),
            typeof(RenderMesh),
            typeof(Child)
            //typeof(re)
            );
        cameraEntity = GetSingletonEntity<CameraTag>();
    }

    protected override void OnUpdate()
    {
        #region first Spawn
        viewerPosition = new float2(GetComponent<LocalToWorld>(cameraEntity).Position.x, GetComponent<LocalToWorld>(cameraEntity).Position.z);
        if(!HasComponent<GridLastPosition_Data>(cameraEntity))
        {
            _em.AddComponent<GridLastPosition_Data>(cameraEntity);
            int currentChunkCoordX = (int)math.round(viewerPosition.x / chunkSize);
            int currentChunkCoordY = (int)math.round(viewerPosition.y / chunkSize);
            int numChunksVisible = (int)math.pow(math.mad(chunksVisibleInViewDst, 2, 1), 2); // number of chunks to display, in a starting state we display ALL
            var chunksInViewPosition = new NativeArray<float2>(numChunksVisible, Allocator.TempJob);
            //THIS MEAN NO TERRAIN DISPLAYED!
            ChunkInFieldViewJob startChunksJob = new ChunkInFieldViewJob
            {
                chunksVisibleInViewDstJob = chunksVisibleInViewDst,
                currentChunkCoordXJob = currentChunkCoordX,
                currentChunkCoordYJob = currentChunkCoordY,
                chunksInViewPositionJob = chunksInViewPosition,
            };
            JobHandle jobHandlestartChunks = startChunksJob.Schedule();
            jobHandlestartChunks.Complete();


            //JOB terrain to display
            chunksPosition.Dispose();
        }
        #endregion first Spawn

        float2 LastPos = GetComponent<GridLastPosition_Data>(cameraEntity).value;

        #region Query chunks currently displayed
        EntityQueryDesc chunksDisplayedQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Tag_Chunks>() },
            None = new ComponentType[] { ComponentType.ReadOnly<Disabled>() },
        };

        chunksDisplayedQuery = GetEntityQuery(chunksDisplayedQueryDesc);
        #endregion Query chunks currently displayed

        if (!LastPos.Equals(viewerPosition))
        {
            int currentChunkCoordX = (int)math.round(viewerPosition.x / chunkSize);
            int currentChunkCoordY = (int)math.round(viewerPosition.y / chunkSize);
            Debug.Log($"{currentChunkCoordX}, {currentChunkCoordY}");
        }
        
    }
    /// <summary>
    /// Get chunk that should be displayed
    /// return : chunksInViewPositionJob
    /// </summary>
    [BurstCompile]
    public struct ChunkInFieldViewJob : IJob
    {
        [ReadOnly] public int chunksVisibleInViewDstJob;
        [ReadOnly] public int currentChunkCoordXJob;
        [ReadOnly] public int currentChunkCoordYJob;

        public NativeArray<float2> chunksInViewPositionJob;
        public void Execute()
        {
            int countToAdd = 0;
            for (int y_Offset = -chunksVisibleInViewDstJob; y_Offset <= chunksVisibleInViewDstJob; y_Offset++)
            {
                for (int x_Offset = -chunksVisibleInViewDstJob; x_Offset <= chunksVisibleInViewDstJob; x_Offset++)
                {
                    float2 viewedChunkCoord = new float2(currentChunkCoordXJob + x_Offset, currentChunkCoordYJob + y_Offset);
                    //if (!mapChunksJob.ContainsKey(viewedChunkCoord))
                    //{
                    chunksInViewPositionJob[countToAdd] = viewedChunkCoord;
                    countToAdd++;
                    //}
                }
            }
        }
    }

    public struct ChunksTerrainToCreateJob : IJob
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}

public struct TerrainChunkECS
{

}
        