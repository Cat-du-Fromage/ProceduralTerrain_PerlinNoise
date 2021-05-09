using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using BovineLabs.Event.Systems;
using BovineLabs.Event.Containers;
using UnityEngine;
/*
public class EntityRemovalSystem : ConsumeSingleEventSystemBase<RemoveEntityEvent>
{
    protected override void OnEvent(RemoveEntityEvent evnt)
    {
        EntityManager.DestroyEntity(evnt.entity);
        Debug.Log("Entity destroyed");
    }
}
*/