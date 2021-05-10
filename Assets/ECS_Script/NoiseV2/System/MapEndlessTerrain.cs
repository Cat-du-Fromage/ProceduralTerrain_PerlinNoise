using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
/*
public class MapEndlessTerrain : SystemBase
{
    EntityManager _em;
    NativeMultiHashMap<float2, TerrainChunkECS> Terrainchunks;
    NativeArray<float2> chunksPosition;

    int chunksVisibleInViewDst;
    public const float maxViewDistance = 450;
    int chunkSize; // Retrive from System Init Map
    public struct ViewerPosition : IComponentData
    {
        public float2 value;
    }
    protected override void OnCreate()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnStartRunning()
    {
        chunksVisibleInViewDst = (int)math.round(maxViewDistance / chunkSize);
    }

    protected override void OnUpdate()
    {

        //var viewPos = GetComponent<ViewerPosition>().value;
        chunksPosition = new NativeArray<float2>(25, Allocator.TempJob); //25 = (chunkPlayer + 2(chunksVisibleInViewDst))^2 see doc
        Job
            //.WithReadOnly(viewPos)
            .WithCode(() =>
            {
                //int currentChunkCoordX = (int)math.round(viewPos.x / chunkSize);
                //int currentChunkCoordY = (int)math.round(viewPos.y / chunkSize);
                
                //Check all chunks around Player (value depends on max view range)
                for (int y_Offset = -chunksVisibleInViewDst; y_Offset <= chunksVisibleInViewDst; y_Offset++)
                {
                    for (int x_Offset = -chunksVisibleInViewDst; x_Offset <= chunksVisibleInViewDst; x_Offset++)
                    {
                        float2 viewedChunkCoord = new float2(currentChunkCoordX + x_Offset, currentChunkCoordY + y_Offset);
                        chunksPosition[math.mad(y_offset, chunksVisibleInViewDst, x_Offset)] = viewedChunkCoord; // add to linear array (maybe replayce 5 by for because <= on for loop)
                    }
                }
                
            }).Schedule();
        

        
        
        Entities
            .ForEach((ref Translation translation, in Rotation rotation) => 
            {

            }).Schedule();

        chunksPosition.Dispose();
    }
}

public struct TerrainChunkECS
{

}
*/