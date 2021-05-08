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
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public float2 offset;
    public int seed;
    [Range(0, 2)]
    public int drawMode;
    public float heightMultiplier;
    [Range(0, 6)]
    public int levelOfDetail;
    /*
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };
    public DrawMode drwMode;
    */
}
