using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Animation;
using Unity.DataFlowGraph;

[GenerateAuthoringComponent]
public class MapHeightCurve : IComponentData
{
    public UnityEngine.AnimationCurve value;
}
