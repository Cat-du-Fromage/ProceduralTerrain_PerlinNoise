using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct NoiseTag : IComponentData { }

public struct NoiseData : IComponentData
{
    public int mapWidth;
    public BlobArray<float2> test;
}

public struct WidthData : IComponentData
{
    public int Value;
}
public struct HeightData : IComponentData
{
    public int Value;
}

public struct NoiseMapBuffer : IBufferElementData
{
    public float value;
}
