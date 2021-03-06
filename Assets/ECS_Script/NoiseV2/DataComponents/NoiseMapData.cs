using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Animation;
using Unity.Rendering;

public struct NoiseMapData : IComponentData{}

public struct mapChunkSizeData : IComponentData
{
    public int value;
}

public struct mapHeightMultiplierData : IComponentData
{
    public float value;
}

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
/*
public class textureData : IComponentData
{
    public UnityEngine.Texture2D value;
}
*/
public class RendererData : IComponentData
{
    public UnityEngine.Renderer value;
}

public class MeshFilterData : IComponentData
{
    public UnityEngine.MeshFilter value;
}

public class MeshRendererData : IComponentData
{
    public UnityEngine.MeshRenderer value;
}

public struct drawModeData : IComponentData
{
    public int value;
}

public struct levelOfDetailData : IComponentData
{
    public int value;
}
public struct maxViewDistanceData : IComponentData
{
    public float value;
}

public struct chunksVisibleInViewDstData : IComponentData
{
    public int value;
}

public struct TerrainTypeBuffer : IBufferElementData
{
    public float height;
    public MaterialColor colour;
}

public struct BlobTest : IBufferElementData
{
    public BlobAssetReference<NativeHashMap<int,Entity>> height;
}