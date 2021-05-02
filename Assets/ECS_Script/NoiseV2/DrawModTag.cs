using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
[GenerateAuthoringComponent]
public class DrawModTag : IComponentData
{
    [Range(1,3)]
    public int drawMode;
    public enum displaymode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };
}
