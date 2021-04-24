using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new float3(texture.width, 1, texture.height);
    }
}
