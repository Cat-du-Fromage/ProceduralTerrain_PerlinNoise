using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if(mapGen.autoUpdate == true)
            {
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
