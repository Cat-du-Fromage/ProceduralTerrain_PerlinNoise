using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public class MapMaterialData : IComponentData
{
    public UnityEngine.Material MatMap;
    public UnityEngine.Material MeshMat;
    public UnityEngine.Mesh mesh;
}