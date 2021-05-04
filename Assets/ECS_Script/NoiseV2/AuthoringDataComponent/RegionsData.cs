using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RegionsData: IComponentData
{
    public float Ocean;
    public float Coast;
    public float Sand;
    public float Plain;
    public float Forest;
    public float Tundra;
    public float Mountain;
    public float Snow;
}
