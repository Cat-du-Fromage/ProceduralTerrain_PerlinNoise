using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(TerrainTypeSystem))]
public class MapDisplaySystem : SystemBase
{
    // Retrieve settings DRAWMODE
    NativeArray<float> heightMapNativeArray;
    NativeArray<Color> colourMapNativeArray;
    EntityManager _em;
    Entity mapGenerator;

    //Stopwatch sw;
    protected override void OnCreate()
    {
        var queryDescription = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Event_MapGen_MapDisplay>() },
            None = new ComponentType[] { ComponentType.ReadOnly<Event_MapGen_AddSetData>(), ComponentType.ReadOnly<Event_MapGen_RegionsData>() },
        };
        RequireForUpdate(GetEntityQuery(queryDescription));
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    protected override void OnStartRunning()
    {
        //sw = Stopwatch.StartNew();
        //sw.Start();
        mapGenerator = GetSingletonEntity<NoiseMapData>();
    }
    protected override void OnUpdate()
    {
        int drawmode = GetComponent<drawModeData>(mapGenerator).value;

        int mapWidth = GetComponent<width>(mapGenerator).value;
        int mapHeight = GetComponent<height>(mapGenerator).value;
        int mapSurface = math.mul(mapWidth, mapHeight);

        DynamicBuffer<noiseMapBuffer> heightMap = GetBuffer<noiseMapBuffer>(mapGenerator);
        Color[] colorsMapArray = new Color[mapSurface];

        heightMapNativeArray = heightMap.ToNativeArray(Allocator.TempJob).Reinterpret<float>();
        colourMapNativeArray = new NativeArray<Color>(mapSurface, Allocator.TempJob);
        //=====================================================================================================
        // TEXTURE MAP calculation
        //=====================================================================================================
        if (drawmode == 0)
        {
            TextureMapJob textureMapJob = new TextureMapJob
            {
                mWidth = mapWidth,
                mHeight = mapHeight,
                noiseMapJob = heightMapNativeArray,
                colorsJob = colourMapNativeArray,
            };
            JobHandle jobHandle = textureMapJob.Schedule();
            jobHandle.Complete();
        }
        //=====================================================================================================
        // COLOR MAP calculation
        //=====================================================================================================
        else if (drawmode == 1)
        {
            DynamicBuffer<TerrainTypeBuffer> regionsBuffer = GetBuffer<TerrainTypeBuffer>(mapGenerator);
            ColorMapJob colorMapJob = new ColorMapJob
            {
                mWidth = mapWidth,
                mHeight = mapHeight,
                noiseMapJob = heightMapNativeArray,
                colorsJob = colourMapNativeArray,
                regionsBuffer = regionsBuffer,
            };
            JobHandle jobHandle = colorMapJob.Schedule();
            jobHandle.Complete();
        }
        //=====================================================================================================
        // MESH MAP calculation
        //=====================================================================================================
        else if(drawmode == 2)
        {

        }

        //=====================================================================================================
        // TEXTURE2D applied to the plane (TextureJob and ColorJob)
        //=====================================================================================================
        if (drawmode == 0 || drawmode == 1)
        {
            colourMapNativeArray.CopyTo(colorsMapArray);
            Texture2D texture2D = new Texture2D(mapWidth, mapHeight);
            texture2D.filterMode = FilterMode.Point;
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.SetPixels(colorsMapArray);
            texture2D.Apply();

            Renderer textureRender = _em.GetComponentData<RendererData>(mapGenerator).value;
            textureRender.sharedMaterial.mainTexture = texture2D;
            textureRender.transform.localScale = new float3(mapWidth, 1, mapHeight);
            _em.SetComponentData(mapGenerator, new RendererData { value = textureRender });
        }
        //=====================================================================================================

        #region Event Trigger End
        colourMapNativeArray.Dispose();
        heightMapNativeArray.Dispose();
        _em.RemoveComponent<Event_MapGen_MapDisplay>(GetSingletonEntity<Event_MapGenTag>());
        #endregion Event Trigger End
        //sw.Stop();
        //UnityEngine.Debug.Log($"Elapsed Texture{drawmode} = {sw.Elapsed}");
    }

    /// <summary>
    /// Map Texture calculation (Black and white)
    /// </summary>
    [BurstCompile]
    public struct TextureMapJob : IJob
    {
        public NativeArray<Color> colorsJob;
        public NativeArray<float> noiseMapJob;
        public int mWidth;
        public int mHeight;
        public void Execute()
        {
            for (int y = 0; y < mWidth; y++)
            {
                for (int x = 0; x < mHeight; x++)
                {
                    colorsJob[math.mad(y, mWidth, x)] = Color.Lerp(Color.black, Color.white, noiseMapJob[math.mad(y, mWidth, x)]);
                }
            }
        }
    }
    /// <summary>
    /// Colour Map Cooration colors depends of the regions
    /// </summary>
    [BurstCompile]
    public struct ColorMapJob : IJob
    {
        public NativeArray<Color> colorsJob;
        public NativeArray<float> noiseMapJob;
        public int mWidth;
        public int mHeight;
        public DynamicBuffer<TerrainTypeBuffer> regionsBuffer;
        public void Execute()
        {
            for (int y = 0; y < mHeight; y++)
            {
                for (int x = 0; x < mWidth; x++)
                {
                    float currentHeight = noiseMapJob[math.mad(y, mWidth, x)];
                    for (int i = 0; i < regionsBuffer.Length; i++)
                    {
                        if (currentHeight <= regionsBuffer[i].height)
                        {
                            colorsJob[math.mad(y, mWidth, x)] = regionsBuffer[i].colour;
                            break;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Mesh Generation
    /// </summary>
    [BurstCompile]
    public struct MeshDataJob : IJob
    {
        public int widthJob;
        public int heightJob;
        public int triangleIndex;
        public float4 trianglesVertexPos;

        public NativeArray<float> noiseMapJob;
        public NativeArray<float3> verticesJob;
        public NativeArray<int> trianglesJob;
        public NativeArray<float2> uvsJob;
        public void Execute()
        {
            int vertexIndex = 0;
            float topLeftX = (widthJob - 1) / -2f;
            float topLeftZ = (heightJob - 1) / 2f;

            for (int y = 0; y < heightJob; y++)
            {
                for (int x = 0; x < widthJob; x++)
                {
                    int4 tranglesVertex = new int4(vertexIndex, vertexIndex + widthJob + 1, vertexIndex + widthJob, vertexIndex + 1);

                    verticesJob[vertexIndex] = new float3(topLeftX + x, noiseMapJob[math.mad(y, widthJob, x)], topLeftZ - y);
                    uvsJob[vertexIndex] = new float2(x / (float)widthJob, y / (float)heightJob);

                    if (x < widthJob - 1 && y < heightJob - 1)
                    {
                        trianglesJob[triangleIndex] = tranglesVertex.x;
                        trianglesJob[triangleIndex + 1] = tranglesVertex.y;
                        trianglesJob[triangleIndex + 2] = tranglesVertex.z;
                        triangleIndex += 3;

                        trianglesJob[triangleIndex] = tranglesVertex.y;
                        trianglesJob[triangleIndex + 1] = tranglesVertex.x;
                        trianglesJob[triangleIndex + 2] = tranglesVertex.w;
                        triangleIndex += 3;
                    }
                    vertexIndex++;
                }
            }
        }
    }
    protected override void OnDestroy()
    {
        if (heightMapNativeArray.IsCreated)
        {
            heightMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }

    protected override void OnStopRunning()
    {
        if (heightMapNativeArray.IsCreated)
        {
            heightMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }
}
