using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RegionsColor: IComponentData
{
    public UnityEngine.Color OceanColor;
    public UnityEngine.Color CoastColor;
    public UnityEngine.Color SandColor;
    public UnityEngine.Color PlainColor;
    public UnityEngine.Color ForestColor;
    public UnityEngine.Color TundraColor;
    public UnityEngine.Color MountainColor;
    public UnityEngine.Color SnowColor;
}
