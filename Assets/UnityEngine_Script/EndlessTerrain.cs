using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// Purpose:
/// 
/// </summary>
public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;
    public Material mapMaterial;
    public static float2 viewerPosition;
    static MapGenerator mapGenerator;
    public float2 LastPosViewer;

    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<float2, TerrainChunk> terrainChunkDico = new Dictionary<float2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = (int)math.round(maxViewDistance / chunkSize);
        //Debug.Log($"chunksVisibleInViewDst = {chunksVisibleInViewDst}");
    }

    void Update()
    {
        viewerPosition = new float2(viewer.position.x, viewer.position.z);
        if(!viewerPosition.Equals(LastPosViewer) || terrainChunkDico.Count == 0) // for some reason chunks created are disable
        {
            UpdateVisibleChunks();
            LastPosViewer = viewerPosition;
        }
    }
    /// <summary>
    /// Determin when chunks has to be generated on removed depending on the "viewer" position on the map
    /// </summary>
    void UpdateVisibleChunks()
    {
        for(int i = 0; i< terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        //Position relative to chunks(not real coord) (Position/chunksize)
        int currentChunkCoordX = (int)math.round(viewerPosition.x / chunkSize);
        int currentChunkCoordY = (int)math.round(viewerPosition.y / chunkSize);
        //Debug.Log($"currentChunkCoordX = {currentChunkCoordX}, currentChunkCoordY = {currentChunkCoordY}");
        for(int y_Offset = -chunksVisibleInViewDst; y_Offset <= chunksVisibleInViewDst; y_Offset++)
        {
            for (int x_Offset = -chunksVisibleInViewDst; x_Offset <= chunksVisibleInViewDst; x_Offset++)
            {
                float2 viewedChunkCoord = new float2(currentChunkCoordX + x_Offset, currentChunkCoordY + y_Offset);
                //Debug.Log($"viewedChunkCoord = {viewedChunkCoord}");
                if (terrainChunkDico.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDico[viewedChunkCoord].UpdateTerrainChunk();
                    if(terrainChunkDico[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDico[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDico.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }
    #region Terrain CHUNK
    /// <summary>
    /// DATA INVOLVED
    /// Mesh
    /// Position(float2)
    /// Bounds
    /// RenderMesh(meshfilter + MeshRenderer)
    /// Material
    /// 
    /// </summary>
    public class TerrainChunk
    {
        GameObject meshObject;
        float2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        public TerrainChunk(float2 coord, int size, Transform parent, Material material)
        {
            //position = math.mul((float2)size, coord);//CAREFUL TEST NEEDED (making (float2) int == float2(int,int) !!!! float2 is transfomr into int??!!(-480 instead of (-240,-240)
            position = size*coord;
            bounds = new Bounds((Vector2)position, new Vector2(size, size));
            float3 positionV3 = new float3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            //meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            //meshObject.transform.localScale = new float3(size, size, size) / 10; //original: Vector3.one * size / 10f => seems to be equal float3 = 24f; vector3 = 24.0
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            float viewDstFromNearestEdge = math.sqrt(bounds.SqrDistance((Vector2)viewerPosition));
            bool visible = viewDstFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
    #endregion Terrain CHUNK
}
