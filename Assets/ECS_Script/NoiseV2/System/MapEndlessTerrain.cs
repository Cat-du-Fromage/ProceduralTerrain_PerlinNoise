using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
/*
public class MapEndlessTerrain : SystemBase
{
    BeginInitializationEntityCommandBufferSystem Begin_init;

    EntityManager _em;
    NativeHashMap<float2, Entity> Terrainchunks;
    NativeArray<float2> chunksPosition;
    EntityArchetype chunkArchetype;

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
    //EntityArchetype chunkArchetype;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<ChunksHolder_Tag>();
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Begin_init = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        chunksVisibleInViewDst = (int)math.round(maxViewDistance / chunkSize);
        
        chunkArchetype = _em.CreateArchetype
            (
            typeof(Tag_Chunks),
            typeof(Tag_NonInitChunk),
            typeof(GridPosition_Data),
            typeof(RenderBounds),
            typeof(RenderMesh),
            typeof(Child)
            );
        
        cameraEntity = GetSingletonEntity<CameraTag>();
    }

    protected override void OnUpdate()
    {
        #region first Spawn
        viewerPosition = new float2(GetComponent<LocalToWorld>(cameraEntity).Position.x, GetComponent<LocalToWorld>(cameraEntity).Position.z);
        if (!HasComponent<GridLastPosition_Data>(cameraEntity))
        {
            _em.AddComponent<GridLastPosition_Data>(cameraEntity);
            int currentChunkCoordX = (int)math.round(viewerPosition.x / chunkSize);
            int currentChunkCoordY = (int)math.round(viewerPosition.y / chunkSize);
            int numChunksVisible = (int)math.pow(math.mad(chunksVisibleInViewDst, 2, 1), 2); // number of chunks to display, in a starting state we display ALL
            var chunksInViewPosition = new NativeArray<float2>(numChunksVisible, Allocator.Persistent);
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
            //Create Chunks Entities
            var chunksHolder = GetSingletonEntity<ChunksHolder_Tag>();
            var ChunkGridPosBuffer = _em.GetBuffer<Buffer_ChunksHolder_Position>(chunksHolder);
            var ChunkEntitiesBuffer = _em.GetBuffer<Buffer_ChunksHolder_Chunks>(chunksHolder);
            //Only set GridPosition for now
            EntityCommandBuffer.ParallelWriter ecb = Begin_init.CreateCommandBuffer().AsParallelWriter();
            
            
            //Entities
            //    .WithDisposeOnCompletion(chunksInViewPosition)
            //    .WithAll<ChunksHolder_Tag>()
            //    .ForEach((Entity ent, int entityInQueryIndex, ref DynamicBuffer<Buffer_ChunksHolder_Position> chunkGridPos, ref DynamicBuffer<Buffer_ChunksHolder_Chunks> chunkEntity) => 
            //    {
            //        for (int i = 0; i < chunksInViewPosition.Length; i++)
            //        {
            //            //DynamicBuffer<Buffer_ChunksHolder_Position> HighlightsBuffer = GetBuffer<Buffer_ChunksHolder_Position>(ent);
            //            //DynamicBuffer<Buffer_ChunksHolder_Chunks> PreselectBuffer = GetBuffer<Buffer_ChunksHolder_Chunks>(ent);
            //            var newChunk = ecb.CreateEntity(entityInQueryIndex, chunkArchetype);
            //            ecb.SetComponent(entityInQueryIndex, newChunk, new GridPosition_Data { value = chunksInViewPosition[i] });
            //            chunkGridPos.Add(new Buffer_ChunksHolder_Position { chunkGridPos = chunksInViewPosition[i] });
            //           chunkEntity.Add(new Buffer_ChunksHolder_Chunks { chunk = newChunk });
            //
             //       }
            //    }).Schedule();
            
            Begin_init.AddJobHandleForProducer(Dependency);
            
            //for (int i = 0; i < chunksInViewPosition.Length; i++)
            //{
            //    var newChunk = _em.CreateEntity(chunkArchetype);
            //   _em.SetComponentData(newChunk, new GridPosition_Data { value = chunksInViewPosition[i] });
            //    ChunkGridPosBuffer.Add(new Buffer_ChunksHolder_Position { chunkGridPos = chunksInViewPosition[i] });
            //    ChunkEntitiesBuffer.Add(new Buffer_ChunksHolder_Chunks { chunk = newChunk });
            //}
            
            //chunksInViewPosition.Dispose();
            //JOB terrain to display
            //chunksPosition.Dispose();
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
        */