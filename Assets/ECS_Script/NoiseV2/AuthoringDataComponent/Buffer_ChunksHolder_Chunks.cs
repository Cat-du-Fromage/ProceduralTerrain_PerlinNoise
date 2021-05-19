using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Buffer_ChunksHolder_Chunks : IBufferElementData
{
    public Entity chunk;
}
