using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace blobTest
{
    [Serializable]
    public struct BlobAssetMapSettings : IComponentData {}

    // This struct is *not* a component
    public struct WaypointBlobs
    {
        public BlobArray<float3> Waypoints;
        public static BlobAssetReference<WaypointBlobs> ConstructBlobdata()
        {
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<WaypointBlobs>();

                var nodearray = builder.Allocate(ref root.Waypoints, 3);

                nodearray[0] = new float3(0, 0, 0);
                nodearray[1] = new float3(100, 100, 100);
                nodearray[2] = new float3(200, 200, 200);

                return builder.CreateBlobAssetReference<WaypointBlobs>(Allocator.Persistent);
            }
        }
    }

    // This struct *is* a component
    public struct WaypointWalker : IComponentData
    {
        public BlobAssetReference<WaypointBlobs> Waypoints;
    }
    public class CreateWaypoints : SystemBase
    {
        // your code here, build a walker etc.
        protected override void OnCreate()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new WaypointWalker
            {
                Waypoints = WaypointBlobs.ConstructBlobdata()
            });
        }
        protected override void OnStartRunning()
        {
            UnityEngine.Debug.Log($"Walker length { GetSingleton<WaypointWalker>().Waypoints.Value.Waypoints.Length} and {GetSingleton<WaypointWalker>().Waypoints.Value.Waypoints[1]} and {GetSingleton<WaypointWalker>().Waypoints.Value.Waypoints[2]}");
        }
        protected override void OnUpdate()
        {/*
            //ConstructBlobdata();
            Entities
                .WithoutBurst()
                .ForEach((in WaypointWalker walker) =>
                {
                    UnityEngine.Debug.Log($"Walker length { walker.Waypoints.Value.Waypoints.Length} and {walker.Waypoints.Value.Waypoints[1]} and {walker.Waypoints.Value.Waypoints[2]}");

                }).Run();
            */
        }
    }

    public class BlobAssetTest
    {
        public static BlobAssetReference<WaypointBlobs> ConstructBlobdata()
        {
            // this using statement makes sure that the builder is disposed at the right time
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<WaypointBlobs>();

                var nodearray = builder.Allocate(ref root.Waypoints, 3);

                nodearray[0] = new float3(0, 0, 0);
                nodearray[1] = new float3(100, 100, 100);
                nodearray[2] = new float3(200, 200, 200);

                return builder.CreateBlobAssetReference<WaypointBlobs>(Allocator.Persistent);
            }
        }
    }
}
