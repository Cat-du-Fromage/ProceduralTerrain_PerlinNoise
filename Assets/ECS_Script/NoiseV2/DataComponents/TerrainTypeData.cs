using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TerrainTypeData : IComponentData
{
    public FixedString32 name;
    public float height;
    public UnityEngine.Color colour;
}

//deepBlue  : 0,100,255      : 0.3
//coast     : 70,110,170     : 0.4
//sand      : 253,238,115    : 0.45
//plain     : 80,140,40      : 0.55
//forest    : 40,90,30       : 0.6
//Rocklow   : 95,70,65       : 0.7
//RockTop   : 70,50,40       : 0.9
//snow      : 255,255,255    : 1