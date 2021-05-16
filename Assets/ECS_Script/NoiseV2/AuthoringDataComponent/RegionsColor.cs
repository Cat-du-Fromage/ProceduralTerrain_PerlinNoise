using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
[GenerateAuthoringComponent]
public struct RegionsColor: IComponentData
{
    /*
    public UnityEngine.Color OceanColor;
    public UnityEngine.Color CoastColor;
    public UnityEngine.Color SandColor;
    public UnityEngine.Color PlainColor;
    public UnityEngine.Color ForestColor;
    public UnityEngine.Color TundraColor;
    public UnityEngine.Color MountainColor;
    public UnityEngine.Color SnowColor;
    */
    public MaterialColor OceanColor; // 0 , 100/255, 255/255, 0
    public MaterialColor CoastColor; // 70/255(0.27), 115/255(0.45), 170/255(0.66), 0
    public MaterialColor SandColor; 
    public MaterialColor PlainColor;
    public MaterialColor ForestColor;
    public MaterialColor TundraColor;
    public MaterialColor MountainColor;
    public MaterialColor SnowColor;
}
