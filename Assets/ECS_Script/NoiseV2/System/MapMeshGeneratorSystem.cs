using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
/*
public class MapMeshGeneratorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.
        
        
        
        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
        }).Schedule();
    }

    [BurstCompile]
    public struct MeshDataJob : IJob
    {
        public int widthJob;
        public int heightJob;
        public int triangleIndex;
        public float4 trianglesVertexPos;

        public NativeArray<float> noiseMapJob;
        public NativeArray<float3> verticesJob;
        public NativeArray<int> trianglesJob;
        public NativeArray<float2> uvsJob;
        public void Execute()
        {
            int vertexIndex = 0;
            float topLeftX = (widthJob - 1) / -2f;
            float topLeftZ = (heightJob - 1) / 2f;

            for(int y = 0; y < heightJob; y++)
            {
                for(int x = 0; x < widthJob; x++)
                {
                    int4 tranglesVertex = new int4(vertexIndex, vertexIndex + widthJob + 1, vertexIndex + widthJob, vertexIndex + 1);

                    verticesJob[vertexIndex] = new float3(topLeftX + x, noiseMapJob[math.mad(y,widthJob,x)], topLeftZ - y);
                    uvsJob[vertexIndex] = new float2(x/(float)widthJob, y/(float)heightJob);

                    if(x < widthJob-1 && y < heightJob -1)
                    {
                        trianglesJob[triangleIndex] = tranglesVertex.x;
                        trianglesJob[triangleIndex + 1] = tranglesVertex.y;
                        trianglesJob[triangleIndex + 2] = tranglesVertex.z;
                        triangleIndex += 3;

                        trianglesJob[triangleIndex] = tranglesVertex.y;
                        trianglesJob[triangleIndex + 1] = tranglesVertex.x;
                        trianglesJob[triangleIndex + 2] = tranglesVertex.w;
                        triangleIndex += 3;
                    }
                    vertexIndex++;
                }
            }
        }
    }
}
*/