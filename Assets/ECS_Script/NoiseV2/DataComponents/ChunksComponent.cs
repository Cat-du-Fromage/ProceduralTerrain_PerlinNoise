using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ChunksComponent : IComponentData
{
   public Hash128 test;
}
