using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 300;
    public Transform view;

    public static float2 viewerPosition;

    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<float2, TerrainChunk> terrainChunkDico = new Dictionary<float2, TerrainChunk>();

    // Start is called before the first frame update
    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = (int)math.round(maxViewDistance / chunkSize);
    }

    void UpdateVisibleChunks()
    {
        //Position relative to chunks(not real coord) (Position/chunksize)
        int currentChunkCoordX = (int)math.round(viewerPosition.x / chunkSize);
        int currentChunkCoordY = (int)math.round(viewerPosition.y / chunkSize);

        for(int y_Offset = -chunksVisibleInViewDst; y_Offset <= chunksVisibleInViewDst; y_Offset++)
        {
            for (int x_Offset = -chunksVisibleInViewDst; x_Offset <= chunksVisibleInViewDst; x_Offset++)
            {
                float2 viewedChunkCoord = new float2(currentChunkCoordX + x_Offset, currentChunkCoordY + y_Offset);
                if(terrainChunkDico.ContainsKey(viewedChunkCoord))
                {

                }
                else
                {
                    //terrainChunkDico.Add(viewedChunkCoord, new TerrainChunk());
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        float2 position;
        Bounds bounds;

        public TerrainChunk(float2 coord, int size)
        {
            position = math.mul((float2)size, coord);//CAREFUL TEST NEEDED
            bounds = new Bounds((Vector2)position, new Vector2(size, size));
            float3 positionV3 = new float3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = new float3(size, size, size) / 10; //original: Vector3.one * size / 10f
        }

        public void Update()
        {
            float viewDstFromNearestEdge = math.sqrt(bounds.SqrDistance((Vector2)viewerPosition));
            bool visible = viewDstFromNearestEdge <= maxViewDistance;
        }

        public void setVisible()
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
