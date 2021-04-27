using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
/// <summary>
/// This is Only to test the limit of static class
/// </summary>
public class NoiseSystem : SystemBase
{
    private float test;
    private EntityManager _em;
    private Entity plan;
    private EntityCommandBufferSystem _ecb;
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate(GetEntityQuery(typeof(NoisetestData)));
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _ecb = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        plan = GetSingletonEntity<NoisetestData>();
        _em.AddComponent<NoiseTag>(plan);
        _em.AddComponent<HeightData>(plan);
        _em.SetComponentData(plan, new HeightData { Value = 5 });
        _em.AddComponent<WidthData>(plan);
        _em.SetComponentData(plan, new WidthData { Value = 10 });
        _em.AddComponent<NoiseMapBuffer>(plan);
        _em.SetComponentData(plan, new NoisetestData { width = 3 });
        test = 0;
        /*
        BlobBuilder builder = new BlobBuilder(Allocator.TempJob);
        ref MapSegments mapSegments = ref builder.ConstructRoot<MapSegments>();
        BlobBuilderArray<float> width = builder.Allocate(ref mapSegments.width, 5);
        BlobBuilderArray<float> height = builder.Allocate(ref mapSegments.height, 10);
        for(int y = 0; y < height.Length; y++)
        {
            for (int x = 0; x < width.Length; x++)
            {
                testalloc++;
                UnityEngine.Debug.Log($"float[{x},{y}] = {testalloc}");
            }
        }

        blobAsset = builder.CreateBlobAssetReference<MapSegments>(Allocator.Persistent);
        builder.Dispose();
        */

    }
    protected override void OnUpdate()
    {
        int heightLength = 5;
        int widthLength = 10;
        float Length = _em.GetBuffer<NoiseMapBuffer>(plan).Length;
        if(Length != 0)
        {
            for (int i = 0; i < Length; i++)
            {
                UnityEngine.Debug.Log($"Buffer at position[{i}] == {_em.GetBuffer<NoiseMapBuffer>(plan)[i].value}");
            }
        }
        
        EntityCommandBuffer.ParallelWriter ecb_para = _ecb.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithBurst()
            .WithAll<NoisetestData, NoiseTag>()
            .ForEach((Entity ent, int entityInQueryIndex, ref NoisetestData data, ref DynamicBuffer<NoiseMapBuffer> noiseMap) => 
            {
                //data.width = NoiseUtils.Test(data.width); //seem to no pose any problem here
                //ecb_para.AddComponent(entityInQueryIndex, ent, new NoisetestData {width = NoiseUtils.Test(12)}); // ok here too

                for (int y = 0; y < heightLength; y++)
                {
                    for (int x = 0; x < widthLength; x++)
                    {
                        //noiseMap.Add([math.mad(y,widthLength,x)].value = 3f);
                        noiseMap.Add( new NoiseMapBuffer {value = math.mad(y,widthLength,x)});
                    }
                }
                ecb_para.RemoveComponent<NoiseTag>(entityInQueryIndex, ent);
            }).ScheduleParallel();
        _ecb.AddJobHandleForProducer(Dependency);
    }
}
