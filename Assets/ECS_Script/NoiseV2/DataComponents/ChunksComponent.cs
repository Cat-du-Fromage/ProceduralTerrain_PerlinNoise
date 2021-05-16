using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[Serializable]
public struct Tag_Chunks : IComponentData {}

public struct ViewerPosition : IComponentData
{
    public float3 value;
}

public struct GridPosition_Data : IComponentData
{
    public float2 value;
}

public struct GridLastPosition_Data : IComponentData
{
    public float2 value;
}


