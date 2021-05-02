using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class MapSettingSystem : SystemBase
{
    EntityManager _em;
    MapSettingsTag settingData;

    NativeArray<float> nMapNativeArray;
    NativeArray<Color> colourMapNativeArray;
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<MapSettingsTag>();
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        Entity MapSettingsEntity = GetSingletonEntity<MapSettingsTag>();
        settingData = _em.GetComponentData<MapSettingsTag>(MapSettingsEntity);

        _em.AddComponent<NoiseMapData>(MapSettingsEntity);

        _em.AddComponent<width>(MapSettingsEntity);
        _em.AddComponent<height>(MapSettingsEntity);
        _em.AddComponent<scale>(MapSettingsEntity);
        _em.AddBuffer<noiseMapBuffer>(MapSettingsEntity);
        _em.AddComponent<textureData>(MapSettingsEntity);
        _em.AddComponent<RendererData>(MapSettingsEntity);
        //_em.AddComponent<colourArrayData>(MapSettingsEntity);

        _em.SetComponentData(MapSettingsEntity, new width { value = settingData.width });
        _em.SetComponentData(MapSettingsEntity, new height { value = settingData.height});
        _em.SetComponentData(MapSettingsEntity, new scale { value = settingData.scale });

        //=====================================================================================================
        //NATIVE 
        //=====================================================================================================
        int mapSurface = math.mul(settingData.width, settingData.height);
        nMapNativeArray = new NativeArray<float>(mapSurface, Allocator.TempJob);
        colourMapNativeArray = new NativeArray<Color>(mapSurface, Allocator.TempJob);
        //=====================================================================================================

        //=====================================================================================================
        // NOISE MAP VALUE
        //=====================================================================================================
        float[] nMap = NoiseMono.NoiseMapGen(settingData.width, settingData.height, settingData.scale);

        DynamicBuffer<noiseMapBuffer> nmBuffer = GetBuffer<noiseMapBuffer>(MapSettingsEntity);
        for (int i = 0; i<nMap.Length; i++)
        {
            noiseMapBuffer nMapElement = new noiseMapBuffer();
            nMapElement.value = nMap[i];
            nmBuffer.Add(nMapElement);
            nMapNativeArray[i] = nMap[i];
        }
        //=====================================================================================================

        //=====================================================================================================
        // COLOR MAP VALUE
        //Storing the Data causes massiv lag, maybe better with dynamicBuffer
        //=====================================================================================================
        Color[] colorsMapArray = new Color[mapSurface];
        ColourMapJob colourMapJob = new ColourMapJob
        {
            mWidth = settingData.width,
            mHeight = settingData.height,
            noiseMapJob = nMapNativeArray,
            colorsJob = colourMapNativeArray,
        };
        JobHandle jobHandle = colourMapJob.Schedule();
        jobHandle.Complete();
        colourMapNativeArray.CopyTo(colorsMapArray); //copy the value from NativeArray<T> to a simple Array[]
        //_em.SetComponentData(MapSettingsEntity, new colourArrayData { value = colorsMapArray });
        //=====================================================================================================

        //=====================================================================================================
        // TEXTURE2D
        //=====================================================================================================
        Texture2D texture2D = new Texture2D(settingData.width, settingData.height);
        texture2D.SetPixels(colorsMapArray);
        texture2D.Apply();
        _em.SetComponentData(MapSettingsEntity, new textureData { value = texture2D });
        //=====================================================================================================

        //=====================================================================================================
        //RENDERER
        //=====================================================================================================
        Renderer textureRender = _em.GetComponentData<DataRenderer>(MapSettingsEntity).renderer;
        textureRender.sharedMaterial.mainTexture = texture2D;
        textureRender.transform.localScale = new float3(settingData.width, 1, settingData.height);
        _em.SetComponentData(MapSettingsEntity, new RendererData {value = textureRender });
        //=====================================================================================================
        //still needed in case the program continue to run for some reasons
        nMapNativeArray.Dispose();
        colourMapNativeArray.Dispose();

        _em.RemoveComponent<DataRenderer>(MapSettingsEntity);
        _em.RemoveComponent<MapSettingsTag>(MapSettingsEntity);
    }

    protected override void OnDestroy()
    {
        if(nMapNativeArray.IsCreated)
        {
            nMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }

    protected override void OnStopRunning()
    {
        if (nMapNativeArray.IsCreated)
        {
            nMapNativeArray.Dispose();
        }
        if (colourMapNativeArray.IsCreated)
        {
            colourMapNativeArray.Dispose();
        }
    }
}



/// <summary>
/// Map Colour calculation
/// </summary>
[BurstCompile]
public struct ColourMapJob : IJob
{
    public NativeArray<Color> colorsJob;
    public NativeArray<float> noiseMapJob;
    public int mWidth;
    public int mHeight;
    //public Color[] colors;
    //public float[] noiseMap;
    public void Execute()
    {
        //colors. = new Color[math.mul(mWidth, mHeight)];
        for (int y = 0; y < mWidth; y++)
        {
            for (int x = 0; x < mHeight; x++)
            {
                colorsJob[math.mad(y, mWidth, x)] = Color.Lerp(Color.black, Color.white, noiseMapJob[math.mad(y, mWidth, x)]);
            }
        }
    }
}

