using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
[GenerateAuthoringComponent]
public struct MapSettingsTag : IComponentData
{
    public int width;
    public int height;
    public float scale;
    [Range(0, 2)]
    public int drawMode;
}
