using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using BovineLabs.Event.Systems;
using BovineLabs.Event.Containers;
/*
public struct RemoveEntityEvent
{
    public Entity entity;
}
[AlwaysUpdateSystem]
public class EventTestSystem : SystemBase
{
    private EventSystem eventSystem;

    EntityQuery entityQuery;
    EntityManager _em;
    Entity _ent;
    protected override void OnCreate()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        this.eventSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventSystem>();
    }
    protected override void OnStartRunning()
    {
        var archetype1 = _em.CreateArchetype(
            ComponentType.ReadOnly<EventDataTest>(),
            ComponentType.ReadWrite<EventDataTestInt>()
            );

        var archetype2 = _em.CreateArchetype(
            ComponentType.ReadOnly<EventDataTest>(),
            ComponentType.ReadWrite<EventDataTestfloat>()
            );

        var archetype3 = _em.CreateArchetype(
            ComponentType.ReadOnly<EventDataTest>(),
            ComponentType.ReadWrite<EventDataTestfloat>(),
            ComponentType.ReadWrite<EventDataTestInt>()
            );

        var ent1 = _em.CreateEntity(archetype1);
        var ent2 = _em.CreateEntity(archetype2);
        var ent3 = _em.CreateEntity(archetype3);
    }
    protected override void OnUpdate()
    {
        
        var queryDescription = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<EventDataTest>(), ComponentType.ReadWrite<EventDataTestfloat>(), ComponentType.ReadOnly<EventDataTestInt>() },
        };
        
        //entityQuery = GetEntityQuery(queryDescription);
        //_ent = entityQuery.GetSingletonEntity();
        Debug.Log("Event Running");
        var writer = eventSystem.CreateEventWriter<RemoveEntityEvent>();

        Entities
            .WithAll<EventDataTest, EventDataTestfloat, EventDataTestInt>()
            .ForEach((Entity entity) =>
            {
                Debug.Log("Event Registered destroyed");
                writer.Write(new RemoveEntityEvent {entity = entity });
            }).ScheduleParallel();

        eventSystem.AddJobHandleForProducer<RemoveEntityEvent>(Dependency);

    }
}
*/
