using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct NoiseMapData : IComponentData{}

public struct width : IComponentData
{
    public int value;
}

public struct height : IComponentData
{
    public int value;
}

public struct seed : IComponentData
{
    public int value;
}

public struct scale : IComponentData
{
    public float value;
}

public struct noiseMapBuffer : IBufferElementData
{
    public float value;
}

public struct octavesData : IComponentData
{
    public int value;
}

public struct persistanceData : IComponentData
{
    public float value;
}

public struct lacunarityData : IComponentData
{
    public float value;
}

public struct offsetData : IComponentData
{
    public float2 value;
}

public class textureData : IComponentData
{
    public UnityEngine.Texture2D value;
}
/*
public class colourArrayData : IComponentData
{
    public UnityEngine.Color[] value;
}
*/
public class RendererData : IComponentData
{
    public UnityEngine.Renderer value;
}
