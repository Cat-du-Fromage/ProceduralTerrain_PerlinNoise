using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(TerrainTypeSystem))]
public class MapDisplaySystem : SystemBase
{
    // Retrieve settings DRAWMODE
    NativeArray<float> heightMapNativeArray;
    NativeArray<Color> colourMapNativeArray;

    //Mesh Array
    NativeArray<float3> verticesArray;
    NativeArray<int> trianglesArray;
    NativeArray<float2> uvsArray;

    //CurveValue MUST BE REPLACE IN FUTUR BY ANIMATION LIBRARY
    NativeArray<float> curveHeightArray;

    EntityManager _em;
    Entity mapGenerator;

    Stopwatch sw;
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
        sw = Stopwatch.StartNew();
        sw.Start();
        mapGenerator = GetSingletonEntity<NoiseMapData>();
    }
    protected override void OnUpdate()
    {
        int drawmode = GetComponent<drawModeData>(mapGenerator).value;

        int mapWidth = GetComponent<width>(mapGenerator).value;
        int mapHeight = GetComponent<height>(mapGenerator).value;
        int mapSurface = math.mul(mapWidth, mapHeight);

        DynamicBuffer<noiseMapBuffer> heightMap = GetBuffer<noiseMapBuffer>(mapGenerator);
        heightMapNativeArray = heightMap.ToNativeArray(Allocator.TempJob).Reinterpret<float>();
        colourMapNativeArray = new NativeArray<Color>(mapSurface, Allocator.TempJob);

        #region JOB CALCULATION
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
            int lvlDetail = GetComponent<levelOfDetailData>(mapGenerator).value;
            int meshSimplificationIncrement = (lvlDetail == 0) ? 1 : math.mul(lvlDetail, 2);
            int verticesPerLine = ((mapWidth - 1) / meshSimplificationIncrement) + 1;// 1 2 3 4 5 6 7 ... 241 (241-1)/2 (this imply 241 is a const because not all number are pow of 2)
            //Size of the meshes now depending on the level of detail stored (enter when generated for now)
            verticesArray = new NativeArray<float3>(((int)math.pow(verticesPerLine, 2)), Allocator.TempJob);
            trianglesArray = new NativeArray<int>(math.mul((int)math.pow(verticesPerLine - 1, 2), 6), Allocator.TempJob);
            uvsArray = new NativeArray<float2>(((int)math.pow(verticesPerLine, 2)), Allocator.TempJob);


            DynamicBuffer<TerrainTypeBuffer> regionsBuffer = GetBuffer<TerrainTypeBuffer>(mapGenerator);

            //Temporary Solution for animation Curve
            curveHeightArray = new NativeArray<float>(mapSurface, Allocator.TempJob);
            var AnimCurve = _em.GetComponentData<MapHeightCurve>(mapGenerator).value;
            for (int i = 0; i < mapSurface; i++)
            {
                curveHeightArray[i] = AnimCurve.Evaluate(heightMapNativeArray[i]);
            }

            ColorMapJob colorMapJob = new ColorMapJob
            {
                mWidth = mapWidth,
                mHeight = mapHeight,
                noiseMapJob = heightMapNativeArray,
                colorsJob = colourMapNativeArray,
                regionsBuffer = regionsBuffer,
            };
            JobHandle colorJobHandle = colorMapJob.Schedule();

            MeshDataJob meshDataJob = new MeshDataJob
            {
                widthJob = mapWidth,
                heightJob = mapHeight,
                noiseMapJob = heightMapNativeArray,
                verticesJob = verticesArray,
                trianglesJob = trianglesArray,
                uvsJob = uvsArray,
                heightMulJob = GetComponent<mapHeightMultiplierData>(mapGenerator).value,
                curveJob = curveHeightArray,
                levelOfDetailJob = lvlDetail,
                meshSimplificationIncrementJob = meshSimplificationIncrement,
                verticesPerLineJob = verticesPerLine,
            };
            JobHandle meshjobHandle = meshDataJob.Schedule();
            JobHandle.CompleteAll(ref colorJobHandle, ref meshjobHandle);
            curveHeightArray.Dispose();
        }
        #endregion JOB CALCULATION

        //=====================================================================================================
        // TEXTURE2D applied to the plane (TextureJob and ColorJob)
        //=====================================================================================================
        Texture2D texture2D = new Texture2D(mapWidth, mapHeight);
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.SetPixels(colourMapNativeArray.ToArray());
        texture2D.Apply();

        var localToWorldScale = new NonUniformScale
        {
            Value = new float3(mapWidth, mapHeight, mapHeight)
        };

        //float4x4 scaleMesh = float4x4.Scale(mapWidth, mapHeight, mapHeight); //CAREFUL y need to be as big as the other vector points!
        //float4x4 totScale = math.mul(_em.GetComponentData<LocalToWorld>(mapGenerator).Value, scaleMesh);
        _em.SetComponentData(mapGenerator, localToWorldScale);
        if (drawmode == 0 || drawmode == 1)
        {
            var material = _em.GetSharedComponentData<RenderMesh>(mapGenerator).material;
            material.mainTexture = texture2D;
        }
        else if(drawmode == 2)
        {
            Mesh mesh = new Mesh();
            mesh.name = "planePROC";
            mesh.vertices = verticesArray.Reinterpret<Vector3>().ToArray();
            mesh.uv = uvsArray.Reinterpret<Vector2>().ToArray();
            mesh.triangles = trianglesArray.ToArray();
            mesh.RecalculateNormals();
            //other mesh
            /*
            MeshFilter meshFilter = _em.GetComponentData<MeshFilterData>(mapGenerator).value;
            MeshRenderer meshRenderer = _em.GetComponentData<MeshRendererData>(mapGenerator).value;
            Renderer renderer = _em.GetComponentData<RendererData>(mapGenerator).value;
            //renderer.transform.localScale = new float3(texture2D.width, 1, texture2D.height);
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial.mainTexture = texture2D;
            */
            //other mesh
            var material = _em.GetSharedComponentData<RenderMesh>(mapGenerator).material;
            material.mainTexture = texture2D;
            _em.SetSharedComponentData(mapGenerator, new RenderMesh { mesh = mesh , material = _em.GetComponentData<MapMaterialData>(mapGenerator).MeshMat});
            //_em.SetSharedComponentData(mapGenerator, new RenderMesh { material = material, mesh = mesh });

            verticesArray.Dispose();
            trianglesArray.Dispose();
            uvsArray.Dispose();
        }
        //=====================================================================================================

        #region Event Trigger End
        colourMapNativeArray.Dispose();
        heightMapNativeArray.Dispose();
        _em.RemoveComponent<Event_MapGen_MapDisplay>(GetSingletonEntity<Event_MapGenTag>());
        #endregion Event Trigger End
        sw.Stop();
        UnityEngine.Debug.Log($"Elapsed Texture{drawmode} = {sw.Elapsed}");
    }
    #region TEXTURE (Black/White) JOB
    /// <summary>
    /// Map Texture calculation (Black and white)
    /// </summary>
    [BurstCompile]
    public struct TextureMapJob : IJob
    {
        public NativeArray<Color> colorsJob;
        [ReadOnly] public NativeArray<float> noiseMapJob;
        [ReadOnly] public int mWidth;
        [ReadOnly] public int mHeight;
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
    #endregion TEXTURE (Black/White) JOB

    #region COLORS MAP JOB
    /// <summary>
    /// Colour Map Cooration colors depends of the regions
    /// </summary>
    [BurstCompile]
    public struct ColorMapJob : IJob
    {
        public NativeArray<Color> colorsJob;
        [ReadOnly] public NativeArray<float> noiseMapJob;
        [ReadOnly] public int mWidth;
        [ReadOnly] public int mHeight;
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
    #endregion COLOR MAP JOB

    #region MESH JOB
    /// <summary>
    /// Mesh Generation
    /// </summary>
    [BurstCompile]
    public struct MeshDataJob : IJob
    {
        [ReadOnly] public int widthJob;
        [ReadOnly] public int heightJob;
        [ReadOnly] public NativeArray<float> noiseMapJob;
        [ReadOnly] public float heightMulJob;
        [ReadOnly] public NativeArray<float> curveJob;

        //Terrain Complexity(increase/Decrease
        [ReadOnly] public int levelOfDetailJob;
        [ReadOnly] public int meshSimplificationIncrementJob;
        [ReadOnly] public int verticesPerLineJob;

        public NativeArray<float3> verticesJob;
        public NativeArray<int> trianglesJob;
        public NativeArray<float2> uvsJob;
        public void Execute()
        {
            int triangleIndex = 0;
            
            float topLeftX = (widthJob - 1) / -2f;
            float topLeftZ = (heightJob - 1) / 2f;

            int vertexIndex = 0;
            for (int y = 0; y < heightJob; y+= meshSimplificationIncrementJob)
            {
                for (int x = 0; x < widthJob; x+= meshSimplificationIncrementJob)
                {
                    int4 tranglesVertex = new int4(vertexIndex, vertexIndex + verticesPerLineJob + 1, vertexIndex + verticesPerLineJob, vertexIndex + 1);

                    //int linearIndex = math.mad(y, widthJob, x); // Index in a Linear Array
                    //float curveValue = animCurveJob.Evaluate(noiseMapJob[linearIndex]); //Value after evaluation in the animation Curve
                    verticesJob[vertexIndex] = new float3(topLeftX + x, math.mul(curveJob[math.mad(y, widthJob, x)], heightMulJob), topLeftZ - y);

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
    #endregion MESH JOB
    protected override void OnDestroy()
    {
        if (heightMapNativeArray.IsCreated)
            heightMapNativeArray.Dispose();
        if (colourMapNativeArray.IsCreated)
            colourMapNativeArray.Dispose();
        if (verticesArray.IsCreated)
            verticesArray.Dispose();
        if (trianglesArray.IsCreated)
            trianglesArray.Dispose();
        if (uvsArray.IsCreated)
            uvsArray.Dispose();
    }
    protected override void OnStopRunning()
    {
        if (heightMapNativeArray.IsCreated)
            heightMapNativeArray.Dispose();
        if (colourMapNativeArray.IsCreated)
            colourMapNativeArray.Dispose();
        if (verticesArray.IsCreated)
            verticesArray.Dispose();
        if (trianglesArray.IsCreated)
            trianglesArray.Dispose();
        if (uvsArray.IsCreated)
            uvsArray.Dispose();
    }

    /*
    /// <summary>
    /// THANKS ARGON!!!
    /// </summary>
    public struct SampledAnimationCurve : System.IDisposable
    {
        NativeArray<float> sampledFloat;
        /// <param name="samples">Must be 2 or higher</param>
        public SampledAnimationCurve(AnimationCurve ac, int samples)
        {
            sampledFloat = new NativeArray<float>(samples, Allocator.Persistent);
            float timeFrom = ac.keys[0].time;
            float timeTo = ac.keys[ac.keys.Length - 1].time;
            float timeStep = (timeTo - timeFrom) / (samples - 1);

            for (int i = 0; i < samples; i++)
            {
                sampledFloat[i] = ac.Evaluate(timeFrom + (i * timeStep));
            }
        }

        public void Dispose()
        {
            sampledFloat.Dispose();
        }

        /// <param name="time">Must be from 0 to 1</param>
        public float EvaluateLerp(float time)
        {
            int len = sampledFloat.Length - 1;
            float clamp01 = time < 0 ? 0 : (time > 1 ? 1 : time);
            float floatIndex = (clamp01 * len);
            int floorIndex = (int)math.floor(floatIndex);
            if (floorIndex == len)
            {
                return sampledFloat[len];
            }

            float lowerValue = sampledFloat[floorIndex];
            float higherValue = sampledFloat[floorIndex + 1];
            return math.lerp(lowerValue, higherValue, math.frac(floatIndex));
        }
    }
    */
}
