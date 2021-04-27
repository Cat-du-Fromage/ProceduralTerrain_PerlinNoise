using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public class DataRenderer : IComponentData
{
    public UnityEngine.Renderer renderer;
    public UnityEngine.MeshFilter meshFilter;
    public UnityEngine.MeshRenderer meshRenderer;
}
