using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        //first take length of the grid (width and height)
        int width = noiseMap.GetLength(0); // 0 = array[0]
        int height = noiseMap.GetLength(1); // 1 = array[1]

        Texture2D texture = new Texture2D(width,height);

        //====================================
        //COLOR IS DEFINED HERE
        //====================================
        Color[] colourMap = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]); // y * width gives row we currently in (in Color[]) and + x gives the column
            }
        }
        texture.SetPixels(colourMap);
        texture.Apply();
        //====================================

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new float3(width, 1, height);
    }
    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
