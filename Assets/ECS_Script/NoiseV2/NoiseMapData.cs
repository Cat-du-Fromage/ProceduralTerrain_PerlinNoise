using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct NoiseMapData : IComponentData{}
[Serializable]
public struct width : IComponentData
{
    public int value;
}
[Serializable]
public struct height : IComponentData
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
